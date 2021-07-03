using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RSAdventurerLogScraper;

namespace FruitPantry
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        //private int executionCount = 0;
        private readonly ILogger<TimedHostedService> _logger;
        private Timer _timer;
        private Timer _timer2;
        private DiscordSocketClient _client;

        public TimedHostedService(/*ILogger<TimedHostedService> logger,*/ DiscordSocketClient client)
        {
            //_logger = logger;
            _client = client;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            //_logger.LogInformation("FruitPantry Background Tasks Service running.");

            _timer = new Timer(DoWork, _client, TimeSpan.Zero,
                TimeSpan.FromMinutes(5));

            return Task.CompletedTask;
        }

        public Task LeaderboardAtResetStartAsync(CancellationToken stoppingToken)
        {
            //_timer2 = new Timer(SomeMethod, _client, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            SomeMethod(_client);

            return Task.CompletedTask;
        }

        void SomeMethod(object state)
        {
            //var currentTime = DateTime.Now;
            var targetTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 17, 00, 00);
            var oneMinute = new TimeSpan(0, 1, 0);

            while (true)
            {
                //Console.WriteLine("========================Oneminute (REMOVE ME)");
                Thread.Sleep(45000);

                if (targetTime - DateTime.Now < oneMinute)
                {
                    //Console.WriteLine("===========================Task Fired (REMOVE ME)");
                    ShowLeaderboard(state);
                    //Reset timer to do again next day
                    targetTime = targetTime.AddDays(1);

                    Thread.Sleep(60000);
                }
            }

        }

        void ShowLeaderboard(object state)
        {
            using (_client.GetGuild(769476224363397140).GetTextChannel(856679881547186196).EnterTypingState())
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



                FruitPantry thePantry = FruitPantry.GetFruitPantry();

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

                 _client.GetGuild(769476224363397140).GetTextChannel(856679881547186196).SendMessageAsync(null, false, embed);

                //🍇🍌🍎🍑💩

            }
            return;
        }

        private void DoWork(object state)
        {
            //var count = Interlocked.Increment(ref executionCount);

            //ICommandContext context = (CommandContext)state;





            using (_client.GetGuild(769476224363397140).GetTextChannel(856679881547186196).EnterTypingState())
            {
                FruitPantry thePantry = FruitPantry.GetFruitPantry();

                //_client.GetGuild(769476224363397140).GetTextChannel(856679881547186196).SendMessageAsync($"Starting automatic scrape on Runepixels for drop log data. This may take a few minutes.");

                int numTotalEntries = thePantry.ScrapeGameData(_client).Result;

                _ = HelperFunctions.LastHelper(FruitPantry.NumNewEntries, _client);

                FruitPantry.NumNewEntries = 0;

                //_client.GetGuild(769476224363397140).GetTextChannel(856679881547186196).SendMessageAsync($"Scrape was successful. There are now `{numEntries}` entries in the drop log.");
            }

            //_client.GetGuild(769476224363397140).GetTextChannel(856679881547186196).SendMessageAsync($"Hello from DoWork()!!!");

            //_logger.LogInformation("Background scrape fired. Now scraping Runepixels and updating both internal and google sheets databases.");
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            //_logger.LogInformation("FruitPantry Background Tasks Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
