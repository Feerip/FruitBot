using DataTypes;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using RS3APIDropLog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FruitBot.Modules
{
    public class FruitBotCommands : ModuleBase
    {
        private readonly ILogger<FruitBotCommands> _logger;
        private readonly FruitPantry.FruitPantry _thePantry = FruitPantry.FruitPantry.GetFruitPantry();

        public FruitBotCommands(ILogger<FruitBotCommands> logger)
        {
            _logger = logger;
        }

        [Command("help", RunMode = RunMode.Async)]
        public async Task Help()
        {
            using (Context.Channel.EnterTypingState())
            {
                string botMention = Context.Client.CurrentUser.Mention;


                EmbedBuilder builder = new EmbedBuilder()
                        .WithTitle("Usage: ")
                        .WithDescription($"{botMention} **help** - displays this help message. Commands are NOT case-sensitive.\n" +
                                            $"{botMention} **last** <***NumEntries***> - shows the last <*NumEntries*> entries in the drop log. Shows only 1 if number not specified.\n" +
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
                                            $"{botMention} **coinflip** - flips a coin.\n" +
                                            $"{botMention} **spreadsheet** - link to the bot control spreadsheet (public view-only, admin edit)\n" +
                                            $"{botMention} **bugreport** <***Report***> - submit a bug report\n" +
                                            $"{botMention} **suggestion** <***Suggestion***> - submit a suggestion\n" +
                                            $"{botMention} **version** - shows version information for {botMention}.")
                        .WithColor(new Color(00, 00, 255))
                        ;

                Embed embed = builder.Build();

                await VersionHelper(embed);

            }
            _logger.LogInformation($"{Context.User.Username} executed the help command!");
        }

        [Command("spreadsheet", RunMode = RunMode.Async)]
        public async Task Spreadsheet()
        {
            using (Context.Channel.EnterTypingState())
            {
                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle("Spreadsheet Link")
                    .WithUrl("https://docs.google.com/spreadsheets/d/1iCJHsiC4nEjjFz1Gmw4aTldnMFR5ZAlGSuJfHbP262s/edit?usp=sharing")
                    .WithDescription("Bot Control Spreadsheet (public view-only, admin edit)");

                Embed embed = builder.Build();

                await Context.Channel.SendMessageAsync(embed: embed, messageReference: new(Context.Message.Id));
            }
            _logger.LogInformation($"{Context.User.Username} executed the spreadsheet command!");
        }

        // This is starting to spaghetti and needs a complete refactor
        [Command("last", RunMode = RunMode.Async)]
        public async Task Last([Remainder] TypeReaders.LastCommandArguments args = null)
        {
            using (Context.Channel.EnterTypingState())
            {
                bool remindSingleEntry = false;



                _thePantry.RefreshEverything();
                string botMention = Context.Client.CurrentUser.Mention;

                if (args == null)
                {
                    await LastHelper(1, Context);
                    return;
                }

                if (args.DiscordUserFound)
                {
                    args.RSN = _thePantry._discordUsers[args.DiscordUser.Id.ToString()][1];
                }

                if (_thePantry.GetDropLog().Count < 1)
                {
                    await ReplyAsync($"There are currently no entries in the drop log to display.");
                }
                else
                {
                    if (!args.NumDropsFound)
                    {
                        args.NumDrops = 1;
                    }

                    if (args.NumDropsFound && (args.NumDrops == 1))
                    {
                        remindSingleEntry = true;
                    }

                    if (args.NumDrops == 1)
                    {
                        await LastHelper(args.NumDrops, Context, args.RSN, args.Fruit);
                    }
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
                    {
                        await ReplyAsync($"For future reference, if you only want the last (1) drop, you don't have to specify a number, just type \"{botMention} last {{optionalFilter}}\".", messageReference: new(Context.Message.Id));
                    }
                }
            }

            _logger.LogInformation($"{Context.User.Username} executed the lastdrop command!");
        }

        public static async Task LastHelper(int numDrops, ICommandContext Context, string rsn = null, string fruit = null)
        {
            using (Context.Channel.EnterTypingState())
            {
                FruitPantry.FruitPantry thePantry = FruitPantry.FruitPantry.GetFruitPantry();

                int idx = 0;

                var pantryLastDropsFirst = thePantry.GetDropLog();
                pantryLastDropsFirst.Reverse();

                foreach (DropLogEntry entry in pantryLastDropsFirst)
                {


                    if (idx == numDrops)
                    {
                        break;
                    }

                    DropLogEntry newEntry = entry;
                    if (rsn != null)
                    {
                        if (!newEntry._playerName.ToLower().Equals(rsn.ToLower()))
                        {
                            continue;
                        }
                    }
                    if (fruit != null)
                    {
                        if (!newEntry._fruit.Equals(fruit, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }
                    //quick and dirty fix, remove later
                    string dropIconURL;
                    if (newEntry._dropIconWEBP == null)
                    {
                        dropIconURL = "";
                    }
                    else if (newEntry._dropIconWEBP.Equals("https://runepixels.com/assets/images/runescape/activities/drop.webp"))
                    {
                        dropIconURL = "";
                    }
                    else
                    {
                        dropIconURL = newEntry._dropIconWEBP;
                    }

                    EmbedBuilder builder = new EmbedBuilder()
                        .WithThumbnailUrl(thePantry._itemDatabase[newEntry._dropName.ToLower()]._imageURL)
                        .WithTitle(newEntry._dropName ?? "null")
                        .WithColor(thePantry._classificationColorList[newEntry._bossName])
                        .AddField("Player Name", newEntry._playerName ?? "null", true)
                        .WithCurrentTimestamp()
                        ;


#if FRUITWARSMODE
                    builder.AddField("Points", newEntry._pointValue, true);
#endif
                    builder.AddField("Boss", newEntry._bossName, true);
                    builder.AddField("Dropped At", newEntry._timestamp, true);

                    Embed embed = builder.Build();

                    await Context.Channel.SendMessageAsync(null, false, embed, messageReference: new MessageReference(Context.Message.Id));
                    idx++;
                }

            }
        }

        [Command("todo", RunMode = RunMode.Async)]
        public async Task ToDo(SocketGuildUser user = null)
        {

            EmbedBuilder builder = new EmbedBuilder()
                .WithDescription("Todo list for Admins")
                .WithColor(new Color(255, 00, 0))
                .AddField("Priority 1: ", "Threshold Formula (with variable placeholders)", true)
                .AddField("Priority 2: ", "Embed Design for Drops: [link](https://robyul.chat/embed-creator)", true)
                .AddField("Low Priority: ", "Finalize Threshold Formula Variables", true)
                ;

            Embed embed = builder.Build();

            await Context.Channel.SendMessageAsync(null, false, embed, messageReference: new(Context.Message.Id));
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
        [Alias("pull")]
        public async Task Scrape(SocketGuildUser user = null)
        {
            using (Context.Channel.EnterTypingState())
            {
                FruitPantry.FruitPantry thePantry = FruitPantry.FruitPantry.GetFruitPantry();
                int numEntries;
                if (FruitPantry.FruitPantry._scrapingSem.CurrentCount > 0)
                {
                    await ReplyAsync($"Starting pull from Runemetrics for drop log data. This may take a few minutes.", messageReference: new(Context.Message.Id));
                    _logger.LogInformation($"{Context.User.Username} invoked the scrape command. This may take a few minutes.");

                    numEntries = thePantry.ScrapeGameData((DiscordSocketClient)Context.Client).Result;
                    await ReplyAsync($"Scrape was successful. There are now `{numEntries}` entries in the drop log.", messageReference: new(Context.Message.Id));

                }
                else
                {
                    await ReplyAsync($"Already scraping. Wait your damn turn.", messageReference: new(Context.Message.Id));
                }
            }


            _logger.LogInformation($"{Context.User.Username} executed the scrape command!");
        }

        [Command("pull wiki images", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task PullWikiImages(SocketGuildUser user = null)
        {
            _logger.LogInformation($"{Context.User.Username} invoked the scrape command. This may take a while...");

            using (Context.Channel.EnterTypingState())
            {

                System.Diagnostics.Stopwatch aStopwatch = new();
                aStopwatch.Start();
                aStopwatch.Stop();

                await ReplyAsync($"Yo who tf told you you could use this command");

                _logger.LogInformation($"{Context.User.Username} executed the pull wiki images command!");
            }

        }

        [Command("leaderboard", RunMode = RunMode.Async)]
        [Alias("lb")]
        public async Task Leaderboard(SocketGuildUser user = null)
        {

            //string nickname = Context.Guild.GetUserAsync(1234).Result.Nickname;


            using (Context.Channel.EnterTypingState())
            {
                _thePantry.RefreshEverything();

                float bananaPoints = 0;
                float kiwiPoints = 0;
                float watermelonPoints = 0;
                float beanPoints = 0;
                float fruitlessHeathenPoints = 0;


                float largestNumber = 0;
                Color leadingColor = FruitResources.Colors.fruitlessHeathen;
                string leadingTeamPictureURL = "https://runescape.wiki/images/b/b8/Ugthanki_dung_detail.png";



                FruitPantry.FruitPantry thePantry = FruitPantry.FruitPantry.GetFruitPantry();

                // Build points values
                foreach (DropLogEntry entry in thePantry.GetDropLog())
                {
                    if (entry._fruit.Equals(FruitResources.Text.banana))
                    {
                        bananaPoints += float.Parse(entry._pointValue);
                    }
                    else if (entry._fruit.Equals(FruitResources.Text.kiwi))
                    {
                        kiwiPoints += float.Parse(entry._pointValue);
                    }
                    else if (entry._fruit.Equals(FruitResources.Text.watermelon))
                    {
                        watermelonPoints += float.Parse(entry._pointValue);
                    }
                    else if (entry._fruit.Equals(FruitResources.Text.bean))
                    {
                        beanPoints += float.Parse(entry._pointValue);
                    }
                    else
                    {
                        fruitlessHeathenPoints += float.Parse(entry._pointValue);
                    }
                }
                // Now find the largest one
                if (bananaPoints > largestNumber)
                {
                    largestNumber = bananaPoints;
                    leadingColor = FruitResources.Colors.banana;
                    leadingTeamPictureURL = FruitResources.Logos.banana;
                }
                if (kiwiPoints > largestNumber)
                {
                    largestNumber = kiwiPoints;
                    leadingColor = FruitResources.Colors.kiwi;
                    leadingTeamPictureURL = FruitResources.Logos.kiwi;
                }
                if (watermelonPoints > largestNumber)
                {
                    largestNumber = watermelonPoints;
                    leadingColor = FruitResources.Colors.watermelon;
                    leadingTeamPictureURL = FruitResources.Logos.watermelon;
                }
                if (beanPoints > largestNumber)
                {
                    largestNumber = beanPoints;
                    leadingColor = FruitResources.Colors.bean;
                    leadingTeamPictureURL = FruitResources.Logos.bean;
                }
                if (fruitlessHeathenPoints > largestNumber)
                {
                    largestNumber = fruitlessHeathenPoints;
                    leadingColor = FruitResources.Colors.fruitlessHeathen;
                    leadingTeamPictureURL = "https://runescape.wiki/images/b/b8/Ugthanki_dung_detail.png";
                }



                // Find leading team and assign color/picture based on that


                EmbedBuilder builder = new EmbedBuilder()
                            .WithTitle("Fruit Wars Leaderboard")
                            .WithDescription("[Spreadsheet Link](https://docs.google.com/spreadsheets/d/1iCJHsiC4nEjjFz1Gmw4aTldnMFR5ZAlGSuJfHbP262s/edit?usp=sharing)")
                            .WithColor(leadingColor)
                            .WithThumbnailUrl(leadingTeamPictureURL)
                            .AddField("🍌Bananas🍌", $"`{Math.Round(bananaPoints)}`", true)
                            .AddField("\u200B", '\u200B', true)
                            .AddField("🥝Kiwis🥝", $"`{Math.Round(kiwiPoints)}`", true)
                            .AddField("🍉Watermelons🍉", $"`{Math.Round(watermelonPoints)}`", true)
                            .AddField("\u200B", '\u200B', true)
                            .AddField("🫘Beans🫘", $"`{Math.Round(beanPoints)}`", true)
                            .AddField("\u200B", '\u200B', false)
                            .AddField("💩Fruitless Heathens💩", $"`{Math.Round(fruitlessHeathenPoints)}`", false)
                            .WithCurrentTimestamp()
                            .WithFooter("Last Update")
                            ;

                Embed embed = builder.Build();

                await Context.Channel.SendMessageAsync(null, false, embed, messageReference: new(Context.Message.Id));
            }
            //🍌🥝🍉🫘💩

        }

        [Command("signup", RunMode = RunMode.Async)]
        public async Task Signup([Remainder] string throwaway = null)
        {
            await Context.Channel.TriggerTypingAsync();
            await Context.Message.ReplyAsync(text: "Check your DMs <:doge:774545768564391947>");

            await Context.User.SendMessageAsync($"Hello {Context.User.Mention}, you've requested to sign up for Fruit Wars. " +
                $"Please confirm by responding with your Runescape Player Name below in this format (with exact capitalization and any spaces): {Context.Client.CurrentUser.Mention} RSN");
        }

        [Command("points", RunMode = RunMode.Async)]
        public async Task Points([Remainder] TypeReaders.LastCommandArguments args = null)
        {
            string botMention = Context.Client.CurrentUser.Mention;
            string mention;
            using (Context.Channel.EnterTypingState())
            {
                _thePantry.RefreshEverything();
                ulong discordID = Context.Message.Author.Id;
                mention = Context.Message.Author.Mention;
                float result = 0;

                string rsn;
                string fruit;
                string fruitPlural;
                string emoji;
                string thumbnail;
                Discord.Color color;

                bool fruitMode = false;
                bool allMode = false;

                if (args == null)
                {
                    // Use defaults aka context message values
                }
                else if (args.DiscordUserFound)
                {
                    discordID = args.DiscordUser.Id;
                    mention = args.DiscordUser.Mention;
                }
                else if (args.RSNFound)
                {
                    discordID = ulong.Parse(_thePantry._runescapePlayers[args.RSN][1]);
                    mention = null;
                }
                else if (args.FruitFound)
                {
                    fruitMode = true;
                }
                else if (args.EveryoneFlagFound)
                {
                    allMode = true;
                }

                if (fruitMode || allMode)
                {
                    SortedDictionary<string, float> players;
                    if (fruitMode)
                    {
                        players = FruitPantry.FruitPantry.PointsCalculator.PointsOfFruitTeamMembers(args.Fruit);
                    }
                    else if (allMode)
                    {
                        players = FruitPantry.FruitPantry.PointsCalculator.PointsOfAllParticipants();
                    }
                    else
                    {
                        players = null;
                    }

                    List<string> output = await FruitPantry.HelperFunctions.BuildPointsList(players, args.Fruit);

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
                else
                {
                    result = FruitPantry.FruitPantry.PointsCalculator.PointsByDiscordID(discordID);
                    rsn = _thePantry._discordUsers[discordID.ToString()][1];
                    fruit = _thePantry._discordUsers[discordID.ToString()][0];
                    fruitPlural = FruitResources.TextPlural.Get(fruit);
                    emoji = FruitResources.Emojis.Get(fruit);
                    color = FruitResources.Colors.Get(fruit);
                    thumbnail = FruitResources.Logos.Get(fruit);

                    EmbedBuilder builder = new EmbedBuilder()
                    .WithDescription($"Fruit Wars contributions for {mention}/RSN {rsn}")
                    .WithColor(color)
                    .WithThumbnailUrl(thumbnail)
                    .AddField($"{emoji}{fruitPlural}{emoji}", $"`{Math.Round(result)}`", true)
                    .WithCurrentTimestamp()
                    ;

                    Embed embed = builder.Build();

                    await Context.Channel.SendMessageAsync(null, false, embed, messageReference: new(Context.Message.Id));
                    if ((args != null) && (args.DiscordUser == Context.Message.Author))
                    {
                        await Context.Channel.SendMessageAsync($"For future reference, if you want to check your own points you don't have to tag yourself, just type \"{botMention} points\".", messageReference: new(Context.Message.Id));
                    }
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
                await ReplyAsync($"Read. my. lips. \"{botMention} points\"", messageReference: new(Context.Message.Id));
                return;
            }
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
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Test([Remainder] TypeReaders.LastCommandArguments args = null)
        {
            bool testing = true;
            if (testing)
            {
                RaidsSignup signup = new();

                Embed embed = signup.GenerateEmbed();

                await Context.Channel.SendMessageAsync(null, embed: embed);

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
            //IUserMessage message = await Context.Channel.SendMessageAsync($"Vought FruitBot V{FruitPantry.FruitPantry._version} - {Context.Guild.GetRole(859595508359364629)} Loyalist", false, embed, messageReference: new(Context.Message.Id));
            //await message.ModifyAsync(msg => msg.Content = $"Vought FruitBot V{FruitPantry.FruitPantry._version} - {Context.Guild.GetRole(859595508359364629).Mention} Loyalist");
            IUserMessage message = await Context.Channel.SendMessageAsync($"Vought FruitBot V{FruitPantry.FruitPantry._version}", false, embed, messageReference: new(Context.Message.Id));
            await message.ModifyAsync(msg => msg.Content = $"Vought FruitBot V{FruitPantry.FruitPantry._version}");
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
            IEmote[] fruits = { new Emoji("🍉"), new Emoji("🥝"), new Emoji("🍌"), new Emoji("🫘") };

            await Context.Message.AddReactionsAsync(fruits, new());
        }

        [Command("bosses", RunMode = RunMode.Async)]
        public async Task Bosses([Remainder] string input = null)
        {
            SortedDictionary<string, float> classifications = _thePantry.GetClassifications();
            int thresholdValue = _thePantry._universalThresholdValue;
            float thresholdMultiplier = _thePantry._thresholdMultiplier;

            decimal totalClassifications = classifications.Count;
            decimal fieldsPerEmbed = 24;
            int numEmbedsNeeded = (int)Math.Ceiling(totalClassifications / fieldsPerEmbed);
            Embed[] embeds = new Embed[numEmbedsNeeded];
            EmbedBuilder[] builders = new EmbedBuilder[numEmbedsNeeded];

            for (int idx = 0; idx < numEmbedsNeeded; idx++)
            {
                builders[idx] = new EmbedBuilder()
                    .WithColor(new(0, 255, 0))
                ;
            }
            builders[0].Title = "Points per drop:";
            builders[0].Description = "[Spreadsheet Link](https://docs.google.com/spreadsheets/d/1iCJHsiC4nEjjFz1Gmw4aTldnMFR5ZAlGSuJfHbP262s/edit?usp=sharing)";
            builders[0].ThumbnailUrl = "https://runescape.wiki/images/7/74/Zero_weakness_icon.png?acad6";
            EmbedFooterBuilder footerBuilder = new EmbedFooterBuilder()
                .WithText($"Threshold (# of drops): {thresholdValue}, Threshold Multiplier: {thresholdMultiplier}.");
            builders[numEmbedsNeeded-1].Footer = footerBuilder;

            int currentClassificationNumber = 1;
            int currentEmbed = 0;
            foreach (KeyValuePair<string, float> classification in classifications)
            {
                builders[currentEmbed].AddField(classification.Key, classification.Value, inline: true);
                if (currentClassificationNumber++%fieldsPerEmbed == 0) 
                {
                    currentEmbed++;
                }
            }

            for (int idx = 0; idx < numEmbedsNeeded; idx++)
            {
                embeds[idx] = builders[idx].Build();
            }

            await Context.Channel.SendMessageAsync(null, false, embeds: embeds, messageReference: new(Context.Message.Id));

        }

        [Command("fuck me gob", RunMode = RunMode.Async)]
        public async Task FuckMeGob(string input = null)
        {
            if (Context.Message.Author.Id == 746368167617495151)
            {
                await Context.Channel.SendMessageAsync($"Hello sir <@!242069991141146624>, {Context.Message.Author.Mention} has requested sexual gratification. Please see to it at your earliest convenience.", messageReference: new(Context.Message.Id));
            }
            else
            {
                await Context.Channel.SendMessageAsync($"https://i.kym-cdn.com/entries/icons/original/000/008/910/IF0yE_copy.jpg", messageReference: new(Context.Message.Id));
            }
            _logger.LogInformation($"{Context.User.Username} executed the fuck me gob command!");
        }

        [Command("coinflip", RunMode = RunMode.Async)]
        public async Task CoinFlip(string input = null)
        {
            int coin = _thePantry._rand.Next(2);

            if (coin == 0)
            {
                await Context.Channel.SendMessageAsync("https://cdn.discordapp.com/attachments/769476224363397144/870832802877292585/Head.png", messageReference: new(Context.Message.Id));
            }
            else if (coin == 1)
            {
                await Context.Channel.SendMessageAsync("https://cdn.discordapp.com/attachments/769476224363397144/870832807566528552/tails.png", messageReference: new(Context.Message.Id));
            }
            else
            {
                // Throw in low iq meme here later
                throw new ArithmeticException();
            }

            _logger.LogInformation($"{Context.User.Username} executed the coinflip command! Result: {(coin == 0 ? "heads" : "tails")}");

        }

        [Command("echo", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Echo([Remainder] string input)
        {
            using (Context.Channel.EnterTypingState())
            {
                await Context.Channel.SendMessageAsync(input);
            }

            _logger.LogInformation($"{Context.User.Username} executed the echo command! Content: \"{input}\"");

        }

        [Command("stop", RunMode = RunMode.Async)]
        [Alias("Restart", "Update")]
        //[RequireUserPermission(GuildPermission.Administrator)]
        public async Task Stop()
        {
            _logger.LogInformation($"{Context.User.Username} executed the stop command! Exiting now.");
            await Context.Channel.SendMessageAsync("Stop command acknowledged. Restarting now. If I'm not back within 30 seconds, cry for help.", messageReference: new(Context.Message.Id));

            DiscordSocketClient client = (DiscordSocketClient)Context.Client;
            await client.SetStatusAsync(UserStatus.DoNotDisturb);
            await client.SetGameAsync($"Be Right Back...", type: ActivityType.Playing);
            Environment.Exit(0);
        }
    }
}