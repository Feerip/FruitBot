using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FruitBot.Modules
{
    public class SystemModule : InteractionModuleBase
    {
        [SlashCommand("ping", "recieve a pong")]
        public async Task Ping() =>
            await RespondAsync("pong");

#if DEBUG
        [SlashCommand("delete-commands", "Delete all commands")]
        public async Task DeleteCommands()
        {
            await (Context.Guild as SocketGuild).DeleteApplicationCommandsAsync();
            await RespondAsync("Guild commands deleted!");
        }
#endif
    }
}
