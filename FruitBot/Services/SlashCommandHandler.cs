using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
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
    internal class SlashCommandHandler : BackgroundService
    {
        private DiscordSocketClient Client;
        private ILogger<SlashCommandHandler> Logger;
        private IServiceProvider _serviceProvider;
        private InteractionService _commands;

        public SlashCommandHandler(DiscordSocketClient client, ILogger<SlashCommandHandler> logger, IServiceProvider provider, InteractionService interactionService) 
        {
            Client = client;
            Logger = logger;
            _serviceProvider = provider;
            _commands = interactionService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Load the commands.
            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);

            // Wait for ready event to fire.
            Client.Ready += async () =>
            {

                Client.InteractionCreated += Client_InteractionCreated;

                _commands.ComponentCommandExecuted += ComponentCommandExecuted;
                _commands.ContextCommandExecuted += ContextCommandExecuted;
                _commands.SlashCommandExecuted += SlashCommandExecuted;
                _commands.Log += _commands_Log;

                await _commands.RegisterCommandsToGuildAsync(769476224363397140);
                // Register commands
                foreach (SocketGuild guild in Client.Guilds)
                {
                    await _commands.RegisterCommandsToGuildAsync(guild.Id);
                }
                await _commands.RegisterCommandsToGuildAsync(769476224363397140);
            };
        }

        private Task _commands_Log(LogMessage arg)
        {
            Logger.LogInformation(arg.Message);
            return Task.CompletedTask;
        }

        private async Task Client_InteractionCreated(SocketInteraction arg)
        {
            var context = new SocketInteractionContext(Client, arg);

            try
            {
                await _commands.ExecuteCommandAsync(context, _serviceProvider);
            }
            catch (Exception ex)
            {
                await arg.RespondAsync($"Command Failed: {ex}", ephemeral: true);
            }
        }

        private async Task SlashCommandExecuted(SlashCommandInfo info, IInteractionContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                Logger.LogError($"SlashCommand {info.Name} failed: {result.ErrorReason}");
                await context.Channel.SendMessageAsync($"{context.User.Mention}", embed: EmbedUtility.FromError("Slash Command Failed", result.ErrorReason, false));
            }
        }

        private async Task ContextCommandExecuted(ContextCommandInfo info, IInteractionContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                Logger.LogError($"Context Command {info.Name} failed: {result.ErrorReason}");
                await context.Channel.SendMessageAsync($"{context.User.Mention}", embed: EmbedUtility.FromError("Context Command Failed", result.ErrorReason, false));
            }
        }

        private async Task ComponentCommandExecuted(ComponentCommandInfo info, IInteractionContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                Logger.LogError($"Component Interaction {info.Name} failed: {result.ErrorReason}");
                await context.Channel.SendMessageAsync($"{context.User.Mention}", embed: EmbedUtility.FromError("Component Interaction Failed", result.ErrorReason, false));
            }
        }
    }
}
