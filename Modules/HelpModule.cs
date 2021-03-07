using Discord;
using Discord.Commands;
using FezBotRedux.Common.Extensions;
using FezBotRedux.Common.Types;
using System.Linq;
using System.Threading.Tasks;

namespace FezBotRedux.Modules {
    [Name("Help"), Summary("contains the help commands.")]
    public class HelpModule : ModuleBase<SocketCommandContext> {
        private readonly CommandService _service;

        public HelpModule(CommandService commands) {
            _service = commands;
        }

        [Command("help")]
        public async Task HelpAsync() {
            var prefix = Configuration.Load().Prefix;
            var builder = new EmbedBuilder {
                Color = new Color(114, 137, 218),
                Description = "These are the Modules you can use.[{prefix}help module] for extra help."
            };

            foreach (var module in _service.Modules.Where(x => !x.IsSubmodule && x.Name != "Help")) {
                var pass = false;
                if (module.Preconditions.Any()) //if module has any preconditions.
                {
                    module.Commands.Select(async x => {
                        if (!(await CheckPreCon(Context, x))) { pass = true; }
                    });
                    if (!module.IsSubmodule && module.Submodules.Count > 0) {
                        foreach (var sub in module.Submodules) {
                            sub.Commands.Select(async cmd => {
                                if (!(await CheckPreCon(Context, cmd))) { pass = true; }
                            });
                        }
                    }
                }
                if (pass)
                    await Task.CompletedTask;

                builder.AddField(x => {
                    x.Name = module.Name;
                    x.Value = module.Summary + " | **" + (module.Submodules.Sum(y => y.Commands.Count) + module.Commands.Count) + "** Commands.";
                    x.IsInline = false;
                });
            }
            await ReplyAsync("", false, builder.Build());
        }

        private async Task<bool> CheckPreCon(SocketCommandContext y, CommandInfo x) {
            var z = await (x.CheckPreconditionsAsync(y));
            if (z.IsSuccess)
                return true;
            return false;
        }

        [Command("help")]
        public async Task HelpAsync([Remainder] string input) {
            var module = _service.Modules.Where(x => x.Name.ToLower() == input.ToLower() && x.Name != "Help").FirstOrDefault();
            if (module != null) //Module found with given input give module details.
            {
                var pass = false;
                if (module.Preconditions.Any()) //if module has any preconditions.
                {
                    module.Commands.Select(async x => {
                        if (!(await CheckPreCon(Context, x))) { pass = true; }
                    });
                    if (!module.IsSubmodule && module.Submodules.Count > 0) {
                        foreach (var sub in module.Submodules) {
                            sub.Commands.Select(async cmd => {
                                if (!(await CheckPreCon(Context, cmd))) { pass = true; }
                            });
                        }
                    }
                }
                if (pass)
                    await Task.CompletedTask;

                var prefix = Configuration.Load().Prefix;
                var builder = new EmbedBuilder {
                    Color = new Color(114, 137, 218),
                    Description = $"These are the commands you can use in this module.[{prefix}help command] for extra help."
                };
                string description = null;
                string subdescription = null;
                string subdescription2 = null;
                if (!module.IsSubmodule && module.Submodules.Count > 0) {
                    foreach (var submodule in module.Submodules) {
                        foreach (var subsub in submodule.Submodules) {
                            foreach (var cmd in subsub.Commands) {
                                var result2 = await CheckPreCon(Context, cmd);
                                if (result2) {
                                    if (cmd.Parameters.Count > 0)
                                        subdescription2 += $"{prefix}{cmd.Aliases.First()} [{string.Join("] [", cmd.Parameters.Select(x => x.Summary ?? x.Name))}] - {cmd.Remarks}\n";
                                    else
                                        subdescription2 += $"{prefix}{cmd.Aliases.First()} - {cmd.Remarks}\n";
                                }
                            }

                            builder.AddField(x => {
                                x.Name = subsub.Name + subsub.Remarks;
                                x.Value = subdescription2;
                                x.IsInline = false;
                            });
                            subdescription2 = null;
                        }
                        foreach (var cmd in submodule.Commands) {
                            var result = await CheckPreCon(Context, cmd);
                            if (result) {
                                if (cmd.Parameters.Count > 0)
                                    subdescription += $"{prefix}{cmd.Aliases.First()} [{string.Join("] [", cmd.Parameters.Select(x => x.Summary ?? x.Name))}] - {cmd.Remarks}\n";
                                else
                                    subdescription += $"{prefix}{cmd.Aliases.First()} - {cmd.Remarks}\n";
                            }
                        }

                        builder.AddField(x => {
                            x.Name = submodule.Name + submodule.Remarks;
                            x.Value = subdescription;
                            x.IsInline = false;
                        });
                        subdescription = null;
                    }
                }
                foreach (var cmd in module.Commands) {
                    var result = await CheckPreCon(Context, cmd);
                    if (result) {
                        if (cmd.Parameters.Count > 0)
                            description += $"{prefix}{cmd.Aliases.First()} [{string.Join("] [", cmd.Parameters.Select(x => x.Summary ?? x.Name))}] - {cmd.Remarks}\n";
                        else
                            description += $"{prefix}{cmd.Aliases.First()} - {cmd.Remarks}\n";
                    }
                }

                if (!string.IsNullOrWhiteSpace(description)) {
                    builder.AddField(x => {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
                if (builder.Fields.Count < 1) {
                    builder = NeoEmbeds.Minimal("You do not have the rights to see this modules commands.");
                }
                await ReplyAsync("", false, builder.Build());
            } else // module not found see if its a command.
              {
                var result = _service.Search(Context, input);

                if (!result.IsSuccess) {
                    await ReplyAsync($"Sorry, I couldn't find a command like **{input}**.");
                    return;
                }

                var builder = new EmbedBuilder {
                    Color = new Color(114, 137, 218),
                    Description = $"Here are some commands like **{input}**"
                };

                foreach (var match in result.Commands) {
                    var cmd = match.Command;

                    builder.AddField(x => {
                        x.Name = string.Join(", ", cmd.Aliases);
                        x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Summary ?? p.Name))}\n" +
                                  $"Remarks: {cmd.Remarks}\n" +
                                  $"Module: {cmd.Module.Name}";
                        x.IsInline = false;
                    });
                }

                await ReplyAsync("", false, builder.Build());
            }
        }
    }
}

