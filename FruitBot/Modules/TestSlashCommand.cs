using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace FruitBot.Modules
{
    public class TestSlashCommand : InteractionModuleBase
    {
        [SlashCommand("ping", "recieve a pong")]
        public async Task Ping() =>
            await RespondAsync("pong");

#if DEBUG
        [SlashCommand("test-button", "Test a button")]
        public async Task TestEmbed()
        {
            var builder = new ComponentBuilder()
                .WithButton("Cats are Best", $"best-animal:cats")
                .WithButton("Dogs are Best", $"best-animal:dogs");

            await RespondAsync("What animal is the best?", component: builder.Build());
        }

        [ComponentInteraction("best-animal:*")]
        public async Task BestAnimalButton(string animal)
        {
                await RespondAsync($"{Context.User.Mention} thinks {animal} are the best");
        }
#endif
    }
}
