using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FruitBot.Services
{
    internal class SlashCommandHandler : DiscordClientService
    {
        IServiceProvider _serviceProvider;
        InteractionService _commands;

        public SlashCommandHandler(DiscordSocketClient client, ILogger<DiscordClientService> logger, IServiceProvider provider, InteractionService interactionService) 
            : base(client, logger)
        {
            _serviceProvider = provider;
            _commands = interactionService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Load the commands.
            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);

            // Wait for ready event to fire.
            await Client.WaitForReadyAsync(stoppingToken);

            Client.InteractionCreated += Client_InteractionCreated;

            // Register commands
            foreach (SocketGuild guild in Client.Guilds)
            {
                await _commands.RegisterCommandsToGuildAsync(guild.Id);
            }
        }

        private async Task Client_InteractionCreated(SocketInteraction arg)
        {
            var context = new SocketInteractionCommandContext(Client, arg);

            try
            {
                await _commands.ExecuteCommandAsync(context, _serviceProvider);
            }
            catch (Exception ex)
            {
                await arg.RespondAsync($"Command Failed: {ex}", ephemeral: true);
            }
        }
    }
}
