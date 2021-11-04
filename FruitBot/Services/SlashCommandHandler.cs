using Discord;
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

            _commands.ComponentCommandExecuted += ComponentCommandExecuted;
            _commands.ContextCommandExecuted += ContextCommandExecuted;
            _commands.SlashCommandExecuted += SlashCommandExecuted;

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

        private async Task SlashCommandExecuted(SlashCommandInfo info, IInteractionCommandContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                Logger.LogError($"SlashCommand {info.Name} failed: {result.ErrorReason}");
                await context.Channel.SendMessageAsync($"{context.User.Mention}", embed: EmbedUtility.FromError("Slash Command Failed", result.ErrorReason, false));
            }
        }

        private async Task ContextCommandExecuted(ContextCommandInfo info, IInteractionCommandContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                Logger.LogError($"Context Command {info.Name} failed: {result.ErrorReason}");
                await context.Channel.SendMessageAsync($"{context.User.Mention}", embed: EmbedUtility.FromError("Context Command Failed", result.ErrorReason, false));
            }
        }

        private async Task ComponentCommandExecuted(ComponentCommandInfo info, IInteractionCommandContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                Logger.LogError($"Component Interaction {info.Name} failed: {result.ErrorReason}");
                await context.Channel.SendMessageAsync($"{context.User.Mention}", embed: EmbedUtility.FromError("Component Interaction Failed", result.ErrorReason, false));
            }
        }
    }
}
