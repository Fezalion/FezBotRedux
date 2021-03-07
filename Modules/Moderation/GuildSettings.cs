using Discord.Commands;
using FezBotRedux.Common.Attributes;
using FezBotRedux.Common.Enums;
using FezBotRedux.Common.Extensions;
using FezBotRedux.Common.Models;
using System.Linq;
using System.Threading.Tasks;

namespace FezBotRedux.Modules.GuildSettings {
    [Name("Guild"), Summary("contains the guild settings commands.")]
    public class GuildSettings : ModuleBase<SocketCommandContext> {
        [Group("prefix"), Alias("pfx"), Name("Guild Prefix")]
        public class GuildPrefixSub : ModuleBase<SocketCommandContext> {
            [Command("")]
            [Remarks("Sets custom prefix.")]
            [MinPermissions(AccessLevel.ServerAdmin)]
            public async Task Prefix([Summary("Prefix")] string prefix) {
                if (prefix.Length < 0 || prefix.Length > 15) {
                    var embed = NeoEmbeds.Error("Prefix length can't be less than 0 or bigger than 15.", Context.User);
                    await ReplyAsync("", false, embed.Build());
                } else {
                    using (var db = new NeoContext()) {
                        var obj = db.Guilds.FirstOrDefault(g => g.Id == Context.Guild.Id);
                        obj.Prefix = prefix;
                        db.Update(obj);
                        db.SaveChanges();
                        var embed = NeoEmbeds.Success($"Prefix of `{Context.Guild.Name}` has been set to `{prefix}`.", Context.User);
                        await ReplyAsync("", false, embed.Build());
                    }
                }
            }

            [Command("")]
            [Remarks("Gets custom prefix of the server.")]
            [MinPermissions(AccessLevel.User)]
            public async Task Prefix() {
                string prefix;
                using (var db = new NeoContext()) {
                    var obj = db.Guilds.FirstOrDefault(g => g.Id == Context.Guild.Id);
                    prefix = obj.Prefix;
                }

                if (prefix == null) {
                    var embed = NeoEmbeds.Error("This guilds custom prefix is not set yet.", Context.User);
                    await ReplyAsync("", false, embed.Build());
                } else {
                    var embed = NeoEmbeds.Success($"Prefix of `{Context.Guild.Name}` is `{prefix}`", Context.User);
                    await ReplyAsync("", false, embed.Build());
                }
            }
        }
    }
}

