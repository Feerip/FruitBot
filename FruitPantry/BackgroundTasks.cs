using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RS3APIDropLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace FruitPantry
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<TimedHostedService> _logger;
        private System.Timers.Timer _timer2;
        private readonly DiscordSocketClient _client;

        public TimedHostedService(DiscordSocketClient client)
        {
            _client = client;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer2 = new(150000);
            _timer2.Elapsed += DoWork;
            _timer2.AutoReset = true;
            _timer2.Enabled = true;

            return Task.CompletedTask;
        }

        public Task LeaderboardAtResetStartAsync(CancellationToken stoppingToken, int dailyHourToBroadcast, int hourlyMinuteToBroadcast = 00)
        {
            BroadcastLeaderboardAtDesignatedTimesOfDay(_client, dailyHourToBroadcast, hourlyMinuteToBroadcast);

            return Task.CompletedTask;
        }

        private void BroadcastLeaderboardAtDesignatedTimesOfDay(object state, int dailyHourToBroadcast, int hourlyMinuteToBroadcast)
        {
            DateTime targetTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, dailyHourToBroadcast, hourlyMinuteToBroadcast, 00);
            TimeSpan oneMinute = new TimeSpan(0, 1, 0);
            TimeSpan zero = new TimeSpan(0, 0, 0);

            while (true)
            {
                Thread.Sleep(45000);

                // This means we've already passed that time today at startup, so we need to compensate for it
                if (targetTime - DateTime.Now < zero)
                {
                    targetTime = targetTime.AddDays(1);
                }

                // This means we're at this time now.
                if (targetTime - DateTime.Now < oneMinute)
                {
                    ShowLeaderboard(state);
                    //Reset timer to do again next day
                    targetTime = targetTime.AddDays(1);

                    Thread.Sleep(60000);
                }
            }

        }

        private void ShowLeaderboard(object state)
        {
#if DEBUG
            using (_client.GetGuild(1088977050750173207).GetTextChannel(1088984348549713961).EnterTypingState())
#else
            using (_client.GetGuild(769476224363397140).GetTextChannel(862385904719364096).EnterTypingState())
#endif
            {


                float pineapplePoints = 0;
                float kiwiPoints = 0;
                float applePoints = 0;
                float beanPoints = 0;
                float fruitlessHeathenPoints = 0;

                Color pineapple = new(128, 00, 128);
                Color kiwi = new(255, 255, 0);
                Color apple = new(255, 0, 0);
                Color bean = new(255, 192, 203);
                Color fruitlessHeathen = new(150, 75, 0);

                float largestNumber = 0;
                Color leadingColor = fruitlessHeathen;
                string leadingTeamPictureURL = "https://runescape.wiki/images/b/b8/Ugthanki_dung_detail.png";



                FruitPantry thePantry = FruitPantry.GetFruitPantry();

                // Build points values
                foreach (DropLogEntry entry in thePantry.GetDropLog())
                {
                    if (entry._fruit.Equals("Pineapple"))
                    {
                        pineapplePoints += float.Parse(entry._pointValue);
                    }
                    else if (entry._fruit.Equals("Kiwi"))
                    {
                        kiwiPoints += float.Parse(entry._pointValue);
                    }
                    else if (entry._fruit.Equals("Apple"))
                    {
                        applePoints += float.Parse(entry._pointValue);
                    }
                    else if (entry._fruit.Equals("Bean"))
                    {
                        beanPoints += float.Parse(entry._pointValue);
                    }
                    else
                    {
                        fruitlessHeathenPoints += float.Parse(entry._pointValue);
                    }
                }
                // Now find the largest one
                if (pineapplePoints > largestNumber)
                {
                    largestNumber = pineapplePoints;
                    leadingColor = pineapple;
                    leadingTeamPictureURL = DataTypes.FruitResources.Logos.pineapple;
                }
                if (kiwiPoints > largestNumber)
                {
                    largestNumber = kiwiPoints;
                    leadingColor = kiwi;
                    leadingTeamPictureURL = DataTypes.FruitResources.Logos.kiwi;
                }
                if (applePoints > largestNumber)
                {
                    largestNumber = applePoints;
                    leadingColor = apple;
                    leadingTeamPictureURL = DataTypes.FruitResources.Logos.apple;
                }
                if (beanPoints > largestNumber)
                {
                    largestNumber = beanPoints;
                    leadingColor = bean;
                    leadingTeamPictureURL = DataTypes.FruitResources.Logos.bean;
                }
                if (fruitlessHeathenPoints > largestNumber)
                {
                    largestNumber = fruitlessHeathenPoints;
                    leadingColor = fruitlessHeathen;
                    leadingTeamPictureURL = DataTypes.FruitResources.Logos.fruitlessHeathen;
                }



                // Find leading team and assign color/picture based on that


                EmbedBuilder builder = new EmbedBuilder()
                            .WithTitle("Fruit Wars Leaderboard")
                            .WithDescription("[Spreadsheet Link](https://docs.google.com/spreadsheets/d/1iCJHsiC4nEjjFz1Gmw4aTldnMFR5ZAlGSuJfHbP262s/edit?usp=sharing)")
                            .WithColor(leadingColor)
                            .WithThumbnailUrl(leadingTeamPictureURL)
                            .AddField("🍍Pineapples🍍", $"`{Math.Round(pineapplePoints)}`", true)
                            .AddField("\u200B", '\u200B', true)
                            .AddField("🥝Kiwis🥝", $"`{Math.Round(kiwiPoints)}`", true)
                            .AddField("🍎Apples🍎", $"`{Math.Round(applePoints)}`", true)
                            .AddField("\u200B", '\u200B', true)
                            .AddField("🫘Beans🫘", $"`{Math.Round(beanPoints)}`", true)
                            .AddField("\u200B", '\u200B', false)
                            .AddField("💩Fruitless Heathens💩", $"`{Math.Round(fruitlessHeathenPoints)}`", false)
                            .WithCurrentTimestamp()
                            ;

                Embed embed = builder.Build();
#if DEBUG
                _client.GetGuild(1088977050750173207).GetTextChannel(1088984348549713961).SendMessageAsync(null, false, embed);
#else
                _client.GetGuild(769476224363397140).GetTextChannel(862385904719364096).SendMessageAsync(null, false, embed);
#endif

                //🍍🥝🍎🫘💩

            }
            return;
        }

        private async void DoWork(object state, ElapsedEventArgs e)
        {
#if DEBUG
            using (_client.GetGuild(1088977050750173207).GetTextChannel(1088984348549713961).EnterTypingState())
#else
            using (_client.GetGuild(769476224363397140).GetTextChannel(862385904719364096).EnterTypingState())
#endif
            {
                FruitPantry thePantry = FruitPantry.GetFruitPantry();

                if (FruitPantry._scrapingSem.CurrentCount > 0)
                {
                    int numTotalEntries = await thePantry.ScrapeGameData(_client);
                }
                else
                {
                    Console.WriteLine("Scraper busy. Skipping timed interval scrape.");
                }
                //await HelperFunctions.LastHelper(FruitPantry.NumNewEntries, _client);

                //FruitPantry.NumNewEntries = 0;

            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer2.Stop();
            _timer2.Dispose();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer2.Stop();
            _timer2.Dispose();
        }
    }
}
