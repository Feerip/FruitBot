using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RSAdventurerLogScraper;

namespace FruitBot.Modules
{
    public class FruitBotCommands : ModuleBase
    {
        private readonly ILogger<FruitBotCommands> _logger;

        public FruitBotCommands(ILogger<FruitBotCommands> logger)
            => _logger = logger;

        [Command("last", RunMode = RunMode.Async)]
        public async Task Last(int numDrops = 1, SocketGuildUser user = null)
        {
            FruitPantry.FruitPantry thePantry = FruitPantry.FruitPantry.GetFruitPantry();
            if (user == null)
            {
                if (thePantry.GetDropLog().Count < 1)
                {
                    await ReplyAsync($"There are currently no entries in the drop log to display.");
                }
                else
                {

                    await LastHelper(numDrops, Context);
                }
            }

            _logger.LogInformation($"{Context.User.Username} executed the lastdrop command!");
        }

        public static async Task LastHelper(int numDrops, ICommandContext Context)
        {
            using (Context.Channel.EnterTypingState())
            {
                FruitPantry.FruitPantry thePantry = FruitPantry.FruitPantry.GetFruitPantry();


                //DropLogEntry lastEntry = thePantry._masterList.Last().Value;

                int idx = 0;

                foreach (KeyValuePair<string, DropLogEntry> entryPair in thePantry.GetDropLog())
                {
                    if (idx == numDrops)
                        break;
                    DropLogEntry entry = entryPair.Value;

                    //quick and dirty fix, remove later
                    string dropIconURL;
                    if (entry._dropIconWEBP == null)
                        dropIconURL = "";
                    else if (entry._dropIconWEBP.Equals("https://runepixels.com/assets/images/runescape/activities/drop.webp"))
                        dropIconURL = "";
                    else
                        dropIconURL = entry._dropIconWEBP;

                    var builder = new EmbedBuilder()
                        //.WithImageUrl("https://cdn.discordapp.com/attachments/856679881547186196/859871618436562944/bandoshelmet_50.webp")
                        //.WithImageUrl(entry._dropIconWEBP ?? "null") 
                        .WithImageUrl(thePantry._itemDatabase[entry._dropName.ToLower()]._imageURL)
                        //.WithThumbnailUrl(entry._dropIconWEBP ?? "null")
                        .WithThumbnailUrl(entry._fruitLogo)
                        .WithDescription("Last Drop: ")
                        .WithColor(new Color(00, 00, 255))
                        .AddField("Player Name", entry._playerName ?? "null", true)
                        .AddField("Drop", entry._dropName ?? "null", true)
                        //.AddField("Fruit", entry._fruit == "" ? "null" : entry._fruit, true)
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


        //private async Task LastDrop(int numDrops = 1, SocketGuildUser user = null)
        //{
        //    FruitPantry.FruitPantry thePantry = FruitPantry.FruitPantry.GetFruitPantry();


        //    //DropLogEntry lastEntry = thePantry._masterList.Last().Value;

        //    if (user == null) 
        //    {
        //        int idx = 0;
        //        foreach (KeyValuePair<string, DropLogEntry> entryPair in thePantry._dropLog)
        //        {
        //            if (idx == numDrops)
        //                break;
        //            DropLogEntry entry = entryPair.Value;

        //            var builder = new EmbedBuilder()
        //                .WithImageUrl("https://cdn.discordapp.com/attachments/856679881547186196/859871618436562944/bandoshelmet_50.webp")
        //                //.WithImageUrl(entry._dropIconWEBP ?? "null")
        //                //.WithThumbnailUrl(entry._dropIconWEBP ?? "null")
        //                .WithThumbnailUrl("https://cdn.discordapp.com/attachments/856679881547186196/859869023607717898/GrapeThing.png")
        //                .WithDescription("Last Drop: ")
        //                .WithColor(new Color(00, 00, 255))
        //                .AddField("Player Name", entry._playerName ?? "null", true)
        //                .AddField("Drop", entry._dropName ?? "null", true)
        //                //.AddField("Fruit", entry._fruit == "" ? "null" : entry._fruit, true)
        //                //.AddField("Drop Timestamp", entry._timestamp ?? "null", true)
        //                //.AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
        //                //.WithCurrentTimestamp()
        //                ;

        //            var embed = builder.Build();

        //            await Context.Channel.SendMessageAsync(null, false, embed);
        //            idx++;
        //        }
        //    }
        //}
        [Command("todo", RunMode = RunMode.Async)]
        public async Task ToDo(SocketGuildUser user = null)
        {

            var builder = new EmbedBuilder()
                .WithDescription("Todo list for Admins")
                .WithColor(new Color(255, 00, 0))
                .AddField("Priority 1: ", "Threshold Formula (with variable placeholders)", true)
                .AddField("Priority 2: ", "Embed Design for Drops: [link](https://robyul.chat/embed-creator)", true)
                .AddField("Low Priority: ", "Finalize Threshold Formula Variables", true)
                //.AddField("Drop Timestamp", entry._timestamp ?? "null", true)
                //.AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                //.WithCurrentTimestamp()
                ;

            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(null, false, embed);



            // dev todo: 
            // fruit integration
            // fruit association with person
            // person association with RSN
            // good bot bad bot 
            // -nsfw
            // total point per team calculator and interfaces
            // fruit bot status message set to "scraping..." while "typing..." and "playing fruit wars" while idle
        }

        [Command("refresh db", RunMode = RunMode.Async)]
        public async Task ForceRefresh(SocketGuildUser user = null)
        {
            using (Context.Channel.EnterTypingState())
            {

                FruitPantry.FruitPantry thePantry = FruitPantry.FruitPantry.GetFruitPantry();

                int numEntries = thePantry.RefreshDropLog().Count;
                thePantry.RefreshClassifications();
                thePantry.RefreshItemDatabase();
                thePantry.RefreshPlayerDatabase();
                thePantry.RefreshThresholdValues();

                await ReplyAsync($"All databases refreshed from google sheets.");

            }

            _logger.LogInformation($"{Context.User.Username} executed the force refresh command!");
        }

        [Command("purge", RunMode = RunMode.Async)]
        public async Task Purge(SocketGuildUser user = null)
        {

            _logger.LogInformation($"{Context.User.Username} executed the purge command!");
            using (Context.Channel.EnterTypingState())
            {
                FruitPantry.FruitPantry thePantry = FruitPantry.FruitPantry.GetFruitPantry();
                int numEntries = thePantry.PurgeThePantry().Count;


                await ReplyAsync($"Database purged. There are now `{numEntries}` drops stored.");
            }


        }

        [Command("scrape", RunMode = RunMode.Async)]
        public async Task Scrape(SocketGuildUser user = null)
        {

            await ReplyAsync($"Starting scrape on Runepixels for drop log data. This may take a few minutes.");

            _logger.LogInformation($"{Context.User.Username} invoked the scrape command. This may take a few minutes.");


            using (Context.Channel.EnterTypingState())
            {
                FruitPantry.FruitPantry thePantry = FruitPantry.FruitPantry.GetFruitPantry();

                int numEntries = thePantry.ScrapeGameData(Context.Client).Result;


                await ReplyAsync($"Scrape was successful. There are now `{numEntries}` entries in the drop log.");
            }


            _logger.LogInformation($"{Context.User.Username} executed the scrape command!");
        }

        [Command("pull wiki images", RunMode = RunMode.Async)]
        public async Task PullWikiImages(SocketGuildUser user = null)
        {
            _logger.LogInformation($"{Context.User.Username} invoked the scrape command. This may take a while...");
            await ReplyAsync($"Starting wiki scrape for high-res images. This may take a while...");

            using (Context.Channel.EnterTypingState())
            {

                System.Diagnostics.Stopwatch aStopwatch = new();
                aStopwatch.Start();
                int numProcessed = FruitPantry.FruitPantry.GetFruitPantry().PullWikiImages();
                aStopwatch.Stop();


                await ReplyAsync($"Scraped wiki for all images. Found and stored `{numProcessed}` images over a total of `{((aStopwatch.ElapsedMilliseconds) / 1000) / 60}` minutes");

                _logger.LogInformation($"{Context.User.Username} executed the pull wiki images command!");
            }

        }

        [Command("leaderboard", RunMode = RunMode.Async)]
        public async Task Leaderboard(SocketGuildUser user = null)
        {

            float grapePoints = 0;
            float bananaPoints = 0;
            float applePoints = 0;
            float peachPoints = 0;
            float fruitlessHeathenPoints = 0;

            Color grape = new(128, 00, 128);
            Color banana = new(255, 255, 0);
            Color apple = new(255, 0, 0);
            Color peach = new(255, 192, 203);
            Color fruitlessHeathen = new(150, 75, 0);

            float largestNumber = 0;
            Color leadingColor = fruitlessHeathen;
            string leadingTeamPictureURL = "https://runescape.wiki/images/b/b8/Ugthanki_dung_detail.png";



            FruitPantry.FruitPantry thePantry = FruitPantry.FruitPantry.GetFruitPantry();

            // Build points values
            foreach (DropLogEntry entry in thePantry.GetDropLog().Values)
            {
                if (entry._fruit.Equals("Grape"))
                    grapePoints += float.Parse(entry._pointValue);
                else if (entry._fruit.Equals("Banana"))
                    bananaPoints += float.Parse(entry._pointValue);
                else if (entry._fruit.Equals("Apple"))
                    applePoints += float.Parse(entry._pointValue);
                else if (entry._fruit.Equals("Peach"))
                    peachPoints += float.Parse(entry._pointValue);
                else
                    fruitlessHeathenPoints += float.Parse(entry._pointValue);
            }
            // Now find the largest one
            if (grapePoints > largestNumber)
            {
                largestNumber = grapePoints;
                leadingColor = grape;
                leadingTeamPictureURL = DropLogEntry.FruitLogos.GrapeLogo;
            }
            if (bananaPoints > largestNumber)
            {
                largestNumber = bananaPoints;
                leadingColor = banana;
                leadingTeamPictureURL = DropLogEntry.FruitLogos.BananaLogo;
            }
            if (applePoints > largestNumber)
            {
                largestNumber = applePoints;
                leadingColor = apple;
                leadingTeamPictureURL = DropLogEntry.FruitLogos.AppleLogo;
            }
            if (peachPoints > largestNumber)
            {
                largestNumber = peachPoints;
                leadingColor = peach;
                leadingTeamPictureURL = DropLogEntry.FruitLogos.PeachLogo;
            }
            if (fruitlessHeathenPoints > largestNumber)
            {
                largestNumber = fruitlessHeathenPoints;
                leadingColor = fruitlessHeathen;
                leadingTeamPictureURL = "https://runescape.wiki/images/b/b8/Ugthanki_dung_detail.png";
            }



            // Find leading team and assign color/picture based on that


            var builder = new EmbedBuilder()
                        //.WithImageUrl(thePantry._itemDatabase[entry._dropName.ToLower()]._imageURL)
                        //.WithThumbnailUrl(entry._fruitLogo)
                        .WithDescription("Fruit Wars Leaderboard")
                        .WithColor(leadingColor)
                        .WithThumbnailUrl(leadingTeamPictureURL)
                        .AddField("🍇Grapes🍇", $"`{Math.Round(grapePoints)}`", true)
                        .AddField("\u200B", '\u200B', true)
                        .AddField("🍌Bananas🍌", $"`{Math.Round(bananaPoints)}`", true)
                        .AddField("🍎Apples🍎", $"`{Math.Round(applePoints)}`", true)
                        .AddField("\u200B", '\u200B', true)
                        .AddField("🍑Peaches🍑", $"`{Math.Round(peachPoints)}`", true)
                        .AddField("\u200B", '\u200B', false)
                        .AddField("💩Fruitless Heathens💩", $"`{Math.Round(fruitlessHeathenPoints)}`", false)
                        
                        //.AddField("Fruit", entry._fruit == "" ? "null" : entry._fruit, true)
                        //.AddField("Drop Timestamp", entry._timestamp ?? "null", true)
                        //.AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                        .WithCurrentTimestamp()
                        ;

            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(null, false, embed);

            //🍇🍌🍎🍑💩

        }

        
    }


}
