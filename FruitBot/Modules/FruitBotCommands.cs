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

        [Command("last")]
        public async Task Last(int numDrops = 1, SocketGuildUser user = null)
        {
            _logger.LogDebug($"{Context.User.Username} started the lastdrop command.");
            using (Context.Channel.EnterTypingState())
            {
                await LastDrop(numDrops, user);
            }
            _logger.LogInformation($"{Context.User.Username} executed the lastdrop command!");
        }

        [Command("todo")]
        public async Task ToDo(SocketGuildUser user = null)
        {

            var builder = new EmbedBuilder()
                .WithDescription("Todo list for Admins")
                .WithColor(new Color(255, 00, 0))
                .AddField("Priority 1: ", "Threshold Formula (with variable placeholders)" , true)
                .AddField("Priority 2: ", "Embed Design for Drops: [link](https://robyul.chat/embed-creator)", true)
                .AddField("Low Priority: ", "Finalize Threshold Formula Variables", true)
                //.AddField("Drop Timestamp", entry._timestamp ?? "null", true)
                //.AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                //.WithCurrentTimestamp()
                ;

            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(null, false, embed);

            _logger.LogInformation($"{Context.User.Username} executed the todo command!");
        }
        private async Task LastDrop(int numDrops = 1, SocketGuildUser user = null)
        {

            FruitPantry.FruitPantry thePantry = new("FruitBot", "1iCJHsiC4nEjjFz1Gmw4aTldnMFR5ZAlGSuJfHbP262s", "Drop Log", "credentials.json");
            //List<DropLogEntry> scraped = await DropLogEntry.CreateListFullAuto();


            //await thePantry.Add(scraped);

            //DropLogEntry lastEntry = thePantry._masterList.Last().Value;

            if (user == null)
            {
                int idx = 0;
                foreach (KeyValuePair<string, DropLogEntry> entryPair in thePantry._masterList)
                {
                    if (idx == numDrops)
                        break;
                    DropLogEntry entry = entryPair.Value;

                    var builder = new EmbedBuilder()
                        .WithImageUrl(entry._playerAvatarPNG ?? "null")
                        .WithThumbnailUrl(entry._dropIconWEBP ?? "null")
                        .WithDescription("Last Drop: ")
                        .WithColor(new Color(00, 00, 255))
                        .AddField("Player Name", entry._playerName ?? "null", true)
                        .AddField("Drop", entry._dropName ?? "null", true)
                        .AddField("Fruit", entry._fruit == "" ? "null" : entry._fruit, true)
                        //.AddField("Drop Timestamp", entry._timestamp ?? "null", true)
                        //.AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                        //.WithCurrentTimestamp()
                        ;

                    var embed = builder.Build();

                    await Context.Channel.SendMessageAsync(null, false, embed);
                    idx++;
                }
            }
        }
    }




}
