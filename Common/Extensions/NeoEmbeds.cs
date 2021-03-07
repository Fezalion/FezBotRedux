using Discord;
using Discord.WebSocket;
using FezBotRedux.Common.Models;
using FezBotRedux.Common.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FezBotRedux.Common.Extensions {
    public static class NeoEmbeds {
        public static EmbedBuilder Afk(string message, IUser u, string reason = null, string time = null, string title = "Away From Keyboard") {
            var embed = new EmbedBuilder {
                Color = new Color(0, 255, 0),
                Description = message,
                Author = new EmbedAuthorBuilder {
                    Name = title,
                    IconUrl = u.GetAvatarUrl()
                },
                Timestamp = DateTime.UtcNow
            };
            if (!string.IsNullOrEmpty(reason)) {
                embed.AddField(x => {
                    x.Name = "Reason";
                    x.Value = reason;
                    x.IsInline = true;
                });
            }

            if (!string.IsNullOrEmpty(time)) {
                embed.AddField(x => {
                    x.Name = "Time Left";
                    x.Value = time;
                    x.IsInline = true;
                });
            }

            return embed;
        }

        public static EmbedBuilder Success(string message, IUser u, string title = "Success") {
            return new EmbedBuilder {
                Color = new Color(0, 255, 0),
                Description = message,
                Author = new EmbedAuthorBuilder {
                    Name = title,
                    IconUrl = "https://cdn0.iconfinder.com/data/icons/small-n-flat/24/678134-sign-check-512.png"
                },
                Footer = new EmbedFooterBuilder {
                    Text = $"Command executed by {u.Username}",
                    IconUrl = u.GetAvatarUrl()
                },
                Timestamp = DateTime.UtcNow
            };
        }

        public static EmbedBuilder Bet(string betname, HashSet<Bet> betset, int total_cash, string bets) {
            var embed = new EmbedBuilder {
                Color = new Color(0, 255, 0),
                Author = new EmbedAuthorBuilder {
                    Name = betname
                },
                Footer = new EmbedFooterBuilder {
                    Text = $"Total bets: {total_cash}"
                },
                Timestamp = DateTime.UtcNow
            };
            foreach (var b in betset) {
                embed.AddField(x => {
                    x.Name = b.BetName;
                    x.Value = b.BetRate;
                    x.IsInline = true;
                });
            }

            if (!string.IsNullOrEmpty(bets)) {
                embed.AddField(x => {
                    x.Name = "Bets";
                    x.Value = bets;
                    x.IsInline = false;
                });
            }

            return embed;
        }

        public static EmbedBuilder Error(string message, IUser u, string title = "Error") {
            return new EmbedBuilder {
                Color = new Color(255, 0, 0),
                Description = message,
                Author = new EmbedAuthorBuilder {
                    Name = title,
                    IconUrl = "https://cdn0.iconfinder.com/data/icons/small-n-flat/24/678069-sign-error-512.png"
                },
                Footer = new EmbedFooterBuilder {
                    Text = $"Command executed by {u.Username}",
                    IconUrl = u.GetAvatarUrl()
                },
                Timestamp = DateTime.UtcNow
            };
        }

        public static Embed Information(DiscordSocketClient c) {
            var prefix = Configuration.Load().Prefix;
            var owners = Configuration.Load().Owners;
            var users = owners.Select(own => c.GetUser(own).Username).ToList();

            return new EmbedBuilder() {
                Author = new EmbedAuthorBuilder {
                    Name = "Information",
                    IconUrl = c.CurrentUser.GetAvatarUrl()
                },
                Footer = new EmbedFooterBuilder {
                    Text = $"Refresh date "
                },
                Timestamp = DateTime.UtcNow,
                Description = $"Hello, this is {c.CurrentUser.Username} \n" +
                              $"Welcome to the server, this is where you can get help about {c.CurrentUser.Username}.\n" +
                              "It's dangerous to go alone [take this.](http://discord.gg/fZ3pcUG)\n\n" +
                              "**BY USING ME AND MY SERVICES**, you ***accept*** that I can store relevant information about you, " +
                              "as in your UserID and seen message count.\n\n" +
                              "**Rules :**\n" +
                              "\t**|** *Respect the mods and owners.*\n" +
                              "\t**|** *Respect others too.*\n" +
                              "\t**|** *Swearing is allowed, but stop when warned.*\n" +
                              "\t**|** *Keep this place sfw, if not stated otherwise.*\n" +
                              "\t**|** *No user is above these rules, except **Owner**.*\n" +
                              "\t**|** *Keep calm and enjoy.*\n\n" +
                              "**Latest Change :**\n" +
                              "\t**|** *Switched to gitlab.*\n\n" +
                              $"DM me with {prefix}help to see all commands.\n" +
                              "My Owners are " + string.Join(", ", users) + ".\n"
            }.Build();
        }

        public static EmbedBuilder Log(string message, string eventName, string newmsg = null, string oldmsg = null) {
            var embed = new EmbedBuilder {
                Color = new Color(255, 0, 0),
                Title = message,
                Footer = new EmbedFooterBuilder {
                    Text = $":zap: {eventName}"
                },
                Timestamp = DateTime.UtcNow
            };

            if (oldmsg != null) {
                embed.AddField(x => {
                    x.Name = "Old";
                    x.Value = oldmsg;
                    x.IsInline = false;
                });
            }

            if (newmsg != null) {
                embed.AddField(x => {
                    x.Name = oldmsg == null ? "Deleted Message" : "New";
                    x.Value = newmsg;
                    x.IsInline = false;
                });
            }

            return embed;
        }

        public static EmbedBuilder Minimal(string message) {
            var embed = new EmbedBuilder {
                Color = new Color(255, 0, 0),
                Title = message
            };

            return embed;
        }

        public static EmbedBuilder TagStats(string username, Tag t) {
            var embed = new EmbedBuilder {
                Color = new Color(255, 255, 255),
                Title = $"Stats of `{t.Trigger}`"
            };

            embed.AddField(x => {
                x.Name = "Owner";
                x.Value = username;
                x.IsInline = true;
            });

            embed.AddField(x => {
                x.Name = "Creation Date";
                x.Value = t.Creation.ToString("dd.MM.yyyy hh:mm", CultureInfo.InvariantCulture);
                x.IsInline = true;
            });

            embed.AddField(x => {
                x.Name = "Uses";
                x.Value = t.Uses.ToString();
                x.IsInline = true;
            });

            embed.AddField(x => {
                x.Name = "Value";
                x.Value = t.Value.WordWrap(30)[0].Length > 29
                    ? t.Value.WordWrap(30)[0] + "..."
                    : t.Value.WordWrap(30)[0];
                x.IsInline = false;
            });

            return embed;
        }

        public static Embed Picture(string objValue, IUser u) {
            return new EmbedBuilder {
                ImageUrl = objValue,
                Footer = new EmbedFooterBuilder {
                    Text = $"Tag executed by {u.Username}",
                    IconUrl = u.GetAvatarUrl()
                },
                Timestamp = DateTime.UtcNow
            }.Build();
        }
    }
}

