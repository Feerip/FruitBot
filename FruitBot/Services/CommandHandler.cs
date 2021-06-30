using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace FruitBot.Services
{
    public class CommandHandler : InitializedService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _config;

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service, IConfiguration config)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;
            

            _service.CommandExecuted += OnCommandExecuted;

            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            //_client.ReactionAdded += OnReactionAdded;
            if (arg3.MessageId != 857061904944988160) return;
            if (arg3.Emote.Name != "✅") return;
            var role = (arg2 as SocketGuildChannel).Guild.Roles.FirstOrDefault(x => x.Id == 856709182514397194);
            await (arg3.User.Value as SocketGuildUser).AddRoleAsync(role);

            
        }

        private async Task OnJoinedGuild(SocketGuild arg)
        {
            //_client.JoinedGuild += OnJoinedGuild;
            await arg.DefaultChannel.SendMessageAsync("Bot joined the server");
        }

        private async Task OnChannelCreated(SocketChannel arg)
        {
            //_client.ChannelCreated += OnChannelCreated;
            if ((arg as ITextChannel) == null) return;
            var channel = arg as ITextChannel;

            await channel.SendMessageAsync("Channel created");
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            if (!message.HasStringPrefix(_config["prefix"], ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            await _service.ExecuteAsync(context, argPos, _provider);
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess)
            {
                await context.Channel.SendMessageAsync($"Error: {result}");
            }
        }
    }
}