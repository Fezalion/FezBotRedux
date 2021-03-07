using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FezBotRedux.Common.Attributes {
    public class GuildCooldownAttribute : PreconditionAttribute {
        private Dictionary<IGuild, Timer> _cd;
        private int _cooldown; //seconds
        public GuildCooldownAttribute(int cooldown = 60) {
            _cd = new Dictionary<IGuild, Timer>();
            _cooldown = cooldown;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider map) {
            if (_cd.ContainsKey(context.Guild))
                return Task.FromResult(PreconditionResult.FromError(SearchResult.FromError(CommandError.UnmetPrecondition, "You have to wait before using this command again.")));
            var timer = new Timer(Timer_Reset, context.Guild, _cooldown * 1000, Timeout.Infinite);
            _cd.Add(context.Guild, timer);
            return Task.FromResult(PreconditionResult.FromSuccess());
        }

        private void Timer_Reset(object state) {
            _cd[(IGuild)state].Dispose();
            _cd.Remove((IGuild)state);
        }
    }
}
