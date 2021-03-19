using Discord;
using Discord.Commands;
using FezBotRedux.Common.Attributes;
using FezBotRedux.Common.Enums;
using FezBotRedux.Common.Extensions;
using FezBotRedux.Common.Models;
using NCalc;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FezBotRedux.Modules.General {
    [Name("General"), Summary("contains the general commands.")]
    public class General : ModuleBase<SocketCommandContext> {
        private static bool CheckTimeString(string input) {
            return !(HandleTime(input) == new TimeSpan(0, 0, 0, 0));
        }

        private static TimeSpan HandleTime(string input) {
            var match = Regex.Match(input, @"^(?=\d)((?<hours>\d+)h)?\s*((?<minutes>\d+)m?)?$", RegexOptions.ExplicitCapture);

            if (!match.Success)
                return new TimeSpan(0, 0, 0, 0);

            int.TryParse(match.Groups["hours"].Value, out var hours);

            int.TryParse(match.Groups["minutes"].Value, out var minutes);

            return new TimeSpan(0, hours, minutes, 0);
        }
        public static string Thing(TimeSpan x) {
            var n = "";
            if (x.Hours != 0)
                n += x.Hours + " hours ";
            if (x.Minutes != 0)
                n += x.Minutes + " minutes ";
            if (x.Seconds != 0)
                n += x.Seconds + " seconds";
            return n;
        }

        [Group("afk"), Alias("brb"), Name("Away From Keyboard")]
        public class SubModule2 : ModuleBase<SocketCommandContext> {
            [Command]
            [Remarks("Sets you afk, or back")]
            public async Task Away([Summary("Time and/or Reason")][Remainder] string reason = null) {
                using (var db = new NeoContext()) {
                    if (db.Afks.Any(afk => afk.User == db.Users.FirstOrDefault(u => u.Id == Context.User.Id))) //user is afk so he is back now
                    {
                        var obj = db.Afks.FirstOrDefault(afk => afk.User == db.Users.FirstOrDefault(u => u.Id == Context.User.Id));
                        if (string.IsNullOrEmpty(obj?.Reason)) {
                            var embed = NeoEmbeds.Afk($"{Context.User} is back!", Context.User);
                            await ReplyAsync("", false, embed.Build());
                        } else {
                            var embed = NeoEmbeds.Afk($"{Context.User} is back from {obj.Reason.TrimStart()}!", Context.User);
                            await ReplyAsync("", false, embed.Build());
                        }
                        db.Afks.Attach(obj ?? throw new InvalidOperationException());
                        db.Afks.Remove(obj);
                        db.SaveChanges();
                    } else {
                        var obj = new Afk();
                        if (reason != null) // there is time and or reason
                        {
                            var time = reason.Substring(0, reason.IndexOf(' ') <= -1 ? reason.Length : reason.IndexOf(' ')); // get first block
                            if (CheckTimeString(time))//check if first block is time string
                            {
                                var timestr = HandleTime(time); //get timespan from that block
                                reason = reason.Replace(time, "");//get time out of reason
                                if (reason.Length == 0) {
                                    var embed = NeoEmbeds.Afk($"{Context.User} is now afk!", Context.User, null, Thing(timestr));
                                    await ReplyAsync("", false, embed.Build());
                                    obj.Reason = null;
                                    obj.Time = (DateTime.Now + timestr);
                                    obj.User = db.Users.FirstOrDefault(u => u.Id == Context.User.Id);
                                } else {
                                    var embed = NeoEmbeds.Afk($"{Context.User} is now afk!", Context.User, reason, Thing(timestr));
                                    await ReplyAsync("", false, embed.Build());
                                    obj.Reason = reason;
                                    obj.Time = (DateTime.Now + timestr);
                                    obj.User = db.Users.FirstOrDefault(u => u.Id == Context.User.Id);
                                }

                            } else//no time just reason
                              {
                                var embed = NeoEmbeds.Afk($"{Context.User} is now afk!", Context.User, reason);
                                await ReplyAsync("", false, embed.Build());
                                obj.Reason = reason;
                                obj.Time = default(DateTime);
                                obj.User = db.Users.FirstOrDefault(u => u.Id == Context.User.Id);
                            }
                        } else//no reason and time
                          {
                            var embed = NeoEmbeds.Afk($"{Context.User} is now afk!", Context.User);
                            await ReplyAsync("", false, embed.Build());
                            obj.Reason = null;
                            obj.Time = default(DateTime);
                            obj.User = db.Users.FirstOrDefault(u => u.Id == Context.User.Id);
                        }
                        db.Afks.Add(obj);
                        db.SaveChanges();
                    }
                }
            }
        }

        [Command("calc")]
        [Remarks("Calculation with NCalc.")]
        public async Task NCalc([Summary("Expression")][Remainder] string exp = null) {
            if (string.IsNullOrWhiteSpace(exp)) //if null leave current
            {
                var embed = NeoEmbeds.Log("You can use this command to solve math and stuff", "powered by NCalc.").Build();
                await ReplyAsync("", false, embed);
            } else {
                var eval = new Expression(exp).Evaluate();
                var embed = NeoEmbeds.Log(eval.ToString(), " by NCalc").Build();
                await ReplyAsync("", false, embed);
            }
        }

        [Command("ping")]
        [Remarks("pong.")]
        [GuildCooldown(5)]
        public async Task Pingpong() {
            var embed = NeoEmbeds.Minimal($":ping_pong: **Gateway:** {Context.Client.Latency}ms");
            await ReplyAsync("", false, embed.Build());
        }

        [Command("stats")]
        [Remarks("Gets the stats of bot.")]
        [MinPermissions(AccessLevel.User)]
        public async Task GetStats() {
            var curuser = Context.Client.CurrentUser;
            var builder = new EmbedBuilder {
                Color = new Color(114, 137, 218),
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
                x.Name = "Author";
                x.Value = "48#0048";
                x.IsInline = true;
            });

            builder.AddField(x => {
                x.Name = "Library";
                x.Value = "Discord.NET " + DiscordConfig.Version;
                x.IsInline = true;
            });

            builder.AddField(x => {
                x.Name = "Runtime";
                x.Value = RuntimeInformation.FrameworkDescription + " " + RuntimeInformation.OSArchitecture;
                x.IsInline = true;
            });

            builder.AddField(x => {
                x.Name = "Uptime";
                x.Value = Thing(GetUptime());
                x.IsInline = true;
            });            

            builder.AddField(x => {
                x.Name = "Details";
                x.Value = $"{Context.Client.Guilds.Count} Servers \n" + $"{Context.Client.Guilds.Sum(g => g.Channels.Count(z => z is ITextChannel))} Text Channels \n" + $"{Context.Client.Guilds.Sum(g => g.Channels.Count(z => z is IVoiceChannel))} Voice Channels \n"
                          + $"{Context.Client.Guilds.Sum(g => g.Users.Count)} Users";
                x.IsInline = true;
            });

            builder.AddField(x => {
                x.Name = "Memory";
                x.Value = GetHeapSize();
                x.IsInline = true;
            });
            await ReplyAsync("", false, builder.Build());
        }

        private static TimeSpan GetUptime()
            => (DateTime.Now - Process.GetCurrentProcess().StartTime);
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.InvariantCulture);
    }
}
