using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using FezBotRedux.Common.Attributes;
using FezBotRedux.Common.Enums;
using FezBotRedux.Common.Extensions;
using FezBotRedux.Common.Utility;
using FezBotRedux.Services;

namespace FezBotRedux.Modules.Fun
{
    [Name("Fun"), Summary("contains the fun commands.")]
    public class Fun : ModuleBase<SocketCommandContext>
    {
        [Command("s2mkaccm")]
        [Remarks("Subject to change, at work.")]
        [MinPermissions(AccessLevel.User)]
        public async Task GetLongevity()
        {
            var u = Context.User;
            var x = LongevityFormula(
                u.CreatedAt.Millisecond,
                (int)u.DiscriminatorValue,
                u.Activity?.Name.Length ?? 0,
                u.Username.Length,
                (int)u.Status
            );
            var y = "";
            for (var i = 0; i <= x; i++)
            {
                y += '=';
            }
            if (u.Username == "48")
            {
                y = y + y;
            }
            var builder = NeoEmbeds.Minimal($"Ɛ{y}>");
            await ReplyAsync("", false, builder.Build());
        }
        private static double LongevityFormula(params int[] form)
        {
            return form.Average() % 20;
        }

        [Command("ask")]
        [Remarks("Ask a question.")]
        [MinPermissions(AccessLevel.User)]
        public async Task AAQ([Remainder, Summary("Text")] string arg)
        {
            string owo = new Random().Next(0, 10) >= 5 ? "I think so, yes." : "No, nope!";
            await ReplyAsync(owo);
        }

        [Command("retard")]
        [Remarks("Retardation of given string.")]
        [MinPermissions(AccessLevel.User)]
        public async Task Retard([Remainder, Summary("Text")] string arg)
        {
            bool x = false;
            string owo = "";
            string[] blobs = arg.Split(' ');
            if (arg.Contains("e.")) return;
            foreach (string blob in blobs)
            {
                x = false;
                foreach (char blib in blob)
                {
                    if ((x = !x))
                    {
                        owo += blib.ToString().ToUpper().ToCharArray()[0];
                    }
                    else
                    {
                        owo += blib.ToString().ToLower().ToCharArray()[0];
                    }
                }
                owo += " ";
            }
            await ReplyAsync($":wheelchair: {owo}");
        }
        public async Task Retard(IMessage msg, IUser user)
        {
            bool x = false;
            string owo = "";
            string[] blobs = msg.Content.Split(' ');
            if (msg.Content.Contains("e.")) return;
            foreach (string blob in blobs)
            {
                x = false;
                foreach (char blib in blob)
                {
                    if ((x = !x))
                    {
                        owo += blib.ToString().ToUpper().ToCharArray()[0];
                    }
                    else
                    {
                        owo += blib.ToString().ToLower().ToCharArray()[0];
                    }
                }
                owo += " ";
            }
            await ReplyAsync($"`{user.Username} at {msg.Timestamp.ToString("dd/MM/yy hh:mm:ss")}`\n:wheelchair: {owo}");
        }


        [Command("retard")]
        [Remarks("Retardation of given user.")]
        [MinPermissions(AccessLevel.User)]
        [RequireContext(ContextType.Guild)]
        public async Task Retard([Summary("User Mention")] IUser user = null)
        {
            try
            {
                var lastmessages = await Context.Channel.GetMessagesAsync(500, CacheMode.AllowDownload).FlattenAsync();
                var userlast = lastmessages.Where(x => !x.Content.Contains("e.") && x.Author != Context.Client.CurrentUser).FirstOrDefault(x => x.Author == user);
                await Retard(userlast.Content);
            }
            catch (NullReferenceException e)
            {
                await NeoConsole.Log(LogSeverity.Error, "FunModule", "Error getting last messages of user, outofcache");
            }
        }
        [Command("rndretard")]
        [Remarks("Random retardation in the channel")]
        [MinPermissions(AccessLevel.User)]
        [RequireContext(ContextType.Guild)]
        public async Task Retard()
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            var lastmessages = await Context.Channel.GetMessagesAsync(500, CacheMode.AllowDownload).FlattenAsync();
            var user = lastmessages.Where(x => !x.Content.Contains("f!") && x.Author != Context.Client.CurrentUser);
            var userat = user.ElementAt(rnd.Next(1, user.Count())).Author;
            var userlast = lastmessages.Where(x => !x.Content.Contains("f!") && x.Author != Context.Client.CurrentUser).FirstOrDefault(x => x.Author == userat);
            await Retard(userlast, userat);
        }
    }
}
