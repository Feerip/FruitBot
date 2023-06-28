
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;

using FruitBot.Modules;

using FruitPantry;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using RS3APIDropLog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace FruitBot.Services
{
    public class CommandHandler : DiscordClientService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _config;
        private readonly FruitPantry.FruitPantry _thePantry = FruitPantry.FruitPantry.GetFruitPantry();

        public CommandHandler(DiscordSocketClient client, ILogger<CommandHandler> logger, IServiceProvider provider, CommandService service, IConfiguration config) : base(client, logger)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;


        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;
            _service.AddTypeReader(typeof(TypeReaders.LastCommandArguments), new TypeReaders.LastCommandTypeReader());

            _service.CommandExecuted += OnCommandExecuted;
            _client.Ready += SetStatusAsync;
            _client.Ready += _client_Ready;
            //_client.Ready += RegisterPosixSignalsAsync;


            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task SetStatusAsync()
        {
#if FRUITWARSMODE
            await _client.SetGameAsync($"Fruit Wars!! | @FruitBot help", null, ActivityType.Playing);
            
            await _client.SetStatusAsync(UserStatus.Online);
#else
            await _client.SetGameAsync($"@FruitBot help", null, ActivityType.Listening);
#endif
        }

        private Task RegisterPosixSignalsAsync()
        {
            Console.WriteLine("Entered Registration");
            PosixSignalRegistration.Create(PosixSignal.SIGTERM, async (context) =>
            {
                Console.WriteLine("Entered posix registration");
                context.Cancel = false;
                await _client.SetGameAsync($"Restarting... Be right back!", null, ActivityType.Playing);
                await _client.SetStatusAsync(UserStatus.DoNotDisturb);
                Console.WriteLine("Finished posix registration");
            });
            Console.WriteLine("Finished registration");
            return Task.CompletedTask;
        }

        private Task _client_Ready()
        {
            TimedHostedService service = new(_client);
            Task backgroundScraper = service.StartAsync(new CancellationToken());

            // Watchdog event to kill the entire program when the Discord.net API glitches out and disconnects. 
            // Calling powershell script will detect the exit and restart it indefinitely.
            // Issues: Calling Environment.Exit(1) to signal that the script should restart is not working properly. Program always exits with exit code 0.
            // Until that's resolved, the powershell script needs to always restart it, but once we get this working it can restart on exit code 1 and 
            //  stay closed on exit code 0.

            System.Timers.Timer timer = new System.Timers.Timer(10000);
            timer.Elapsed += new ElapsedEventHandler(CheckConnection);
            timer.Start();

#if FRUITWARSMODE

            List<int> intlist = new List<int> { 17, 23, 05, 11 };

            Parallel.ForEach(intlist, (i) => service.LeaderboardAtResetStartAsync(new CancellationToken(), i, 02));
#endif
            return Task.CompletedTask;

        }

        private void CheckConnection(object sender, ElapsedEventArgs e)
        {
            Environment.ExitCode = 1;
            if (_client.ConnectionState == ConnectionState.Disconnecting)
            {
                Environment.Exit(1);
            }
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg3.MessageId != 857061904944988160)
            {
                return;
            }

            if (arg3.Emote.Name != "✅")
            {
                return;
            }

            SocketRole role = (arg2 as SocketGuildChannel).Guild.Roles.FirstOrDefault(x => x.Id == 856709182514397194);
            await (arg3.User.Value as SocketGuildUser).AddRoleAsync(role);


        }

        private async Task OnJoinedGuild(SocketGuild arg)
        {
            await arg.DefaultChannel.SendMessageAsync("Bot joined the server");
        }

        private async Task OnChannelCreated(SocketChannel arg)
        {
            if ((arg as ITextChannel) == null)
            {
                return;
            }

            ITextChannel channel = arg as ITextChannel;

            await channel.SendMessageAsync("Channel created");
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message))
            {
                return;
            }

            SocketCommandContext context = new SocketCommandContext(_client, message);
            if (message.Source != MessageSource.User)
            {
                return;
            }

            int argPos = 0;



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

                await context.Channel.SendMessageAsync($"Upvotes: `{response.goodBot}`, Downvotes: `{response.badBot}`", messageReference: new(context.Message.Id));
                // This is on purpose, only send the upvote/downvote tally if the user is not being abusive to my poor baby
            }
            if (message.Content.ToLower().Equals("bad gob"))
            {
                FruitPantry.FruitPantry.VoteResponse response = _thePantry.QueryBadGob();
                await context.Channel.SendMessageAsync($"Upvotes: `{response.goodBot}`, Downvotes: `{response.badBot}`", messageReference: new(context.Message.Id));
            }

            if (!message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                return;
            }

            await _service.ExecuteAsync(context, argPos, _provider);


        }

        private async Task OnPrivateMessageReceived(SocketMessage arg, SocketCommandContext context, SocketUserMessage message, int argPos)
        {
            _thePantry.RefreshEverything();
            Random rand = new(DateTime.Now.Millisecond);
            List<string> kiwiJob = new();
            kiwiJob.Add("Also, 🥝Kiwi🥝 is the superior fruit.");
            kiwiJob.Add("Also, 🥝Kiwi🥝#1");
            kiwiJob.Add("Also, if 🥝Kiwis🥝 don't win it's rigged.");
            kiwiJob.Add("Also, 🥝Kiwis🥝 control the bot, just sayin.");
            kiwiJob.Add("Also, 🥝Kiwis🥝 shall inherit the earth.");


            if (!message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                return;
            }
            else
            {
                string[] buffer = context.Message.ToString().Split(' ');
                string RSNInput = "";

                for (int idx = 1; idx < buffer.Count(); idx++)
                {
                    RSNInput += buffer[idx];
                    if (idx < buffer.Count() - 1 && !buffer[idx].Equals(" "))
                    {
                        RSNInput += " ";
                    }
                }


                string userDiscordTag = context.Message.Author.Username + "#" + context.Message.Author.Discriminator;

                using (context.Channel.EnterTypingState())
                {
                    FruitPantry.FruitPantry thePantry = FruitPantry.FruitPantry.GetFruitPantry();
                    ulong userID = context.Message.Author.Id;
                    IReadOnlyCollection<SocketRole> userRoles = _client.GetGuild(769476224363397140).GetUser(userID).Roles;

                    SocketRole kiwiRole = _client.GetGuild(769476224363397140).Roles.First(x => x.Name == "Kiwis");
                    SocketRole appleRole = _client.GetGuild(769476224363397140).Roles.First(x => x.Name == "Apples");
                    SocketRole peachRole = _client.GetGuild(769476224363397140).Roles.First(x => x.Name == "Peaches");
                    SocketRole bananaRole = _client.GetGuild(769476224363397140).Roles.First(x => x.Name == "Bananas");
                    string userTeam;
                    string userTeamIcon;
                    if (userRoles.Contains(kiwiRole))
                    {
                        userTeam = "Kiwi";
                        userTeamIcon = "🥝";
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

                    if (thePantry._discordUsers.ContainsKey(userID.ToString()))
                    {
                        await context.User.SendMessageAsync($"You have already signed up for fruit wars and will not be added again.");
                        await context.User.SendMessageAsync($"If this is not correct, please let an admin know so we can fix the issue.");
                        return;
                    }

                    if (thePantry._runescapePlayers.ContainsKey(RSNInput.ToLower()))
                    {
                        await context.User.SendMessageAsync($"RSN `{RSNInput}` has already been claimed and registered to " +
                            $"{_client.GetGuild(769476224363397140).GetUser(ulong.Parse(thePantry._runescapePlayers[RSNInput.ToLower()][1])).Mention} " +
                            $"and will not be added again.");
                        await context.User.SendMessageAsync($"If this is not correct, please let an admin know so we can fix the issue.");
                        return;
                    }

                    List<string> allClanPlayerNames = await RSDropLog.GetAllVoughtPlayerNames();

                    if (allClanPlayerNames.Contains(RSNInput) || allClanPlayerNames.Contains(RSNInput.Replace(" ", " ")) || allClanPlayerNames.Contains(RSNInput.Replace(" ", "_")))
                    {

                        thePantry.RegisterPlayer(RSNInput, userTeam, userID.ToString());
                        await context.User.SendMessageAsync($"{userTeamIcon}Signup confirmed. " +
                            $"{context.Message.Author.Mention} added to database with RSN `{RSNInput}` for team {userTeam}.{userTeamIcon}");
                        await context.User.SendMessageAsync($"{userTeamIcon}If this is not correct, please let an admin know so we can fix the issue.{userTeamIcon}");
                        await context.User.SendMessageAsync(kiwiJob[rand.Next() % kiwiJob.Count]);
                    }
                    else
                    {
                        await context.User.SendMessageAsync($"{context.User.Mention}: RSN `{RSNInput}` not found in clan. Fruit Wars database unchanged.");
                        await context.User.SendMessageAsync("Please check to make sure you've spelled your RSN correctly (capitalization does matter). " +
                            "If this was a mistake, please ask your friendly neighborhood clan Admin or Bot Herder for help.");
                    }
                }

            }
            return;
        }//🥝🍌🍎🍑💩

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
                        {
                            await context.Channel.SendMessageAsync($"It's possible that this could be due to the person specified not being signed up for Fruit Wars.");
                        }

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