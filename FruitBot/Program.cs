#define DEBUG_LIMITS
#define FRUITWARSMODE
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using FruitBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FruitBot
{
    public class Program
    {
        private static async Task<int> Main(string[] args)
        {
            IHostBuilder builder = new HostBuilder()
                .ConfigureAppConfiguration(x =>
                {
                    IConfigurationRoot configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
#if DEBUG
                        .AddJsonFile("Config/appsettings.debug.json", false, true)
#else
                        .AddJsonFile("Config/appsettings.json", false, true)
#endif
                        .Build();

                    x.AddConfiguration(configuration);
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Trace); // Defines what kind of information should be logged (e.g. Debug, Information, Warning, Critical) adjust this to your liking
                })
                // TODO: use this once the Discord.Net.Labs.Interactions is merged into labs. Then we can re-use the hosting addon.
                //.ConfigureDiscordHost((context, config) =>
                //{
                //    config.SocketConfig = new DiscordSocketConfig
                //    {
                //        LogLevel = LogSeverity.Verbose, // Defines what kind of information should be logged from the API (e.g. Verbose, Info, Warning, Critical) adjust this to your liking
                //                    AlwaysDownloadUsers = true,
                //        MessageCacheSize = 1000,
                //        GatewayIntents = GatewayIntents.All,
                //        LargeThreshold = 250,

                //    };

                //    config.Token = context.Configuration["token"];

                //})
                //.UseCommandService((context, config) =>
                //{
                //    config.CaseSensitiveCommands = false;
                //    config.LogLevel = LogSeverity.Debug;
                //    config.DefaultRunMode = Discord.Commands.RunMode.Sync;
                //})
                .ConfigureServices((context, services) =>
                {
                    services
                    .AddSingleton(new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Verbose,
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 1000,
                        GatewayIntents = GatewayIntents.All,
                        LargeThreshold = 250,
                    })
                    .AddSingleton<DiscordSocketClient>()
                    .AddSingleton<LogAdapter>()
                    .AddHostedService<Services.DiscordHostedService>()
                    .AddSingleton(new CommandServiceConfig()
                    {
                        CaseSensitiveCommands = false,
                        LogLevel = LogSeverity.Debug,
                        DefaultRunMode = Discord.Commands.RunMode.Sync
                    })
                    .AddSingleton<CommandService>()
                    .AddHostedService<CommandHandler>()
                    .AddSingleton(new InteractionServiceConfig
                    {
                        DefaultRunMode = Discord.Interactions.RunMode.Async,
                        LogLevel = LogSeverity.Debug,
                        UseCompiledLambda = true,
                        ThrowOnError = true,
                        WildCardExpression = "*"
                    })
                    .AddSingleton<InteractionService>()
                    .AddHostedService<SlashCommandHandler>();
                    // TODO: use this once the Discord.Net.Labs.Interactions is merged into labs. Then we can re-use the hosting addon.
                    //services
                    //.AddHostedService<CommandHandler>()
                    //.AddSingleton(new InteractionServiceConfig
                    //{
                    //    DefaultRunMode = Discord.Interactions.RunMode.Async,
                    //    LogLevel = LogSeverity.Debug,
                    //    UseCompiledLambda = true,
                    //    ThrowOnError = true,
                    //    WildCardExpression = "*"
                    //})
                    //.AddSingleton<InteractionService>()
                    //.AddHostedService<SlashCommandHandler>();
                })
                .UseConsoleLifetime();

            Environment.ExitCode = 1;

            IHost host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }

            return 1;
        }
    }
}