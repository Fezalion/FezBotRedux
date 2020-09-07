using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using NCalc;
using FezBotRedux.Common.Attributes;
using FezBotRedux.Common.Enums;
using FezBotRedux.Common.Extensions;
using FezBotRedux.Common.Models;
using FezBotRedux.Common.Types;


namespace FezBotRedux.Modules.Owner
{
    [Name("Owner"), Summary("contains owner commands."), MinPermissions(AccessLevel.BotOwner)]
    public class OwnerModule : ModuleBase<SocketCommandContext>
    {
        private static InteractiveService S { get; set; }

        [Command("leave")]
        [Remarks("Leaves given guild.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task LeaveGuild([Summary("guild")][Remainder] string guild = null)
        {
            ulong id;
            if (string.IsNullOrWhiteSpace(guild)) //if null leave current
            {
                await Context.Guild.LeaveAsync();
            }
            else if (ulong.TryParse(guild, out id)) //if its a id
            {
                if (Context.Client.Guilds.Any(x => x.Id == id))
                {
                    await Context.Client.GetGuild(id).LeaveAsync();
                }
            }
            else if (guild == "all")
            {
                foreach (var g in Context.Client.Guilds)
                {
                    await g.LeaveAsync();
                    await Task.Delay(500);
                }
            }
            else //if its a name
            {
                if (Context.Client.Guilds.Any(x => x.Name == guild))
                {
                    await Context.Client.Guilds.FirstOrDefault(x => x.Name == guild).LeaveAsync();
                }
            }
        }

        [Command("information")]
        [Remarks("Post an information text to information channel.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task PostInformation()
        {
            using (var db = new NeoContext())
            {
                var chan = Context.Channel.Id;
                if (!db.NeoHubSettings.Any(x => x.ChannelId == chan))
                {
                    var set = new NeoHub
                    {
                        ChannelId = chan,
                        MsgId = await PostInformationAsync(Context)
                    };
                    db.NeoHubSettings.Add(set);
                    db.SaveChanges();
                    await Context.Message.DeleteAsync();
                }
            }
        }

        [Command("information")]
        [Remarks("Post an information text to information channel.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task PostInformation(ulong mid)
        {
            using (var db = new NeoContext())
            {
                var chan = Context.Channel.Id;
                if (db.NeoHubSettings.Any(x => x.MsgId == mid))
                {
                    var set = db.NeoHubSettings.FirstOrDefault(f => f.MsgId == mid);
                    db.NeoHubSettings.Remove(set);
                    db.SaveChanges();
                    await Context.Message.DeleteAsync();
                    var m = await (Context.Client.GetChannel(set.ChannelId) as ITextChannel)
                        .GetMessageAsync(set.MsgId);
                    await m.DeleteAsync();
                }
            }
        }

        private async Task<ulong> PostInformationAsync(SocketCommandContext c)
        {
            var embed = NeoEmbeds.Information(c.Client);
            var m = await c.Channel.SendMessageAsync("", false, embed);
            return m.Id;
        }

        [Command("die")]
        [Remarks("Kills the bot process.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task KillProcess()
        {
            var embed = NeoEmbeds.Minimal("Boi");
            await ReplyAsync("", false, embed.Build());
            await Task.Delay(100);
            Environment.FailFast("exit");
        }

        [Command("restart")]
        [Remarks("Restarts the bot process.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task RestartProcess()
        {
            var embed = NeoEmbeds.Minimal("Boi!");
            await ReplyAsync("", false, embed.Build());
            await Task.Delay(100);
            var embed2 = NeoEmbeds.Minimal("Boi ytho.");
            await ReplyAsync("", false, embed2.Build());
            Process.Start("././launch.cmd");
            Environment.FailFast("exit");
            //fix this shit
        }

        [Group("blacklist")]
        public class SubModule2 : InteractiveBase<SocketCommandContext>
        {
            [Command]
            [Remarks("Display Blacklist.")]
            [MinPermissions(AccessLevel.BotOwner)]
            public async Task DisplayBlacklistHelper()
            {
                var embed = NeoEmbeds.Minimal("Select").AddField("\u200B", "Not yet implemented use `list` subcommand.").Build();
                var msg = await ReplyAsync("", false, embed);
            }

            [Command("list")]
            [Remarks("Display Blacklist.")]
            [MinPermissions(AccessLevel.BotOwner)]
            public async Task DisplayBlacklist([Summary("Page Number")] int page = 1)
            {
                var limit = 20;
                List<Blacklist> bl;
                using (var db = new NeoContext())
                {
                    bl = await db.Blacklist.AsAsyncEnumerable().Skip((page * limit) - limit).Take(limit).ToListAsync();
                }
                await PagedReplyAsync(bl);
            }

            [Command("add")]
            [Remarks("Add an user to blacklist.")]
            [MinPermissions(AccessLevel.BotOwner)]
            public async Task AddBlacklist([Summary("User Mention")] IUser userid, [Summary("Reason")][Remainder] string reason = null)
            {
                await AddBlacklist(userid.Id, reason);
            }

            [Command("add")]
            [Remarks("Add an user to blacklist.")]
            [MinPermissions(AccessLevel.BotOwner)]
            public async Task AddBlacklist([Summary("User ID")] ulong userid, [Summary("Reason")][Remainder] string reason = null)
            {
                using (var db = new NeoContext())
                {
                    if (db.Blacklist.Any(bl => bl.User == db.Users.FirstOrDefault(u => u.Id == userid)))
                    {
                        var builder = NeoEmbeds.Error("User is already blacklisted.", Context.User);
                        await ReplyAsync("", false, builder.Build());
                        return;
                    }

                    var obj = new Blacklist
                    {
                        User = db.Users.FirstOrDefault(u => u.Id == userid),
                        Creation = DateTime.Now,
                        reason = reason
                    };
                    db.Blacklist.Add(obj);
                    db.SaveChanges();
                    var builder2 = NeoEmbeds.Success("User is now blacklisted.", Context.User);
                    await ReplyAsync("", false, builder2.Build());
                }
            }

            [Command("del")]
            [Remarks("Remove a user from blacklist.")]
            [MinPermissions(AccessLevel.BotOwner)]
            public async Task DelBlacklist([Summary("User Mention")] IUser userid)
            {
                await DelBlacklist(userid.Id);
            }

            [Command("del")]
            [Remarks("Remove a user from blacklist.")]
            [MinPermissions(AccessLevel.BotOwner)]
            public async Task DelBlacklist([Summary("User ID")] ulong userid)
            {
                using (var db = new NeoContext())
                {
                    if (!db.Blacklist.Any(bl => bl.User == db.Users.FirstOrDefault(u => u.Id == userid)))
                    {
                        var builder = NeoEmbeds.Error("User is not blacklisted.", Context.User);
                        await ReplyAsync("", false, builder.Build());
                        return;
                    }

                    var obj = db.Blacklist.FirstOrDefault(bl => bl.User == db.Users.FirstOrDefault(u => u.Id == userid));
                    db.Blacklist.Remove(obj);
                    db.SaveChanges();
                    var builder2 = NeoEmbeds.Success("User removed from blacklist.", Context.User);
                    await ReplyAsync("", false, builder2.Build());
                }
            }
        }

        [Group("set")]
        public class SubModule3 : ModuleBase<SocketCommandContext>
        {
            [Command("name")]
            [Remarks("Changes the bots name.")]
            [MinPermissions(AccessLevel.BotOwner)]
            public async Task SetName([Summary("New Name")][Remainder] string newName)
            {
                if (string.IsNullOrWhiteSpace(newName))
                    return;

                var oldname = Context.Client.CurrentUser.Username;
                await Context.Client.CurrentUser.ModifyAsync(x => x.Username = newName);
                await Task.Delay(100);
                if (newName == Context.Client.CurrentUser.Username)
                {
                    var builder = NeoEmbeds.Success($"Changed my name to {newName} from {oldname}", Context.User, "Yay !!");
                    await ReplyAsync("", false, builder.Build());
                }
                else
                {
                    var builder = NeoEmbeds.Error("I could not change my name :c", Context.User, "Boo...");
                    await ReplyAsync("", false, builder.Build());
                }
            }

            [Command("ninja")]
            [Remarks("Changes the bots visibility.")]
            [MinPermissions(AccessLevel.BotOwner)]
            public async Task SetNinja()
            {
                var current = Context.Client.CurrentUser.Status;
                var newstat = current != UserStatus.Invisible ? UserStatus.Invisible : UserStatus.Online;
                await Context.Client.SetStatusAsync(newstat);

                await Task.Delay(100);
                var builder = NeoEmbeds.Success($"I am now {newstat}", Context.User, "Like a Ninja...");
                await ReplyAsync("", false, builder.Build());
            }

            [Command("nickname")]
            [Remarks("Changes the bots nickname.")]
            [RequireContext(ContextType.Guild)]
            [RequireBotPermission(GuildPermission.ChangeNickname)]
            [MinPermissions(AccessLevel.BotOwner)]
            public async Task SetNickName([Summary("New name (empty for reset)")][Remainder] string newName = null)
            {
                var guild = Context.Guild;
                var self = guild.CurrentUser;

                await self.ModifyAsync(x => x.Nickname = newName);
                await Task.Delay(100);

                var builder = NeoEmbeds.Success($"Changed my nickname to {newName}", Context.User, "Yay !!");
                await ReplyAsync("", false, builder.Build());

            }
            [Command("avatar")]
            [Remarks("Changes the bots avatar.")]
            [MinPermissions(AccessLevel.BotOwner)]
            public async Task SetAvatar([Summary("Avatar URL")][Remainder] string img)
            {
                if (string.IsNullOrWhiteSpace(img))
                    return;

                using (var http = new HttpClient())
                {
                    using (var sr = await http.GetStreamAsync(img))
                    {
                        var imgStream = new MemoryStream();
                        await sr.CopyToAsync(imgStream);
                        imgStream.Position = 0;

                        await Context.Client.CurrentUser.ModifyAsync(u => u.Avatar = new Image(imgStream)).ConfigureAwait(false);
                    }
                }

                var builder = NeoEmbeds.Success("Changed my avatar !", Context.User, "Yay !!");
                await ReplyAsync("", false, builder.Build());
            }
        }

        [Group("playing")]
        public class SubModule4 : ModuleBase<SocketCommandContext>
        {
            [Command("add")]
            [Remarks("Add to the rotating game queue.")]
            [MinPermissions(AccessLevel.BotOwner)]
            public async Task AddGame([Summary("Game Name")][Remainder] string game)
            {
                using (var db = new NeoContext())
                {
                    if (db.Playings.Any(x => x.Name == game))
                    {
                        var embedFail = NeoEmbeds.Minimal("Game exists in current queue.").Build();
                        await ReplyAsync("", false, embedFail);
                    }
                    else
                    {
                        var obj = new Playing
                        {
                            Name = game
                        };
                        db.Playings.Add(obj);
                        db.SaveChanges();
                        var embedSucc = NeoEmbeds.Minimal($"Successfully added {game} to the queue.").Build();
                        await ReplyAsync("", false, embedSucc);
                    }
                }
            }

            [Command("del")]
            [Remarks("Remove from the rotating game queue.")]
            [MinPermissions(AccessLevel.BotOwner)]
            public async Task RemGame([Summary("Game Name")][Remainder] string game)
            {
                using (var db = new NeoContext())
                {
                    if (!db.Playings.Any(x => x.Name == game))
                    {
                        var embedFail = NeoEmbeds.Minimal("Game do not exist in current queue.").Build();
                        await ReplyAsync("", false, embedFail);
                    }
                    else
                    {
                        var obj = db.Playings.FirstOrDefault(x => x.Name == game);
                        db.Playings.Remove(obj);
                        db.SaveChanges();
                        var embedSucc = NeoEmbeds.Minimal($"Successfully removed {game} from the queue.").Build();
                        await ReplyAsync("", false, embedSucc);
                    }
                }
            }

            [Command("list")]
            [Remarks("List the rotating game queue.")]
            [MinPermissions(AccessLevel.BotOwner)]
            public async Task ListGame()
            {
                using (var db = new NeoContext())
                {
                    var embed = NeoEmbeds.Minimal("Queue");
                    var list = "";
                    foreach (var game in db.Playings.ToList())
                    {
                        list += game.Name + "\n";
                    }
                    embed.Description = list;
                    await ReplyAsync("", false, embed.Build());
                }
            }
        }
    }
}
