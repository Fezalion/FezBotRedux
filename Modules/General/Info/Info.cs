using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FezBotRedux.Common.Attributes;
using FezBotRedux.Common.Enums;
using FezBotRedux.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace FezBotRedux.Modules.General {
    [Name("Info"), Summary("contains the user info commands.")]
    public partial class Info : ModuleBase<SocketCommandContext> {
        [Group("guildinfo"), Alias("ginfo"), Name("Guild Info")]
        public class InfoGuildSub : ModuleBase<SocketCommandContext> {
            [Command("")]
            [Remarks("Gets the info of the guild.")]
            [MinPermissions(AccessLevel.User)]
            public async Task GetGuildInfo() {
                var guild = Context.Guild as SocketGuild;
                var curuser = Context.Client.CurrentUser;
                var builder = new EmbedBuilder {
                    Color = new Color(114, 137, 218),
                    Description = guild.Name,
                    ThumbnailUrl = guild.IconUrl,
                    Author = new EmbedAuthorBuilder {
                        Name = curuser.Username,
                        IconUrl = curuser.GetAvatarUrl()
                    },
                    Footer = new EmbedFooterBuilder {
                        Text = $"Info requested by {Context.User.Username}",
                        IconUrl = Context.User.GetAvatarUrl()
                    },
                    Timestamp = DateTime.Now
                };

                builder.AddField(x => {
                    x.Name = "ID";
                    x.Value = guild.Id.ToString();
                    x.IsInline = true;
                });

                builder.AddField(x => {
                    x.Name = "Owner";
                    x.Value = guild.Owner?.Username ?? "Not Found...";
                    x.IsInline = true;
                });

                builder.AddField(x => {
                    x.Name = "Members";
                    x.Value = guild.MemberCount.ToString();
                    x.IsInline = true;
                });

                builder.AddField(x => {
                    x.Name = "Channels";
                    x.Value = $"Text : {guild.TextChannels.Count} \nVoice : {guild.VoiceChannels.Count}";
                    x.IsInline = true;
                });

                builder.AddField(x => {
                    x.Name = "Created At";
                    x.Value = guild.CreatedAt.ToString("dd.MM.yyyy hh:mm");
                    x.IsInline = true;
                });

                builder.AddField(x => {
                    x.Name = "Region";
                    x.Value = guild.VoiceRegionId;
                    x.IsInline = true;
                });

                var roles = Context.Guild.Roles.Where(x => !x.IsEveryone);
                List<SocketRole> roles2 = roles.Take(10).ToList();
                if (roles.Count() > 10) {
                    builder.AddField(x => {
                        x.Name = $"Roles - ({roles.Count()})";
                        x.Value = "「" + string.Join(",", roles2) + " and more... " + "」";
                        x.IsInline = false;
                    });
                } else {
                    builder.AddField(x => {
                        x.Name = $"Roles - ({roles.Count()})";
                        x.Value = "「" + string.Join(",", roles2) + "」";
                        x.IsInline = false;
                    });
                }


                var emojilist = new List<string>();
                foreach (var e in guild.Emotes.Take(10)) {
                    emojilist.Add($"{e.Name} <:{e.Name}:{e.Id}>");
                }
                if (guild.Emotes.Count() > 10) {
                    builder.AddField(x => {
                        x.Name = $"Custom Emojis ({guild.Emotes.Count})";
                        x.Value = "「" + string.Join(" ", emojilist) + "  and more..." + "」";
                        x.IsInline = false;
                    });
                } else {
                    builder.AddField(x => {
                        x.Name = $"Custom Emojis ({guild.Emotes.Count})";
                        x.Value = "「" + string.Join(" ", emojilist) + "」";
                        x.IsInline = false;
                    });
                }


                await ReplyAsync("", false, builder.Build());
            }
        }

        [Group("uinfo"), Alias("userinfo"), Name("User Info")]
        public class InfoUserSub : ModuleBase<SocketCommandContext> {
            [Command("av")]
            [Remarks("Gets the info of specified user.")]
            [MinPermissions(AccessLevel.User)]
            public async Task GetUserAvatar([Summary("User Mention")] IUser user = null) {
                if (user == null) {
                    user = Context.User;
                }
                await ReplyAsync(user.GetAvatarUrl());
            }

            [Command("")]
            [Remarks("Gets the info of specified user.")]
            [MinPermissions(AccessLevel.User)]
            public async Task GetUserInfo([Summary("User Mention")] IUser user = null) {
                if (user == null) {
                    user = Context.User;
                }
                var curuser = Context.Client.CurrentUser;
                var builder = new EmbedBuilder {
                    Color = new Color(114, 137, 218),
                    Title = user.Username,
                    Description = (user.Activity != null ? $"Playing {user.Activity.Name}." : "Doing nothing..."),
                    Author = new EmbedAuthorBuilder {
                        Name = curuser.Username,
                        IconUrl = curuser.GetAvatarUrl()
                    },
                    ThumbnailUrl = user.GetAvatarUrl(),
                    Footer = new EmbedFooterBuilder {
                        Text = $"Info requested by {Context.User.Username}",
                        IconUrl = Context.User.GetAvatarUrl()
                    },
                    Timestamp = DateTime.Now
                };


                builder.AddField(x => {
                    x.Name = "ID";
                    x.Value = user.Id.ToString();
                    x.IsInline = true;
                });

                builder.AddField(x => {
                    x.Name = "Nickname";
                    x.Value = Context.Guild.GetUser(user.Id).Nickname ?? "No nickname, Boo...";
                    x.IsInline = true;
                });

                builder.AddField(x => {
                    x.Name = "Joined Discord";
                    x.Value = $"{user.CreatedAt.ToString("dd.MM.yyyy hh:mm", CultureInfo.InvariantCulture)}";
                    x.IsInline = true;
                });

                builder.AddField(x => {
                    x.Name = $"Joined {Context.Guild.Name}";
                    x.Value = $"{Context.Guild.GetUser(user.Id).JoinedAt?.ToString("dd.MM.yyyy hh:mm", CultureInfo.InvariantCulture)}";
                    x.IsInline = true;
                });

                builder.AddField(x => {
                    x.Name = "Shared Servers";
                    x.Value = "I share " + GetsharedAsync(user, Context) + " guilds with " + user.Username;
                    x.IsInline = true;
                });

                //get roles here
                var roles = Context.Guild.GetUser(user.Id).Roles.Where(x => !x.IsEveryone);

                builder.AddField(x => {
                    x.Name = $"Roles ({roles.Count()})";
                    x.Value = $"「{string.Join(",", roles)}」";
                    x.IsInline = false;
                });

                await ReplyAsync("", false, builder.Build());
            }

            private static int GetsharedAsync(IUser u, SocketCommandContext self) {
                var selfguildlist = self.Client.Guilds;
                return selfguildlist.Count(g => g.Users.Contains(u));
            }
        }
    }
}
