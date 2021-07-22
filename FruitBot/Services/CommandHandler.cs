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
using FruitPantry;
using System.Collections.Generic;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Timers;
using OpenQA.Selenium.Remote;
using FruitBot.Modules;

namespace FruitBot.Services
{
    public class CommandHandler : InitializedService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _config;
        private readonly FruitPantry.FruitPantry _thePantry = FruitPantry.FruitPantry.GetFruitPantry();
        //private readonly ReliabilityService _reliabilityService;

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service, IConfiguration config)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
            //_reliabilityService = new ReliabilityService(_client);


        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;

            // Add LastCommandTypeReader to read type for command "last"
            _service.AddTypeReader(typeof(TypeReaders.LastCommandArguments), new TypeReaders.LastCommandTypeReader());



            _service.CommandExecuted += OnCommandExecuted;
            _client.Ready += SetStatusAsync;
            _client.Ready += _client_Ready;


            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task SetStatusAsync()
        {
            //await _client.SetGameAsync($"@FruitBot help", null, ActivityType.Listening);
            await _client.SetGameAsync($"Fruit Wars | @FruitBot help", null, ActivityType.Playing);
        }

        private async Task _client_Ready()
        {
            TimedHostedService service = new(_client);
            Task backgroundScraper = service.StartAsync(new CancellationToken());

            // Watchdog event to kill the entire program when the Discord.net API glitches out and disconnects. 
            // Calling powershell script will detect the exit and restart it indefinitely.
            // Issues: Calling Environment.Exit(1) to signal that the script should restart is not working properly. Program always exits with exit code 0.
            // Until that's resolved, the powershell script needs to always restart it, but once we get this working it can restart on exit code 1 and 
            //  stay closed on exit code 0.

            var timer = new System.Timers.Timer(10000);
            timer.Elapsed += new ElapsedEventHandler(CheckConnection);
            timer.Start();
            //service.LeaderboardAtResetStartAsync(new CancellationToken(), 23);
            //service.LeaderboardAtResetStartAsync(new CancellationToken(), 05);
            //service.LeaderboardAtResetStartAsync(new CancellationToken(), 11);
            //await service.LeaderboardAtResetStartAsync(new CancellationToken(), 17);

            List<int> intlist = new List<int> { 17, 23, 05, 11 };

            Parallel.ForEach(intlist, (i) => service.LeaderboardAtResetStartAsync(new CancellationToken(), i, 02));


            //service.LeaderboardAtResetStartAsync(new CancellationToken(), 05, 00);
            //service.LeaderboardAtResetStartAsync(new CancellationToken(), 04, 58);
            //service.LeaderboardAtResetStartAsync(new CancellationToken(), 04, 56);
            //await service.LeaderboardAtResetStartAsync(new CancellationToken(), 04, 54);

        }

        private void CheckConnection(object sender, ElapsedEventArgs e)
        {
            Environment.ExitCode = 1;
            if (_client.ConnectionState == ConnectionState.Disconnecting)
                Environment.Exit(1);

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
            var context = new SocketCommandContext(_client, message);
            if (message.Source != MessageSource.User) return;
            var argPos = 0;



            if (context.IsPrivate)
            {
                await OnPrivateMessageReceived(arg, context, message, argPos);
                return;
            }

            if (message.Content.ToLower().Equals("good bot"))
            {
                FruitPantry.FruitPantry.VoteResponse response = _thePantry.QueryGoodBot();

                await context.Channel.SendMessageAsync(response.message, messageReference: new(context.Message.Id));
                await context.Channel.SendMessageAsync($"V{FruitPantry.FruitPantry._version} Upvotes: `{response.goodBot}`, Downvotes: `{response.badBot}`");
                // This is on purpose, only send the upvote/downvote tally if the user is not being abusive to my poor baby
            }

            if (message.Content.ToLower().Equals("bad bot"))
            {
                FruitPantry.FruitPantry.VoteResponse response = _thePantry.QueryBadBot();

                await context.Channel.SendMessageAsync(response.message, messageReference: new(context.Message.Id));
            }

            if (message.Content.ToLower().Equals("good gob"))
            {
                FruitPantry.FruitPantry.VoteResponse response = _thePantry.QueryGoodGob();

                //await context.Channel.SendMessageAsync(response.message, messageReference: new(context.Message.Id));
                await context.Channel.SendMessageAsync($"Upvotes: `{response.goodBot}`, Downvotes: `{response.badBot}`", messageReference: new(context.Message.Id));
                // This is on purpose, only send the upvote/downvote tally if the user is not being abusive to my poor baby
            }
            if (message.Content.ToLower().Equals("bad gob"))
            {
                FruitPantry.FruitPantry.VoteResponse response = _thePantry.QueryBadGob();
                await context.Channel.SendMessageAsync($"Upvotes: `{response.goodBot}`, Downvotes: `{response.badBot}`", messageReference: new(context.Message.Id));
            }

            if (/*!message.HasStringPrefix(_config["prefix"], ref argPos) &&*/ !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;


            await _service.ExecuteAsync(context, argPos, _provider);


        }

        private async Task OnPrivateMessageReceived(SocketMessage arg, SocketCommandContext context, SocketUserMessage message, int argPos)
        {
            _thePantry.RefreshEverything();
            Random rand = new(DateTime.Now.Millisecond);
            List<string> grapeJob = new();
            grapeJob.Add("Also 🍇Grape🍇 is the superior fruit.");
            grapeJob.Add("Also 🍇Grape🍇#1");
            grapeJob.Add("Also if 🍇Grapes🍇 don't win it's rigged.");
            grapeJob.Add("Also 🍇Grapes🍇 control the bot, just sayin.");


            if (!message.HasMentionPrefix(_client.CurrentUser, ref argPos))
                return;
            else
            {
                string[] buffer = context.Message.ToString().Split(' ');
                string RSNInput = "";

                for (int idx = 1; idx < buffer.Count(); idx++)
                {
                    RSNInput += buffer[idx];
                    if (idx < buffer.Count() - 1 && !buffer[idx].Equals(" "))
                        RSNInput += " ";
                }


                string userDiscordTag = context.Message.Author.Username + "#" + context.Message.Author.Discriminator;

                using (context.Channel.EnterTypingState())
                {
                    FruitPantry.FruitPantry thePantry = FruitPantry.FruitPantry.GetFruitPantry();
                    ulong userID = context.Message.Author.Id;
                    IReadOnlyCollection<SocketRole> userRoles = _client.GetGuild(769476224363397140).GetUser(userID).Roles;

                    SocketRole grapeRole = _client.GetGuild(769476224363397140).Roles.First(x => x.Name == "Grapes");
                    SocketRole appleRole = _client.GetGuild(769476224363397140).Roles.First(x => x.Name == "Apples");
                    SocketRole peachRole = _client.GetGuild(769476224363397140).Roles.First(x => x.Name == "Peaches");
                    SocketRole bananaRole = _client.GetGuild(769476224363397140).Roles.First(x => x.Name == "Bananas");
                    string userTeam;
                    string userTeamIcon;
                    if (userRoles.Contains(grapeRole))
                    {
                        userTeam = "Grape";
                        userTeamIcon = "🍇";
                    }
                    else if (userRoles.Contains(appleRole))
                    {
                        userTeam = "Apple";
                        userTeamIcon = "🍎";
                    }
                    else if (userRoles.Contains(peachRole))
                    {
                        userTeam = "Peach";
                        userTeamIcon = "🍑";
                    }
                    else if (userRoles.Contains(bananaRole))
                    {
                        userTeam = "Banana";
                        userTeamIcon = "🍌";
                    }
                    else
                    {
                        userTeam = "Fruitless Heathen";
                        userTeamIcon = "💩";
                    }

                    if (userTeam.Equals("Fruitless Heathen"))
                    {
                        await context.User.SendMessageAsync($"Sorry, 💩fruitless heathens💩 are not eligible to participate in Fruit Wars. Please join a team before registering with {_client.CurrentUser.Mention}.");
                        await context.User.SendMessageAsync($"If this is not correct, please let an admin know so we can fix the issue.");
                        return;
                    }

                    // ADDROLE NOT WORKING - REPLACE LATER
                    //SocketRole verifiedRole = _client.GetGuild(769476224363397140).Roles.First(x => x.Name == "RSN Verified");
                    if (/*userRoles.Contains(verifiedRole) || */thePantry._discordUsers.ContainsKey(userDiscordTag))
                    {
                        await context.User.SendMessageAsync($"You have already signed up for fruit wars and will not be added again.");
                        await context.User.SendMessageAsync($"If this is not correct, please let an admin know so we can fix the issue.");
                        return;
                    }

                    if (thePantry._runescapePlayers.ContainsKey(RSNInput))
                    {
                        await context.User.SendMessageAsync($"RSN {RSNInput} has already been claimed and registered to {thePantry._runescapePlayers[buffer.Last()][1]} and will not be added again.");
                        await context.User.SendMessageAsync($"If this is not correct, please let an admin know so we can fix the issue.");
                        return;
                    }

                    thePantry.RegisterPlayer(RSNInput, userTeam, userDiscordTag);
                    // ADDROLE NOT WORKING - REPLACE LATER
                    //await _client.GetGuild(769476224363397140).GetUser(userID).AddRoleAsync(verifiedRole);
                    await context.User.SendMessageAsync($"{userTeamIcon}Signup confirmed. " +
                        $"{context.Message.Author.Mention} added to database with RSN \"{RSNInput}\" for team {userTeam}.{userTeamIcon}");
                    await context.User.SendMessageAsync($"{userTeamIcon}If this is not correct, please let an admin know so we can fix the issue.{userTeamIcon}");
                    await context.User.SendMessageAsync(grapeJob[rand.Next() % grapeJob.Count]);
                }
                // ADDROLE NOT WORKING - REPLACE LATER
                // Refresh user roles
                //_client.PurgeUserCache();
            }
            return;
        }//🍇🍌🍎🍑💩

        private async Task OnCommandExecuted(Optional<Discord.Commands.CommandInfo> command, ICommandContext context, IResult result)
        {
            // Error catcher
            if (command.IsSpecified && !result.IsSuccess)
            {
                switch (result.Error)
                {
                    case CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync($"Error type: {CommandError.BadArgCount}");
                        break;
                    case CommandError.Exception:
                        await context.Channel.SendMessageAsync($"Error type: {CommandError.Exception}");
                        if (result.ErrorReason.Contains("was not present in the dictionary."))
                            await context.Channel.SendMessageAsync($"It's possible that this could be due to the person specified not being signed up for Fruit Wars.");
                        break;
                    case CommandError.ObjectNotFound:
                        await context.Channel.SendMessageAsync($"Error type: {CommandError.ObjectNotFound}");
                        break;
                    case CommandError.ParseFailed:
                        await context.Channel.SendMessageAsync($"Error type: {CommandError.ParseFailed}");
                        break;
                    case CommandError.UnknownCommand:
                        await context.Channel.SendMessageAsync($"Error type: {CommandError.UnknownCommand}");
                        break;
                    case CommandError.UnmetPrecondition:
                        await context.Channel.SendMessageAsync($"Error type: {CommandError.UnmetPrecondition}");
                        break;
                    case CommandError.Unsuccessful:
                        await context.Channel.SendMessageAsync($"Error type: {CommandError.Unsuccessful}");
                        break;
                    case CommandError.MultipleMatches:
                        await context.Channel.SendMessageAsync($"Error type: {CommandError.MultipleMatches}");
                        await context.Channel.SendMessageAsync($"Multiple crash causes detected. This is bad, you should probably look into it.");
                        break;
                }
                await context.Channel.SendMessageAsync($"Error: {result}");
                await context.Channel.SendMessageAsync($"I just caught an error that would have normally crashed me! Now call me a good bot :D");
            }

        }
    }
}