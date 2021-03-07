using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using FezBotRedux.Common.Attributes;
using FezBotRedux.Common.Enums;
using FezBotRedux.Common.Extensions;
using FezBotRedux.Common.Models;
using MingweiSamuel.Camille;
using MingweiSamuel.Camille.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FezBotRedux.Modules.Fun {
    [Name("Betting"), Summary("contains the betting commands.")]
    public class Betting : InteractiveBase<SocketCommandContext> {
        [RequireContext(ContextType.Guild)]
        [Group("bet"), Name("Betting")]
        public class SubModuleBet : InteractiveBase<SocketCommandContext> {
            [Command("close"), Remarks("close the latest bet.")]
            public async Task CloseBet() {
                using (var db = new NeoContext()) {
                    if (db.NeoBet.Any(x => x.ChannelId == Context.Channel.Id)) //if bet exist
                    {
                        if (db.NeoBet.FirstOrDefault(x => x.ChannelId == Context.Channel.Id).open) {
                            var obj = db.NeoBet.FirstOrDefault(x => x.ChannelId == Context.Channel.Id);
                            obj.open = false;
                            db.Update(obj);
                            db.SaveChanges();
                            var embed = NeoEmbeds.Minimal($"Bet closed. Good luck!").Build();
                            await ReplyAsync("", false, embed);
                        } else //already closed.
                          {
                            var embed = NeoEmbeds.Error("Bet is already closed.", Context.User);
                            await ReplyAsync("", false, embed.Build());
                        }
                    } else {
                        var embed = NeoEmbeds.Error("There is no active bets.", Context.User);
                        await ReplyAsync("", false, embed.Build());
                    }
                }
            }
            [Command("cash"), Remarks("get cash info")]
            public async Task GetCash([Summary("User Mention")] IUser userid = null) {
                userid = userid ?? Context.User;
                using (var db = new NeoContext()) {
                    if (db.Users.Any(nbu => nbu.Id == userid.Id)) {
                        var cash = db.Users.FirstOrDefault(u => u.Id == userid.Id);
                        var embed = NeoEmbeds.Minimal($"{userid} has {cash.Cash}₺").Build();
                        await ReplyAsync("", false, embed);
                    } else {
                        var embed = NeoEmbeds.Minimal($"{userid} is not yet registered.").Build();
                        await ReplyAsync("", false, embed);
                    }
                }
            }

            [Command("delete"), Remarks("delete current bet.")]
            public async Task DeleteBet() {
                using (var db = new NeoContext()) {
                    if (db.NeoBet.Any(x => x.ChannelId == Context.Channel.Id)) {
                        var obj = db.NeoBet.FirstOrDefault(x => x.ChannelId == Context.Channel.Id);
                        foreach (var loser in obj.userBets) {
                            if (loser.User.Cash < 500) {
                                loser.User.Cash = 500;
                            }
                            db.Users.Update(loser.User);
                        }
                        foreach (var i in obj.userBets) {
                            db.Remove(i);

                        }
                        foreach (var i in obj.Bets) {
                            db.Remove(i);
                        }
                        db.NeoBet.Attach(obj);
                        db.NeoBet.Remove(obj);
                        db.SaveChanges();
                        var embed = NeoEmbeds.Success("Bet removed.", Context.User);
                        await ReplyAsync("", false, embed.Build());
                    } else {
                        var embed = NeoEmbeds.Error("There is no active bets.", Context.User);
                        await ReplyAsync("", false, embed.Build());
                    }
                }
            }
            [Command("lb"), Remarks("get leader board")]
            public async Task GetLeaderBoardAsync() {
                using (var db = new NeoContext()) {
                    var list = new List<Tuple<string, int>>();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("**Top 10**\n");
                    var usersw = await Context.Channel.GetUsersAsync().FlattenAsync();
                    var users = usersw.ToList();
                    foreach (var u in users) {
                        list.Add(new Tuple<string, int>(u.Username, db.Users.FirstOrDefault(c => c.Id == u.Id)?.Cash ?? 0));
                    }
                    var newlist = list.Where(x => x.Item2 > 0).OrderByDescending(x => x.Item2).ToList();
                    foreach (var i in newlist) {
                        sb.Append($"**{i.Item1}** : {i.Item2}\n");
                    }
                    var embed = NeoEmbeds.Minimal(sb.ToString());
                    await ReplyAsync("", false, embed.Build());
                }
            }

            [Command("create", RunMode = RunMode.Async), Remarks("Create bet."), MinPermissions(AccessLevel.ServerAdmin)]
            public async Task CreateBet([Summary("The bet"), Remainder] string betname) {
                using (var db = new NeoContext()) {
                    if (db.NeoBet.Any(x => x.ChannelId == Context.Channel.Id)) //there is a bet
                    {
                        var embed = NeoEmbeds.Error("There is already a bet going.", Context.User);
                        await ReplyAsync("", false, embed.Build());
                    }
                    //Create bet
                    else {
                        var tuples = new List<Tuple<string, double>>();
                        var count = 0;
                        double answer2 = 0.0;

                        var msg = await ReplyAsync("How many rates?");
                        var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(30));

                        await msg.DeleteAsync();
                        await response.DeleteAsync();

                        if (response != null) {
                            if (int.TryParse(response.Content, out count)) {
                                if (count <= 0 || count >= 5) {
                                    await ReplyAndDeleteAsync("Please give a number either bigger than 0 or lower than 5.", false, null, TimeSpan.FromSeconds(5));
                                } else // we gucci lets get the nay nays
                                  {
                                    for (int index = 0; index < count; index++) {
                                        var old1 = await ReplyAsync($"Bet {index + 1}:");
                                        var response1 = await NextMessageAsync(true, true, TimeSpan.FromSeconds(30));
                                        if (response1 != null) {
                                            var old2 = await ReplyAsync($"Bet rate {index + 1}:");
                                            var response2 = await NextMessageAsync(true, true, TimeSpan.FromSeconds(30));
                                            if (response2 != null) {
                                                if (double.TryParse(response2.Content, out answer2)) {
                                                    tuples.Add(new Tuple<string, double>(response1.Content, answer2));
                                                    await old1.DeleteAsync();
                                                    await old2.DeleteAsync();
                                                    await response1.DeleteAsync();
                                                    await response2.DeleteAsync();
                                                } else {
                                                    await ReplyAndDeleteAsync("Please give a corrent number.", false, null, TimeSpan.FromSeconds(5));
                                                    await Task.CompletedTask;
                                                }
                                            } else {
                                                await ReplyAndDeleteAsync("You did not reply before the timeout.", false, null, TimeSpan.FromSeconds(5));
                                                await Task.CompletedTask;
                                            }
                                        } else {
                                            await ReplyAndDeleteAsync("You did not reply before the timeout.", false, null, TimeSpan.FromSeconds(5));
                                            await Task.CompletedTask;
                                        }
                                    }
                                }
                            } else {
                                await ReplyAndDeleteAsync("Please give a number.", false, null, new TimeSpan(0, 0, 5));
                                await Task.CompletedTask;
                            }
                        } else {
                            await ReplyAndDeleteAsync("You did not reply before the timeout.", false, null, new TimeSpan(0, 0, 5));
                            await Task.CompletedTask;
                        }



                        var msgobj = await ReplyAsync("Creating new bet...");
                        var betobj = new NeoBet {
                            BetName = betname,
                            ChannelId = Context.Channel.Id,
                            open = true,
                            msgID = msgobj.Id
                        };
                        foreach (var tple in tuples) {
                            betobj.Bets.Add(new Bet {
                                BetName = tple.Item1,
                                BetRate = tple.Item2
                            });
                        }

                        db.NeoBet.Attach(betobj);
                        db.SaveChanges();

                        var embed = NeoEmbeds.Bet(
                            betobj.BetName,
                            betobj.Bets,
                            0,
                            ""
                            );
                        await msgobj.ModifyAsync(x => { x.Embed = embed.Build(); x.Content = ""; });
                    }
                }
            }
            [Command(""), Remarks("play in current bet")]
            public async Task playBet([Summary("bet amount")] int cash, [Summary("bet")] int bet) {
                var flag1 = false;
                using (var db = new NeoContext()) {
                    if (!db.NeoBet.Any(x => x.ChannelId == Context.Channel.Id)) //no bet
                    {
                        var embed = NeoEmbeds.Error("There is no active bets.", Context.User);
                        await ReplyAsync("", false, embed.Build());
                    } else if (!db.NeoBet.FirstOrDefault(x => x.ChannelId == Context.Channel.Id).open)//closed bet
                      {
                        var embed = NeoEmbeds.Error("Bet is closed.", Context.User);
                        await ReplyAsync("", false, embed.Build());
                    } else //yes bet
                      {
                        var uwu = db.NeoBet.FirstOrDefault(x => x.ChannelId == Context.Channel.Id);// get bet
                        if (uwu.userBets?.Count > 0) {
                            foreach (var betto in uwu.userBets) {
                                if (betto.User.Id == Context.User.Id) {
                                    var embed = NeoEmbeds.Error("You have already bet.", Context.User);
                                    await ReplyAsync("", false, embed.Build());
                                    flag1 = true;
                                    break;
                                }
                            }
                        }

                        if (flag1 == false) {
                            var maxbet = db.NeoBet.FirstOrDefault(x => x.ChannelId == Context.Channel.Id);
                            var max = maxbet.Bets.Count;
                            if (bet <= 0 || bet > max) { //düzgün oyna mq
                                var embed = NeoEmbeds.Error("Wrong bet location.", Context.User);
                                await ReplyAsync("", false, embed.Build());
                            } else {
                                var user = db.Users.FirstOrDefault(x => x.Id == Context.User.Id);
                                if (user.Cash < cash || cash <= 0) //not enough money
                                {
                                    var embed = NeoEmbeds.Error($"Not enough ₺ to play. (Have {user.Cash}₺)", Context.User);
                                    await ReplyAsync("", false, embed.Build());
                                } else { //oh yes time to play baby

                                    var betobj = db.NeoBet.FirstOrDefault(x => x.ChannelId == Context.Channel.Id);

                                    var newobj = new NeoBets {
                                        BetAmount = cash,
                                        User = db.Users.FirstOrDefault(a => a.Id == Context.User.Id),
                                        BetLoc = bet
                                    };

                                    user.Cash -= cash;

                                    betobj.userBets.Add(newobj);

                                    db.Update(user);
                                    db.Update(betobj);
                                    db.SaveChanges();

                                    var embed = NeoEmbeds.Success($"Bet Accepted.", Context.User);
                                    await ReplyAsync("", false, embed.Build());
                                }
                            }
                        }
                    }
                }
            }
            [Command("current"), Alias("c"), Remarks("get current bet")]
            public async Task getCurrentBet() {
                using (var db = new NeoContext()) {
                    if (db.NeoBet.Any(x => x.ChannelId == Context.Channel.Id)) //there is a bet
                    {
                        var obj = db.NeoBet.FirstOrDefault(x => x.ChannelId == Context.Channel.Id);

                        StringBuilder sb = new StringBuilder();
                        int total = 0;

                        if (obj.userBets != null) {
                            foreach (var bet in obj.userBets) {
                                total += bet.BetAmount;
                                sb.Append($"{Context.Guild.GetUser(bet.User.Id).Username} : {bet.BetAmount}\n");
                            }
                        }

                        var embed = NeoEmbeds.Bet(
                            obj.BetName,
                            obj.Bets,
                            total,
                            sb.ToString()
                            );
                        await ReplyAsync("", false, embed.Build());
                    } else {
                        var embed = NeoEmbeds.Error("There is no active bets.", Context.User);
                        await ReplyAsync("", false, embed.Build());
                    }
                }
            }

            [Command("finish"), Remarks("Finish the bet and award participants")]
            public async Task FinishBet([Summary("Winning Bet Index")] int winning_index) {
                using (var db = new NeoContext()) {
                    if (db.NeoBet.Any(x => x.ChannelId == Context.Channel.Id)) //if bet
                    {
                        var bet = db.NeoBet.FirstOrDefault(x => x.ChannelId == Context.Channel.Id);
                        var realbet = bet.Bets.ToArray()[winning_index - 1];
                        StringBuilder sb = new StringBuilder();
                        StringBuilder sbloser = new StringBuilder();
                        foreach (var userBet in bet.userBets.Where(b => b.BetLoc == winning_index)) {
                            var newcash = (int)(userBet.BetAmount * realbet.BetRate);
                            userBet.User.Cash += newcash;
                            db.Users.Update(userBet.User);
                            sb.Append($"{Context.Guild.GetUser(userBet.User.Id).Username} : {newcash}₺\n");
                        } // give everybody their winnings

                        foreach (var loser in bet.userBets.Where(b => b.BetLoc != winning_index)) {
                            sbloser.Append($"{Context.Guild.GetUser(loser.User.Id).Username} : {loser.BetAmount}₺\n");
                            if (loser.User.Cash < 500) {
                                loser.User.Cash = 500;
                            }
                            db.Users.Update(loser.User);
                        }
                        //lets remove the bet and update users

                        foreach (var i in bet.userBets) {
                            db.Remove(i);
                        }
                        foreach (var i in bet.Bets) {
                            db.Remove(i);
                        }
                        db.Attach(bet);
                        db.Remove(bet);
                        db.SaveChanges();
                        var winners = sb.ToString();
                        var losers = sbloser.ToString();
                        var embed = NeoEmbeds.Minimal($"**Winners**:\n {(!string.IsNullOrEmpty(winners) ? winners : "No one wins")}\n **Losers**:\n {(!string.IsNullOrEmpty(losers) ? losers : "No losers")}");
                        await ReplyAsync("", false, embed.Build());
                    }
                }
            }
        }
        public class LeagueOfOmer {
            private Timer _lolCheckTimer = null;
            private AutoResetEvent _autoEvent = null;
            private RiotApi riot;
            public LeagueOfOmer() {
                _autoEvent = new AutoResetEvent(false);
                _lolCheckTimer = new Timer(CheckLoLAsync, _autoEvent, 0, 1000 * 60 * 5);
                //init rito api
                riot = RiotApi.NewInstance("");
            }
            private async void CheckLoLAsync(object stateInfo) {
                var game = await riot.SpectatorV4.GetCurrentGameInfoBySummonerAsync(Region.TR, "osshowcomeback");
                //check for ömers accs
                if (game != null) //live game poggers
                {
                    //todo shinenigans
                }
            }
        }
    }
}
