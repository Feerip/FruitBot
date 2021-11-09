using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FruitBot.Preconditions
{
    internal class AdminOnly : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionCommandContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (context.User is SocketGuildUser guildUser)
            {
                if (guildUser.Roles.Any(role => role.Permissions.Administrator))
                {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }
            }
            return Task.FromResult(PreconditionResult.FromError("You require admin permission to use this command."));
        }
    }
}
