using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using FezBotRedux.Common.Enums;
using FezBotRedux.Common.Types;

namespace FezBotRedux.Common.Attributes
{
    /// <summary>
    /// Set the minimum permission required to use a module or command
    /// similar to how MinPermissions works in Discord.Net 0.9.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class MinPermissionsAttribute : PreconditionAttribute
    {
        private readonly AccessLevel _level;

        public MinPermissionsAttribute(AccessLevel level)
        {
            _level = level;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var access = GetPermission(context);            // Get the acccesslevel for this context

            return Task.FromResult(access >= _level ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("Insufficient permissions."));
        }

        private static AccessLevel GetPermission(ICommandContext c)
        {
            if (c.User.IsBot)                                    // Prevent other bots from executing commands.
                return AccessLevel.Blocked;

            if (Configuration.Load().Owners.Contains(c.User.Id)) // Give configured owners special access.
                return AccessLevel.BotOwner;

            if (!(c.User is SocketGuildUser user))
                return AccessLevel.User;                     // If nothing else, return a default permission.
            if (c.Guild.OwnerId == user.Id)                  // Check if the user is the guild owner.
                return AccessLevel.ServerOwner;

            if (user.GuildPermissions.Administrator)         // Check if the user has the administrator permission.
                return AccessLevel.ServerAdmin;

            if (user.GuildPermissions.ManageMessages ||      // Check if the user can ban, kick, or manage messages.
                user.GuildPermissions.BanMembers ||
                user.GuildPermissions.KickMembers)
                return AccessLevel.ServerMod;

            return AccessLevel.User;                             // If nothing else, return a default permission.
        }
    }
}
