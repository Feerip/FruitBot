using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using DataTypes;
using System;
using FruitBot.Preconditions;

namespace FruitBot.Modules
{
#nullable enable
    public class LastCommand : InteractionModuleBase
    {
        private readonly ILogger<LastCommand> _logger;

        public LastCommand(ILogger<LastCommand> logger)
        {
            _logger = logger;
        }

        [SlashCommand("last", "Get the last drops from the drop log")]
        public async Task Last(
            [Summary("num-drops", "The number of drops to fetch (1-100).")]
            [InclusiveRange(1, 100)]
            int numDrops = 1,

            [Summary(description:"The optional user to filter the drops by. This can be used instead of the 'rsn' param.")]
            SocketGuildUser? user = null,

            [Summary(description:"The optional RSN to filter the drops by. This can be used instead of the 'user' param.")]
            string? rsn = null,

            [Summary(description:"The optional team to filter drops by.")]
            FruitResources.Fruit fruit = FruitResources.Fruit.Invalid)
        {
            using (Context.Channel.EnterTypingState())
            {
                var fruitPantry = FruitPantry.FruitPantry.GetFruitPantry();
                fruitPantry.RefreshEverything();

                // User param takes precedence over rsn.
                if (user != null)
                {
                    rsn = fruitPantry._discordUsers[user.Username + "#" + user.Discriminator][1];
                }

                if (fruitPantry.GetDropLog().Count < 1)
                {
                    await ReplyAsync($"There are currently no entries in the drop log to display.");
                    return;
                }

                // Defer as the lookup may take some time
                await Context.Interaction.DeferAsync();

                var fruitName = FruitResources.GetFruitName(fruit);

                // Clamp it just to ensure it's valid
                numDrops = Math.Clamp(numDrops, 1, 100);

                if (numDrops == 1)
                {
                    var drops = FruitPantry.HelperFunctions.GetLastNDrops(numDrops, rsn, fruitName);
                    if (drops.Count() == 1)
                    {
                        Embed embed = FruitPantry.HelperFunctions.BuildDropEmbed(drops.First());
                        await FollowupAsync(embed:embed);
                    }
                    else
                    {
                        await FollowupAsync("No drops could be found for this query.");
                    }
                }
                else
                {
                    List<string> output = await FruitPantry.HelperFunctions.BuildLastDropList(numDrops, rsn, fruitName);

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
