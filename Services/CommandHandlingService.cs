using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FezBotRedux.Common.Extensions;
using FezBotRedux.Common.Models;
using FezBotRedux.Common.Types;
using FezBotRedux.Common.Utility;
using FezBotRedux.Modules.General;
using FezBotRedux.Modules.Fun;

namespace FezBotRedux.Services
{
    class CommandHandler
    {
        private DiscordSocketClient _client;
        private CommandService _cmds;
        private IServiceProvider _services;

        public async Task Install(DiscordSocketClient c, IServiceProvider s)
        {
            _cmds = new CommandService(new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false,
                LogLevel = LogSeverity.Verbose
            });
            _client = c;
            _services = s;
            _client.MessageReceived += HandleCommand;
            _cmds.CommandExecuted += _cmds_CommandExecuted;
            await _cmds.AddModulesAsync(Assembly.GetEntryAssembly(), s);
        }

        private async Task _cmds_CommandExecuted(Optional<CommandInfo> info, ICommandContext cmdContext, IResult cmdResult)
        {
            if (!cmdResult.IsSuccess)
            {
                var embed = NeoEmbeds.Error(cmdResult.ErrorReason, cmdContext.User, cmdResult.Error != null ? cmdResult.Error.ToString() : "Error").Build();
                await cmdContext.Channel.SendMessageAsync("", false, embed);
            }
        }

        private async Task CheckAfk(SocketUserMessage msg)
        {
            using (var db = new NeoContext())
            {
                if (msg.MentionedUsers.Any() && msg.MentionedUsers.FirstOrDefault() != msg.Author)
                {
                    var user = msg.MentionedUsers.FirstOrDefault();
                    if (db.Afks.Any(afk => afk.User == db.Users.FirstOrDefault(u => u.Id == user.Id)))
                    {
                        var obj = db.Afks.FirstOrDefault(afk => afk.User == db.Users.FirstOrDefault(u => u.Id == user.Id));
                        var reason = obj.Reason;
                        var time = obj.Time;
                        if (reason != null)
                        {
                            if (time != default)
                            {
                                var left = time - DateTime.Now;
                                var embed = NeoEmbeds.Afk($"{user.Username} is afk and can't asnwer you now!", user, reason, left < new TimeSpan(0, 0, 0) ? "Expired" : General.Thing(left));
                                await msg.Channel.SendMessageAsync("", false, embed.Build());
                            }
                            else
                            {
                                var embed = NeoEmbeds.Afk($"{user.Username} is afk and can't asnwer you now!", user, reason);
                                await msg.Channel.SendMessageAsync("", false, embed.Build());
                            }
                        }
                        else
                        {
                            if (time != default(DateTime))
                            {
                                var left = time - DateTime.Now;
                                var embed = NeoEmbeds.Afk($"{user.Username} is afk and can't asnwer you now!", user, null, left < new TimeSpan(0, 0, 0) ? "Expired" : General.Thing(left));
                                await msg.Channel.SendMessageAsync("", false, embed.Build());
                            }
                            else
                            {
                                var embed = NeoEmbeds.Afk($"{user.Username} is afk and can't asnwer you now!", user);
                                await msg.Channel.SendMessageAsync("", false, embed.Build());
                            }
                        }
                    }
                }
            }
        }

        private async Task HandleTag(SocketUserMessage e)
        {
            using (var db = new NeoContext())
            {
                if (!db.Tags.Any(t =>
                    t.Trigger == e.Content))
                {
                    return;
                }
                var obj = db.Tags.FirstOrDefault(t => t.Trigger == e.Content);
                if (obj.IfAttachment)
                {
                    await NeoConsole.Log(LogSeverity.Critical, "TAG", obj.Value);
                    //await e.Channel.SendFileAsync(obj.Value,"henlo");
                    await e.Channel.SendMessageAsync("", false, NeoEmbeds.Picture(obj.Value, e.Author));

                }
                else
                {
                    await e.Channel.SendMessageAsync(obj.Value);
                }
                db.SaveChanges();
            }
        }

        private async Task HandleCommand(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null) return;                        // Check if the received message is from a user.
            var context = new SocketCommandContext(_client, msg);     // Create a new command context.

            if (context.User.IsBot) return;
            var prefix = Configuration.Load().Prefix;

            using (var db = new NeoContext())
            {
                if (!db.Users.Any(x => x.Id == msg.Author.Id)) //if user does net exist in database create it
                {
                    var newobj = new DbUser
                    {
                        Id = msg.Author.Id
                    };
                    db.Users.Add(newobj);
                    db.SaveChanges();
                }
                if (!(msg.Channel is ISocketPrivateChannel) && db.Guilds.Any(x => x.Id == (msg.Channel as IGuildChannel).GuildId))
                {
                    prefix = db.Guilds.FirstOrDefault(x => x.Id == (msg.Channel as IGuildChannel).GuildId).Prefix ?? Configuration.Load().Prefix;
                }
                if (db.Blacklist.Any(bl => bl.User == db.Users.FirstOrDefault(u => u.Id == context.User.Id)))
                {
                    return;
                }
            }

            await CheckAfk(msg);
            await HandleTag(msg);
            await NeoConsole.Log(msg);

            var argPos = 0;                                     // Check if the message has either a string or mention prefix.
            if (msg.HasStringPrefix(prefix, ref argPos) ||
                msg.HasStringPrefix(Configuration.Load().Prefix, ref argPos) ||
                msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                IResult result; // Try and execute a command with the given context.
                try
                {
                    result = await _cmds.ExecuteAsync(context, argPos, _services);
                    if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    {
                        var embed = NeoEmbeds.Error(result.ErrorReason, context.User, result.Error != null ? result.Error.ToString() : "Error").Build();
                        await context.Channel.SendMessageAsync("", false, embed);
                    }
                }
                catch (Exception ex)
                {
                    var embed = NeoEmbeds.Minimal(ex.Message).AddField("inner", ex.InnerException?.Message ?? "nope");
                    await NeoConsole.NewLine(ex.StackTrace);
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }            
        }
    }
}
