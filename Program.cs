using Discord.WebSocket;
using Discord;

using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

using FezBotRedux.Common.Utility;
using FezBotRedux.Common.Types;
using FezBotRedux.Common.Models;
using System.Linq;
using FezBotRedux.Common.Extensions;
using FezBotRedux.Services;
using Discord.Addons.Interactive;

namespace FezBotRedux
{
    internal class Program
    {
        private static void Main() => new Program().Run().GetAwaiter().GetResult();
        private DiscordSocketClient _client;
        private IServiceProvider _services;

        private Timer _playingTimer = null;
        private AutoResetEvent _autoEvent = null;

        private static async Task PrintInfoAsync()
        {
            var art = new[] {
                @"███████╗███████╗███████╗██████╗  ██████╗ ████████╗██████╗ ███████╗██████╗ ██╗   ██╗██╗  ██╗",
                @"██╔════╝██╔════╝╚══███╔╝██╔══██╗██╔═══██╗╚══██╔══╝██╔══██╗██╔════╝██╔══██╗██║   ██║╚██╗██╔╝",
                @"█████╗  █████╗    ███╔╝ ██████╔╝██║   ██║   ██║   ██████╔╝█████╗  ██║  ██║██║   ██║ ╚███╔╝ ",
                @"██╔══╝  ██╔══╝   ███╔╝  ██╔══██╗██║   ██║   ██║   ██╔══██╗██╔══╝  ██║  ██║██║   ██║ ██╔██╗ ",
                @"██║     ███████╗███████╗██████╔╝╚██████╔╝   ██║   ██║  ██║███████╗██████╔╝╚██████╔╝██╔╝ ██╗",
                @"╚═╝     ╚══════╝╚══════╝╚═════╝  ╚═════╝    ╚═╝   ╚═╝  ╚═╝╚══════╝╚═════╝  ╚═════╝ ╚═╝  ╚═╝",
                @"                                                                                           "
            };
            foreach (var line in art)
                await NeoConsole.NewLineArt(line, ConsoleColor.DarkMagenta);
            await NeoConsole.Append("\n        +-------------------------------------------------------------------------+", ConsoleColor.Gray);
            await NeoConsole.Append("\n           Source Code(Github Repo): https://github.com/EchelonDev/NeoKapcho/      ", ConsoleColor.Yellow);
            await NeoConsole.Append("\n                 Build with love by 48 | Powered by SQLite and EFCore              ", ConsoleColor.Red);
            await NeoConsole.Append("\n        +-------------------------------------------------------------------------+", ConsoleColor.Gray);

        }
        private async Task Run()
        {
            Console.OutputEncoding = Encoding.Unicode;
            await PrintInfoAsync();
            using (var db = new NeoContext())
            {
                db.Database.EnsureCreated();
            }
            EnsureConfigExists();

            _client = new DiscordSocketClient(new DiscordSocketConfig {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 5000
            });

            await _client.LoginAsync(TokenType.Bot, Configuration.Load().Token);
            await NeoConsole.Append("Logging in...",ConsoleColor.Cyan);
            await _client.StartAsync();
            await NeoConsole.NewLine("Success...", ConsoleColor.Cyan);

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton<CommandHandler>()
                .AddSingleton<InteractiveService>()
                .BuildServiceProvider();
            await NeoConsole.NewLine("Activating Services...\nDone.", ConsoleColor.Cyan);

            await _services.GetRequiredService<CommandHandler>().Install(_client, _services);
            await NeoConsole.NewLine("Command Handler starting...\nDone.\n", ConsoleColor.Cyan);

            _client.Log += Log;
            _client.Ready += _client_Ready;

            await Task.Delay(-1);
        }

        private Task _client_Ready()
        {
            Task.Run(() =>
            {
                foreach (var guild in _client.Guilds)
                {
                    using (var db = new NeoContext())
                    {
                        if (db.Guilds.Any(x => x.Id == guild.Id)) continue;
                        var newobj = new Guild
                        {
                            Id = guild.Id,
                            Prefix = null
                        };
                        db.Guilds.Add(newobj);
                        db.SaveChanges();
                        Console.WriteLine($"Guild Added : {guild.Name}");
                    }
                }
            });

            Task.Run(async () =>
            {
                using (var db = new NeoContext())
                {
                    foreach (var hub in db.NeoHubSettings)
                    {
                        var m = await (_client.GetChannel(hub.ChannelId) as ITextChannel)
                            .GetMessageAsync(hub.MsgId);
                        await (m as IUserMessage).ModifyAsync(x => x.Embed = NeoEmbeds.Information(_client));
                    }
                }
            });

            _autoEvent = new AutoResetEvent(false);
            _playingTimer = new Timer(ChangePlayingAsync, _autoEvent, 0, 1000 * 60 * 5);
            Task.Run(async () =>
            {
                using (var db = new NeoContext())
                {
                    if (!db.Playings.Any()) return;
                    var game = await db.Playings.AsAsyncEnumerable().OrderBy(o => Guid.NewGuid())
                        .FirstOrDefaultAsync();
                    await _client.SetGameAsync(game.Name);
                }
            });
            return Task.CompletedTask;
        }
        private async void ChangePlayingAsync(object stateInfo)
        {
            using (var db = new NeoContext())
            {
                if (!db.Playings.Any()) return;
                var game = await db.Playings.AsAsyncEnumerable().OrderBy(o => Guid.NewGuid())
                    .FirstOrDefaultAsync();
                await _client.SetGameAsync(game.Name);
            }
        }
        private static Task Log(LogMessage l)
        {
            Task.Run(async ()
                => await NeoConsole.Log(l.Severity, l.Source, l.Exception?.ToString() ?? l.Message));
            return Task.CompletedTask;
        }

        private static async void EnsureConfigExists()
        {
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "data")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "data"));

            var loc = Path.Combine(AppContext.BaseDirectory, "data/configuration.json");

            if (!File.Exists(loc) || Configuration.Load().Token == null)
            {
                await NeoConsole.NewLine("Configuration not found...\nCreating...", ConsoleColor.Cyan);
                var config = new Configuration();
                await NeoConsole.NewLine("Token :", ConsoleColor.DarkCyan);
                config.Token = Console.ReadLine();
                await NeoConsole.NewLine("Default Prefix :", ConsoleColor.Cyan);
                config.Prefix = Console.ReadLine();               
                config.Save();
            }
            else
            {
                await NeoConsole.NewLine("Configuration found...\nLoading...", ConsoleColor.Cyan);
            }

        }
    }
}
