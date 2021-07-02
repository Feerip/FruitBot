using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FruitPantry
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        //private int executionCount = 0;
        private readonly ILogger<TimedHostedService> _logger;
        private Timer _timer;
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
