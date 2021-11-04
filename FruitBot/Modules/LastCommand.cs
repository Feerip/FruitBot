using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using RS3APIDropLog;

namespace FruitBot.Modules
{
    public class LastCommand : InteractionModuleBase
    {
        private readonly ILogger<LastCommand> _logger;

        public LastCommand(ILogger<LastCommand> logger)
        {
            _logger = logger;
        }

        [SlashCommand("last", "Get the last drops from the drop log")]
        public async Task Last(int numDrops = 1, SocketGuildUser user = null, string fruit = null)
        {
            using (Context.Channel.EnterTypingState())
            {
                var fruitPantry = FruitPantry.FruitPantry.GetFruitPantry();
                fruitPantry.RefreshEverything();

                string? rsn = null;
                if (user != null)
                {
                    rsn = fruitPantry._discordUsers[user.Username + "#" + user.Discriminator][1];
                }

                if (fruitPantry.GetDropLog().Count < 1)
                {
                    await ReplyAsync($"There are currently no entries in the drop log to display.");
                    return;
                }

                if (numDrops == 1)
                {
                    var drops = FruitPantry.HelperFunctions.GetLastNDrops(numDrops, rsn, fruit);

                    foreach (DropLogEntry entry in drops)
                    {
                        Embed embed = FruitPantry.HelperFunctions.BuildDropEmbed(entry);
                        await RespondAsync(embed:embed);
                    }
                }
                else
                {
                    List<string> output = await FruitPantry.HelperFunctions.BuildLastDropList(numDrops, rsn, fruit);

                    int numMessages = output.Count;

                    if (numMessages > 1)
                    {
                        for (int idx = 0; idx < numMessages; idx++)
                        {
                            output[idx] = $"`{idx + 1}/{numMessages}`\n" + output[idx];
                        }
                    }

                    ulong additionalMessageReferenceMessage = 0;
                    for (int i = 0; i < numMessages; ++i)
                    {
                        if (i == 0)
                        {
                            // Reply to the command with the first message
                            await Context.Interaction.DeferAsync();
                            var message = await FollowupAsync(output[i]);
                            additionalMessageReferenceMessage = message.Id;
                        }
                        else
                        {
                            // You can only follow up an interaction once to reply to the first message for subsequent ones.
                            await Context.Channel.SendMessageAsync(output[i], messageReference: new MessageReference(additionalMessageReferenceMessage));
                        }
                    }
                }
            }

            _logger.LogInformation($"{Context.User.Username} executed the lastdrop command!");
        }
    }
}
