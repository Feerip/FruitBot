using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FruitBot.Services
{
    internal class DiscordHostedService : IHostedService
    {
        private readonly ILogger<DiscordHostedService> _logger;
        private readonly BaseSocketClient _client;
        private readonly IConfiguration _config;

        public DiscordHostedService(ILogger<DiscordHostedService> logger, LogAdapter adapter, DiscordSocketClient client, IConfiguration config)
        {
            _logger = logger;
            _client = client;
            _client.Log += adapter.Log;
            _config = config;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Discord.NET hosted service is starting");

            try
            {
                await _client.LoginAsync(TokenType.Bot, _config["token"]);
                await _client.StartAsync();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Startup has been aborted, exiting...");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Discord.NET hosted service is stopping");
            try
            {
                await _client.StopAsync();
            }
            catch (OperationCanceledException)
            {
                _logger.LogCritical("Discord.NET client could not be stopped within the given timeout and may have permanently deadlocked");
            }
        }
    }

    internal class LogAdapter
    {
        private readonly ILogger<DiscordSocketClient> _logger;
        private readonly Func<LogMessage, Exception, string> _formatter;

        public LogAdapter(ILogger<DiscordSocketClient> logger, DiscordSocketConfig config, DiscordSocketClient client)
        {
            _logger = logger;
            _formatter = (LogMessage msg, Exception ex) => $"{msg}{ex}";
            client.Log += Log;
        }

        public Task Log(LogMessage message)
        {
            _logger.Log(GetLogLevel(message.Severity), default, message, message.Exception, _formatter);
            return Task.CompletedTask;
        }

        private static LogLevel GetLogLevel(LogSeverity severity)
            => severity switch
            {
                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Warning => LogLevel.Warning,
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Verbose => LogLevel.Debug,
                LogSeverity.Debug => LogLevel.Trace,
                _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
            };
    }
}
