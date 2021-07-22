using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DataTypes;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RSAdventurerLogScraper;

namespace FruitBot.Modules
{
    public class FruitBotCommands : ModuleBase
    {
        private readonly ILogger<FruitBotCommands> _logger;
        private readonly FruitPantry.FruitPantry _thePantry = FruitPantry.FruitPantry.GetFruitPantry();


        public FruitBotCommands(ILogger<FruitBotCommands> logger)
            => _logger = logger;

        [Command("help", RunMode = RunMode.Async)]
        public async Task Help()
        {
            using (Context.Channel.EnterTypingState())
            {
                string botMention = Context.Client.CurrentUser.Mention;


                var builder = new EmbedBuilder()
                        .WithTitle("Usage: ")
                        .WithDescription($"{botMention} **help** - displays this help message. Commands are NOT case-sensitive.\n" +
                                            $"{botMention} **last** <***NumEntries***> - shows the last <*NumEntries*> entries in the drop log. Shows only 1 if number not specified.\n" +
                                            //$"{botMention} **todo** - shows a list of work to be done on the bot.\n" +
                                            $"{botMention} **refresh db** - updates all of the bot's known information from the database. Use this command to propagate changes made by hand to Google Sheets throughout the system.\n" +
                                            $"{botMention} **purge** - deletes everything in the Google Sheets drop log ONLY. leaves all other sheets intact. Admins only.\n" +
                                            $"{botMention} **scrape** - forces a pull of drop log data from Jagex's Runemetrics API no matter how much time has passed since the last auto-scrape.\n" +
                                            $"{botMention} **pull wiki images** - pulls fresh item thumbnails from the RS3 Wiki. WARNING: this command pulls raw images, and can be potentially destructive to the existing database. Admins only.\n" +
                                            $"{botMention} **leaderboard** - shows the current leaderboard for Fruit Wars.\n" +
                                            $"{botMention} **signup** - registers your RSN in the Google Sheets database so that points can be properly awarded to your Fruit Wars team.\n" +
                                            $"{botMention} **points** <***DiscordUser***> - shows fruit wars points gained by the specified Discord user. Shows your own if username not specified.\n" +
                                            $"{botMention} **pointsRSN** <***RuneScapeName***> - shows Fruit Wars points gained by the specified RSN.\n" +
                                            $"{botMention} **test** - dynamic command with various uses - Admins only.\n" +
                                            $"{botMention} **fuck you gob** - for when you *really* need to say it.\n" +
                                            $"{botMention} **nsfw** - you know what this command does.\n" +
                                            $"{botMention} **bosses** - shows a list of eligible bosses and the point values for each.\n" +
                                            $"{botMention} **bugreport** <***Report***> - submit a bug report\n" +
                                            $"{botMention} **suggestion** <***Suggestion***> - submit a suggestion\n" +
                                            $"{botMention} **version** - shows version information for {botMention}.")
                        .WithColor(new Color(00, 00, 255))
                        ;

                var embed = builder.Build();

                await VersionHelper(embed);
                //await Context.Channel.SendMessageAsync(null, false, embed);

            }
            _logger.LogInformation($"{Context.User.Username} executed the help command!");
        }

        // This is starting to spaghetti and needs a complete refactor
        [Command("last", RunMode = RunMode.Async)]
        public async Task Last([Remainder]TypeReaders.LastCommandArguments args)
        {
            using (Context.Channel.EnterTypingState())
            {
                bool remindSingleEntry = false;



                _thePantry.RefreshEverything();
                //FruitPantry.FruitPantry thePantry = FruitPantry.FruitPantry.GetFruitPantry();
                string botMention = Context.Client.CurrentUser.Mention;

                if (args == null)
                {
                    await LastHelper(1, Context);
                    return;
                }

                if (args.DiscordUserFound)
                {
                    args.RSN = _thePantry._discordUsers[args.DiscordUser.Username + "#" + args.DiscordUser.Discriminator][1];
                }

                if (_thePantry.GetDropLog().Count < 1)
                {
                    await ReplyAsync($"There are currently no entries in the drop log to display.");
                }
                else
                {
                    if (!args.NumDropsFound)
                        args.NumDrops = 1;
                    if (args.NumDropsFound && (args.NumDrops == 1))
                    {
                        remindSingleEntry = true;
                    }

                    if (args.NumDrops == 1)
                        await LastHelper(args.NumDrops, Context, args.RSN, args.Fruit);
                    else
                    {
                        List<string> output = await FruitPantry.HelperFunctions.BuildLastDropList(args.NumDrops, args.RSN, args.Fruit);

                        int numMessages = output.Count;

                        if (numMessages > 1)
                        {
                            for (int idx = 0; idx < numMessages; idx++)
                            {
                                output[idx] = $"`{idx + 1}/{numMessages}`\n" + output[idx];
                            }
                        }

                        foreach (string message in output)
                        {
                            await Context.Channel.SendMessageAsync(message, messageReference: new(Context.Message.Id));
                        }
                    }

                    if (remindSingleEntry)
                        await ReplyAsync($"For future reference, if you only want the last (1) drop, you don't have to specify a number, just type \"{botMention} last {{optionalFilter}}\".", messageReference: new(Context.Message.Id));

                }
            }

            _logger.LogInformation($"{Context.User.Username} executed the lastdrop command!");
        }

        public static async Task LastHelper(int numDrops, ICommandContext Context, string rsn = null, string fruit = null)
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
                    if (rsn != null)
                    {
                        if (!entry._playerName.ToLower().Equals(rsn.ToLower()))
                            continue;
                    }
                    if (fruit != null)
                    {
                        if (!entry._fruit.Equals(fruit, StringComparison.OrdinalIgnoreCase))
                            continue;
                    }
                    //quick and dirty fix, remove later
                    string dropIconURL;
                    if (entry._dropIconWEBP == null)
                        dropIconURL = "";
                    else if (entry._dropIconWEBP.Equals("https://runepixels.com/assets/images/runescape/activities/drop.webp"))
                        dropIconURL = "";
                    else
                        dropIconURL = entry._dropIconWEBP;

                    var builder = new EmbedBuilder()
                        .WithImageUrl(thePantry._itemDatabase[entry._dropName.ToLower()]._imageURL)
                        .WithThumbnailUrl(entry._fruitLogo)
                        .WithTitle("Last Drop:")
                        .WithDescription("[Spreadsheet Link](https://docs.google.com/spreadsheets/d/1iCJHsiC4nEjjFz1Gmw4aTldnMFR5ZAlGSuJfHbP262s/edit?usp=sharing)")
                        .WithColor(FruitResources.Colors.Get(entry._fruit))
                        .AddField("Player Name", entry._playerName ?? "null", true)
                        .AddField("Drop", entry._dropName ?? "null", true)
                        .AddField("Points", entry._pointValue, true)
                        .AddField("Dropped At", entry._timestamp, true)
                        .AddField("Boss", entry._bossName, true)
    //.AddField("Fruit", entry._fruit == "" ? "null" : entry._fruit, true)
    //.AddField("Drop Timestamp", entry._timestamp ?? "null", true)
    //.AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
    //.WithCurrentTimestamp()
    ;

                    var embed = builder.Build();

                    await Context.Channel.SendMessageAsync(null, false, embed, messageReference: new MessageReference(Context.Message.Id));
                    idx++;
                }

            }
        }

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

            await Context.Channel.SendMessageAsync(null, false, embed, messageReference: new(Context.Message.Id));



            // dev todo: 

            // fruit bot status message set to "scraping..." while "typing..." and "playing fruit wars" while idle
            // thread locking on -last command
            //  on second thought, change -last command to put everything in one embed
            // crash detection and recovery in timed scraper code


        }

        [Command("refresh db", RunMode = RunMode.Async)]
        public async Task ForceRefresh(SocketGuildUser user = null)
        {
            using (Context.Channel.EnterTypingState())
            {

                int numEntries = _thePantry.RefreshEverything().Count;

                await ReplyAsync($"All databases refreshed from google sheets.", messageReference: new(Context.Message.Id));

            }

            _logger.LogInformation($"{Context.User.Username} executed the force refresh command!");
        }

        [Command("purge", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Purge(SocketGuildUser user = null)
        {

            _logger.LogInformation($"{Context.User.Username} executed the purge command!");
            using (Context.Channel.EnterTypingState())
            {
                FruitPantry.FruitPantry thePantry = FruitPantry.FruitPantry.GetFruitPantry();
                int numEntries = thePantry.PurgeThePantry().Count;


                await ReplyAsync($"Database purged. There are now `{numEntries}` drops stored.", messageReference: new(Context.Message.Id));
            }


        }

        [Command("scrape", RunMode = RunMode.Async)]
        public async Task Scrape(SocketGuildUser user = null)
        {

            await ReplyAsync($"Starting scrape on Runepixels for drop log data. This may take a few minutes.", messageReference: new(Context.Message.Id));

            _logger.LogInformation($"{Context.User.Username} invoked the scrape command. This may take a few minutes.");

            using (Context.Channel.EnterTypingState())
            {
                FruitPantry.FruitPantry thePantry = FruitPantry.FruitPantry.GetFruitPantry();

                int numEntries = thePantry.ScrapeGameData(Context.Client).Result;


                await ReplyAsync($"Scrape was successful. There are now `{numEntries}` entries in the drop log.", messageReference: new(Context.Message.Id));
            }


            _logger.LogInformation($"{Context.User.Username} executed the scrape command!");
        }

        [Command("pull wiki images", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task PullWikiImages(SocketGuildUser user = null)
        {
            _logger.LogInformation($"{Context.User.Username} invoked the scrape command. This may take a while...");
            //await ReplyAsync($"Starting wiki scrape for high-res images. This may take a while...");

            using (Context.Channel.EnterTypingState())
            {

                System.Diagnostics.Stopwatch aStopwatch = new();
                aStopwatch.Start();
                //int numProcessed = FruitPantry.FruitPantry.GetFruitPantry().PullWikiImages();
                aStopwatch.Stop();

                await ReplyAsync($"Yo who tf told you you could use this command");
                //await ReplyAsync($"Scraped wiki for all images. Found and stored `{numProcessed}` images over a total of `{((aStopwatch.ElapsedMilliseconds) / 1000) / 60}` minutes");

                _logger.LogInformation($"{Context.User.Username} executed the pull wiki images command!");
            }

        }

        [Command("leaderboard", RunMode = RunMode.Async)]
        [Alias("lb")]
        public async Task Leaderboard(SocketGuildUser user = null)
        {

            using (Context.Channel.EnterTypingState())
            {
                _thePantry.RefreshEverything();

                float grapePoints = 0;
                float bananaPoints = 0;
                float applePoints = 0;
                float peachPoints = 0;
                float fruitlessHeathenPoints = 0;


                float largestNumber = 0;
                Color leadingColor = FruitResources.Colors.fruitlessHeathen;
                string leadingTeamPictureURL = "https://runescape.wiki/images/b/b8/Ugthanki_dung_detail.png";



                FruitPantry.FruitPantry thePantry = FruitPantry.FruitPantry.GetFruitPantry();

                // Build points values
                foreach (DropLogEntry entry in thePantry.GetDropLog().Values)
                {
                    if (entry._fruit.Equals(FruitResources.Text.grape))
                        grapePoints += float.Parse(entry._pointValue);
                    else if (entry._fruit.Equals(FruitResources.Text.banana))
                        bananaPoints += float.Parse(entry._pointValue);
                    else if (entry._fruit.Equals(FruitResources.Text.apple))
                        applePoints += float.Parse(entry._pointValue);
                    else if (entry._fruit.Equals(FruitResources.Text.peach))
                        peachPoints += float.Parse(entry._pointValue);
                    else
                        fruitlessHeathenPoints += float.Parse(entry._pointValue);
                }
                // Now find the largest one
                if (grapePoints > largestNumber)
                {
                    largestNumber = grapePoints;
                    leadingColor = FruitResources.Colors.grape;
                    leadingTeamPictureURL = FruitResources.Logos.grape;
                }
                if (bananaPoints > largestNumber)
                {
                    largestNumber = bananaPoints;
                    leadingColor = FruitResources.Colors.banana;
                    leadingTeamPictureURL = FruitResources.Logos.banana;
                }
                if (applePoints > largestNumber)
                {
                    largestNumber = applePoints;
                    leadingColor = FruitResources.Colors.apple;
                    leadingTeamPictureURL = FruitResources.Logos.apple;
                }
                if (peachPoints > largestNumber)
                {
                    largestNumber = peachPoints;
                    leadingColor = FruitResources.Colors.peach;
                    leadingTeamPictureURL = FruitResources.Logos.peach;
                }
                if (fruitlessHeathenPoints > largestNumber)
                {
                    largestNumber = fruitlessHeathenPoints;
                    leadingColor = FruitResources.Colors.fruitlessHeathen;
                    leadingTeamPictureURL = "https://runescape.wiki/images/b/b8/Ugthanki_dung_detail.png";
                }



                // Find leading team and assign color/picture based on that


                var builder = new EmbedBuilder()
                            //.WithImageUrl(thePantry._itemDatabase[entry._dropName.ToLower()]._imageURL)
                            //.WithThumbnailUrl(entry._fruitLogo)
                            .WithTitle("Fruit Wars Leaderboard")
                            .WithDescription("[Spreadsheet Link](https://docs.google.com/spreadsheets/d/1iCJHsiC4nEjjFz1Gmw4aTldnMFR5ZAlGSuJfHbP262s/edit?usp=sharing)")
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
                            .WithFooter("Last Update")
                            ;

                var embed = builder.Build();

                await Context.Channel.SendMessageAsync(null, false, embed, messageReference: new(Context.Message.Id));
            }
            //🍇🍌🍎🍑💩

        }

        [Command("signup", RunMode = RunMode.Async)]
        public async Task Signup([Remainder] string throwaway = null)
        {
            await Context.User.SendMessageAsync($"Hello {Context.User.Mention}, you've requested to sign up for Fruit Wars. " +
                $"Please confirm by responding with your Runescape Player Name below in this format (with exact capitalization and any spaces): {Context.Client.CurrentUser.Mention} RSN");

        }

        [Command("points", RunMode = RunMode.Async)]
        public async Task Points(SocketGuildUser user = null, [Remainder] string remainder = null)
        {
            string botMention = Context.Client.CurrentUser.Mention;
            string mention;
            using (Context.Channel.EnterTypingState())
            {
                _thePantry.RefreshEverything();
                string discordTag;
                float result = 0;

                string rsn;
                string fruit;
                string fruitPlural;
                string emoji;
                string thumbnail;
                Discord.Color color;
                if (user == null)
                {
                    discordTag = $"{Context.Message.Author.Username}#{Context.Message.Author.Discriminator}";
                    mention = Context.Message.Author.Mention;
                }
                else
                {
                    discordTag = $"{user.Username}#{user.Discriminator}";
                    mention = user.Mention;
                }
                result = FruitPantry.FruitPantry.PointsCalculator.PointsByDiscordTag(discordTag);
                rsn = _thePantry._discordUsers[discordTag][1];
                fruit = _thePantry._discordUsers[discordTag][0];
                fruitPlural = FruitResources.TextPlural.Get(fruit);
                emoji = FruitResources.Emojis.Get(fruit);
                color = FruitResources.Colors.Get(fruit);
                thumbnail = FruitResources.Logos.Get(fruit);

                var builder = new EmbedBuilder()
                //.WithImageUrl(thePantry._itemDatabase[entry._dropName.ToLower()]._imageURL)
                //.WithThumbnailUrl(entry._fruitLogo)
                .WithDescription($"Fruit Wars contributions for {mention}/RSN {rsn}")
                .WithColor(color)
                .WithThumbnailUrl(thumbnail)
                .AddField($"{emoji}{fruitPlural}{emoji}", $"`{Math.Round(result)}`", true)

                //.AddField("Fruit", entry._fruit == "" ? "null" : entry._fruit, true)
                //.AddField("Drop Timestamp", entry._timestamp ?? "null", true)
                //.AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                .WithCurrentTimestamp()
                ;

                var embed = builder.Build();

                await Context.Channel.SendMessageAsync(null, false, embed, messageReference: new(Context.Message.Id));
                if (user == Context.Message.Author)
                {
                    await Context.Channel.SendMessageAsync($"For future reference, if you want to check your own points you don't have to tag yourself, just type \"{botMention} points\".", messageReference: new(Context.Message.Id));
                }
            }
            _logger.LogInformation($"{Context.User.Username} executed the points command: mention mode for user {mention}!");
        }

        [Command("pointsrsn", RunMode = RunMode.Async)]
        public async Task Points(string playerName = null)
        {
            string botMention = Context.Client.CurrentUser.Mention;
            if (true)
            {
                await ReplyAsync($"Read my lips. \"{botMention} points\"");
                return;
            }
            using (Context.Channel.EnterTypingState())
            {
                _thePantry.RefreshEverything();
                if (playerName == null)
                {
                    await Context.Channel.SendMessageAsync($"Syntax: {botMention} pointsrsn <RSN>", messageReference: new(Context.Message.Id));
                    return;
                }
                float result = FruitPantry.FruitPantry.PointsCalculator.PointsByRSN(playerName);
                string fruit = _thePantry._runescapePlayers[playerName.ToLower()][0];
                string fruitPlural = FruitResources.TextPlural.Get(fruit);
                string emoji = FruitResources.Emojis.Get(fruit);
                string thumbnail = FruitResources.Logos.Get(fruit);
                Discord.Color color = FruitResources.Colors.Get(fruit);
                // TODO: modify code to include discord IDs in database in order to use @mentions in this command.
                //string discordMention = Context.Guild.GetUsersAsync().Result.FirstOrDefault("")

                var builder = new EmbedBuilder()
                //.WithImageUrl(thePantry._itemDatabase[entry._dropName.ToLower()]._imageURL)
                //.WithThumbnailUrl(entry._fruitLogo)
                .WithDescription($"Fruit Wars contributions for {playerName}")
                .WithColor(color)
                .WithThumbnailUrl(thumbnail)
                .AddField($"{emoji}{fruitPlural}{emoji}", $"`{Math.Round(result)}`", true)

                //.AddField("Fruit", entry._fruit == "" ? "null" : entry._fruit, true)
                //.AddField("Drop Timestamp", entry._timestamp ?? "null", true)
                .WithCurrentTimestamp()
                ;

                var embed = builder.Build();

                await Context.Channel.SendMessageAsync(null, false, embed);
            }
            _logger.LogInformation($"{Context.User.Username} executed the points command: RSN mode for player {playerName}!");
        }

        [Command("bugreport")]
        public async Task BugReport([Remainder] string report = null)
        {
            string botMention = Context.Client.CurrentUser.Mention;

            using (Context.Channel.EnterTypingState())
            {
                if (report == null)
                {
                    await Context.Channel.SendMessageAsync($"Syntax: {botMention} bugreport <text>", messageReference: new(Context.Message.Id));
                    return;
                }
                string discordTag = Context.Message.Author.Username + "#" + Context.Message.Author.Discriminator;

                _thePantry.UploadBugReport(discordTag, report, DateTime.Now.ToString());

                await Context.Channel.SendMessageAsync($"Bug report successfully uploaded, thank you! If you'd like, you can see the status of your report in the Google Spreadsheet. " +
                    $"Use \"{botMention} leaderboard\" to get the link to the spreadsheet.", messageReference: new(Context.Message.Id));
            }
        }

        [Command("suggestion")]
        public async Task Suggestion([Remainder] string suggestionText = null)
        {
            string botMention = Context.Client.CurrentUser.Mention;

            using (Context.Channel.EnterTypingState())
            {
                if (suggestionText == null)
                {
                    await Context.Channel.SendMessageAsync($"Syntax: {botMention} suggestion <text>", messageReference: new(Context.Message.Id));
                    return;
                }
                string discordTag = Context.Message.Author.Username + "#" + Context.Message.Author.Discriminator;

                _thePantry.UploadSuggestion(discordTag, suggestionText, DateTime.Now.ToString());

                await Context.Channel.SendMessageAsync($"Suggestion successfully uploaded, thank you! If you'd like, you can see the status of your suggestion in the Google Spreadsheet. " +
                    $"Use \"{botMention} leaderboard\" to get the link to the spreadsheet.", messageReference: new(Context.Message.Id));
            }
        }

        [Command("test", RunMode = RunMode.Async)]
        //[RequireOwner]
        //[RequireUserPermission(GuildPermission.Administrator)]
        public async Task Test([Remainder] TypeReaders.LastCommandArguments args = null)
        {
            bool testing = false;
            if (testing)
            {
                // string output = "";
                // if (args == null)
                // {
                //     await Context.Channel.SendMessageAsync($"No arguments found, no parsing necessary.", messageReference: new(Context.Message.Id));
                //     return;
                // }

                // if (args.NumDropsFound)
                //     output += $"Numdrops: {args.NumDrops}\n";
                // else
                //     output += $"Numdrops: NULL\n";

                // output += $"Fruit: {args.Fruit ?? "NULL"}\n";

                // if (args.DiscordUserFound)
                //     output += $"Discord User: {args.DiscordUser.Mention}\n";
                // else
                //     output += $"Discord User: NULL\n";

                //output += $"RSN: \"{args.RSN ?? "NULL"}\"\n";

                // await Context.Channel.SendMessageAsync(output, messageReference: new(Context.Message.Id));


            }
            else
            {
                await Context.Channel.SendMessageAsync($"There is currently no active testing condition. Nice try bozo.");
            }

            _logger.LogInformation($"{Context.User.Username} executed the test command!");
        }

        [Command("version", RunMode = RunMode.Async)]
        public async Task Version()
        {
            await VersionHelper();
            _logger.LogInformation($"{Context.User.Username} executed the version command!");
        }
        public async Task VersionHelper(Embed embed = null)
        {
            IUserMessage message = await Context.Channel.SendMessageAsync($"Vought FruitBot V{FruitPantry.FruitPantry._version} - {Context.Guild.GetRole(859595508359364629)} Loyalist", false, embed, messageReference: new(Context.Message.Id));
            await message.ModifyAsync(msg => msg.Content = $"Vought FruitBot V{FruitPantry.FruitPantry._version} - {Context.Guild.GetRole(859595508359364629).Mention} Loyalist");
        }

        [Command("fuck you gob", RunMode = RunMode.Async)]
        public async Task FuckYouGob()
        {
            await Context.Channel.SendMessageAsync($"Fuck you <@242069991141146624>", isTTS: false, messageReference: new(Context.Message.Id));
            _logger.LogInformation($"{Context.User.Username} executed the fuck you gob command!");
        }

        [Command("nsfw", RunMode = RunMode.Async)]
        public async Task Nsfw()
        {
            using (Context.Channel.EnterTypingState())
            {
                System.IO.Stream stream;
                string[] splitUrl;
                do
                {
                    HttpClient httpClient = new();
                    string result = await httpClient.GetStringAsync("https://reddit.com/r/dogpictures/random.json?limit=1");
                    JArray array = JArray.Parse(result);
                    JObject post = JObject.Parse(array[0]["data"]["children"][0]["data"].ToString());

                    string imageUrl = post["url"].ToString();

                    System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(imageUrl);
                    webRequest.AllowWriteStreamBuffering = true;
                    webRequest.Timeout = 30000;

                    System.Net.WebResponse webResponse = webRequest.GetResponse();

                    stream = webResponse.GetResponseStream();

                    splitUrl = imageUrl.Split('.');

                } while (splitUrl.Last().Count() > 5);

                await Context.Channel.SendFileAsync(stream, $"nsfw.{splitUrl.Last()}", isSpoiler: true, messageReference: new(Context.Message.Id));
            }
            _logger.LogInformation($"{Context.User.Username} executed the nsfw command!");
        }

        [Command("betasignup", RunMode = RunMode.Async)]
        public async Task BetaSignup([Remainder] string playerName = null)
        {
            IEmote[] fruits = { new Emoji("🍎"), new Emoji("🍌"), new Emoji("🍇"), new Emoji("🍑") };
            //IUserMessage embedMessage = await Context.Channel.SendMessageAsync(embed: EmbedGenerator.SignupEmbed(Context.User), messageReference: new(Context.Message.Id));

            await Context.Message.AddReactionsAsync(fruits, new());
        }

        [Command("bosses", RunMode = RunMode.Async)]
        public async Task Bosses([Remainder] string input = null)
        {
            SortedDictionary<string, float> classifications = _thePantry.GetClassifications();

            var builder = new EmbedBuilder()
                .WithTitle("Points per boss drop:")
                .WithDescription("[Spreadsheet Link](https://docs.google.com/spreadsheets/d/1iCJHsiC4nEjjFz1Gmw4aTldnMFR5ZAlGSuJfHbP262s/edit?usp=sharing)")
                .WithColor(new(0, 255, 0))
                .WithThumbnailUrl("https://runescape.wiki/images/7/74/Zero_weakness_icon.png?acad6");
            ;
            foreach (KeyValuePair<string, float> classification in classifications)
            {
                builder.AddField(classification.Key, classification.Value, inline: true);
            }


            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(null, false, embed, messageReference: new(Context.Message.Id));

        }

    }
}