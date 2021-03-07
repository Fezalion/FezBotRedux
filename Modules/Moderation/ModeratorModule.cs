using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FezBotRedux.Common.Attributes;
using FezBotRedux.Common.Enums;
using FezBotRedux.Common.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace FezBotRedux.Modules.Moderation {
    [Name("Moderator"), Summary("contains moderating commands for server mods.")]
    [RequireContext(ContextType.Guild)]
    [MinPermissions(AccessLevel.ServerMod)]
    public class ModeratorModule : ModuleBase<SocketCommandContext> {
        [Command("kick")]
        [Remarks("Kick the specified user.")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task Kick([Summary("User Mention")][Remainder] SocketGuildUser user) {
            await ReplyAsync($"cya {user.Mention} :wave:");
            await user.KickAsync();
        }

        [Command("ban")]
        [Remarks("Ban the specified user.")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task Ban([Summary("User Mention")][Remainder] SocketGuildUser user) {
            await ReplyAsync($"There won't be a next time... :cry: {user.Username} :wave:");
            await Context.Guild.AddBanAsync(user);
        }

        [Command("prune")]
        [Remarks("Clear bots recent messages.")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task Clearm() {
            var self = Context.Client.CurrentUser;

            var messages = (await Context.Channel.GetMessagesAsync().FlattenAsync()).AsEnumerable();
            messages = messages.Where(x => x.Author.Id == self.Id);
            //TODO await Context.Channel.DeleteMessagesAsync(messages);

            var embed = NeoEmbeds.Success($"Deleted {messages.Count()} messages.", Context.User);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("prune")]
        [Remarks("Clear a users recent messages.")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task Clearm([Summary("User Mention")] IUser user) {
            var messages = (await Context.Channel.GetMessagesAsync().FlattenAsync()).AsEnumerable();
            messages = messages.Where(x => x.Author.Id == user.Id);
            //TODO await Context.Channel.DeleteMessagesAsync(messages);

            var embed = NeoEmbeds.Success($"Deleted {messages.Count()} messages of {user.Username}.", Context.User);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("prune")]
        [Remarks("Clear a users recent messages.")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task Clearm([Summary("User Mention")] IUser user, [Summary("Message Amount")] int amount) {
            var messages = (await Context.Channel.GetMessagesAsync(amount < 100 ? amount : 100).FlattenAsync()).AsEnumerable();
            messages = messages.Where(x => x.Author.Id == user.Id);
            //TODO await Context.Channel.DeleteMessagesAsync(messages);

            var embed = NeoEmbeds.Success($"Deleted {messages.Count()} messages of {user.Username}.", Context.User);
            await ReplyAsync("", false, embed.Build());
        }
    }
}
