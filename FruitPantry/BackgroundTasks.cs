using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RS3APIDropLog;

namespace FruitPantry
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<TimedHostedService> _logger;
        private System.Timers.Timer _timer2;
        private DiscordSocketClient _client;

        public TimedHostedService( DiscordSocketClient client)
        {
            _client = client;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer2 = new(300000);
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

        void BroadcastLeaderboardAtDesignatedTimesOfDay(object state, int dailyHourToBroadcast, int hourlyMinuteToBroadcast)
        {
            var targetTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, dailyHourToBroadcast, hourlyMinuteToBroadcast, 00);
            var oneMinute = new TimeSpan(0, 1, 0);
            var zero = new TimeSpan(0, 0, 0);

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

        void ShowLeaderboard(object state)
        {
            using (_client.GetGuild(769476224363397140).GetTextChannel(862385904719364096).EnterTypingState())
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
                    leadingTeamPictureURL = DataTypes.FruitResources.Logos.grape;
                }
                if (bananaPoints > largestNumber)
                {
                    largestNumber = bananaPoints;
                    leadingColor = banana;
                    leadingTeamPictureURL = DataTypes.FruitResources.Logos.banana;
                }
                if (applePoints > largestNumber)
                {
                    largestNumber = applePoints;
                    leadingColor = apple;
                    leadingTeamPictureURL = DataTypes.FruitResources.Logos.apple;
                }
                if (peachPoints > largestNumber)
                {
                    largestNumber = peachPoints;
                    leadingColor = peach;
                    leadingTeamPictureURL = DataTypes.FruitResources.Logos.peach;
                }
                if (fruitlessHeathenPoints > largestNumber)
                {
                    largestNumber = fruitlessHeathenPoints;
                    leadingColor = fruitlessHeathen;
                    leadingTeamPictureURL = DataTypes.FruitResources.Logos.fruitlessHeathen;
                }



                // Find leading team and assign color/picture based on that


                var builder = new EmbedBuilder()
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
                            .WithCurrentTimestamp()
                            ;

                var embed = builder.Build();

                 _client.GetGuild(769476224363397140).GetTextChannel(862385904719364096).SendMessageAsync(null, false, embed);

                //🍇🍌🍎🍑💩

            }
            return;
        }

        private async void DoWork(object state, ElapsedEventArgs e)
        {
            using (_client.GetGuild(769476224363397140).GetTextChannel(862385904719364096).EnterTypingState())
            {
                FruitPantry thePantry = FruitPantry.GetFruitPantry();


                int numTotalEntries = thePantry.ScrapeGameData(_client).Result;

                await HelperFunctions.LastHelper(FruitPantry.NumNewEntries, _client);

                FruitPantry.NumNewEntries = 0;

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
