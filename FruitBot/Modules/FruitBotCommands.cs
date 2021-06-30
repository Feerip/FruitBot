using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using RSAdventurerLogScraper;

namespace FruitBot.Modules
{
    public class FruitBotCommands : ModuleBase
    {
        private readonly ILogger<FruitBotCommands> _logger;

        public FruitBotCommands(ILogger<FruitBotCommands> logger)
            => _logger = logger;

        [Command("lastdrop")]
        public async Task LastDrop(SocketGuildUser user = null)
        {
            _logger.LogDebug($"{Context.User.Username} started the lastdrop command.");
            using (Context.Channel.EnterTypingState())
            {
                FruitPantry.FruitPantry thePantry = new("FruitBot", "1iCJHsiC4nEjjFz1Gmw4aTldnMFR5ZAlGSuJfHbP262s", "Drop Log", "credentials.json");
                //List<DropLogEntry> scraped = await DropLogEntry.CreateListFullAuto();


                //await thePantry.Add(scraped);

                DropLogEntry lastEntry = thePantry._masterList.Last().Value;

                if (user == null)
                {
                    var zbuilder = new EmbedBuilder()
                        //.WithThumbnailUrl(lastEntry._dropIconWEBP ?? "null")
                        //.WithImageUrl(lastEntry._dropIconWEBP ?? "null")
                        .WithDescription("Last Drop: ")
                        .WithColor(new Color(00, 00, 255))
                        .AddField("Player Name", lastEntry._playerName ?? "null", true)
                        .AddField("Drop", lastEntry._dropName ?? "null", true)
                        .AddField("Fruit", lastEntry._fruit ?? "null", true)
                        .AddField("Drop Timestamp", lastEntry._timestamp ?? "null", true);
                        //.AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                        //.WithCurrentTimestamp();

                    var embed = zbuilder.Build();

                    await Context.Channel.SendMessageAsync(null, false, embed);
                }
            }
            _logger.LogInformation($"{Context.User.Username} executed the lastdrop command!");
        }
    }


}
