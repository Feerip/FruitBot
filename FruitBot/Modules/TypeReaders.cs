using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTypes;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace FruitBot.Modules
{
    public class TypeReaders
    {
        public class LastCommandArguments
        {
            public int NumDrops = 0;
            public string RSN = null;
            public SocketGuildUser DiscordUser = null;
            public string Fruit = null;


            public bool NumDropsFound = false;
            public bool RSNFound = false;
            public bool DiscordUserFound = false;
            public bool FruitFound = false;
        }

        public class LastCommandTypeReader : TypeReader
        {
            public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
            {
                LastCommandArguments args = new();

                ulong userDiscordID;
                bool noMoreArguments = false;

                bool foundFilter = false;

                string[] tokens = input.Split(' ');

                
                for (int idx = 0; (!noMoreArguments) && (idx < tokens.Length); idx++)
                {
                    // If the argument is a straight up number, that means it's the number of drops the user wants to see.
                    // Priority #1.
                    if ((args.NumDropsFound == false) && int.TryParse(tokens[idx], out args.NumDrops))
                    {
                        args.NumDropsFound = true;
                        // Check to make sure that the number of drops requested valid, otherwise throw some sass at the user
                        if (args.NumDrops < 1)
                            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid command: number of drops must be greater than zero. " +
                                "Exactly what are you trying to accomplish here?"));
                    }
                    // If the argument can be parsed to a ulong after removing the standard Discord tag header and footer, that means it's 
                    // a Discord mention and should be converted to a SocketGuildUser. 
                    // Priority #2.
                    else if ((args.DiscordUserFound == false) && ulong.TryParse(tokens[idx].Replace("<@!", "").Replace("<@", "").Replace(">", ""), out userDiscordID))
                    {

                        args.DiscordUserFound = true;
                        foundFilter = true;
                        args.DiscordUser = (SocketGuildUser)context.Guild.GetUserAsync(userDiscordID).Result;
                        // Check to make sure this is the last argument. If not, it's an invalid command.
                        if (idx != tokens.Length - 1)
                            return Task.FromResult(TypeReaderResult.FromError(CommandError.BadArgCount, "Invalid command: too many arguments. " +
                                "Discord @tag must be the last argument in the command."));
                    }
                    // If the full argument can be parsed into a fruit, well then obviously it's a fruit.
                    // Priority #3
                    else if ((args.FruitFound == false) && FruitResources.Text.TryParse(tokens[idx], out args.Fruit))
                    {

                        args.FruitFound = true;
                        foundFilter = true;
                        // Check to make sure this is the last argument. If not, it's an invalid command.
                        if (idx != tokens.Length - 1)
                            return Task.FromResult(TypeReaderResult.FromError(CommandError.BadArgCount, "Invalid command: too many arguments. " +
                                "Fruit type must be the last argument in the command."));
                    }
                    // If all else fails, then it's either an RSN or garbage, and should be treated as such.
                    // Lowest priority
                    else
                    {
                        // Check to make sure this is the only filter type specified
                        if (foundFilter)
                            return Task.FromResult(TypeReaderResult.FromError(CommandError.BadArgCount, "Invalid command: too many arguments. " +
                                "You may only use ONE \"filtering\" argument on the command (@tag, FruitName, or RSN)."));

                        // Stop looping even if there are more arguments, because RSNs can have spaces in them
                        // and at this point the rest of the argument should be treated as an RSN, with the calling
                        // function taking care of whether the RSN is valid or if the command arguments need to be 
                        // rejected.
                        foundFilter = true;
                        noMoreArguments = true;
                        args.RSN = "";
                        for (; idx < tokens.Length; idx++)
                        {
                            args.RSN += tokens[idx];
                            if (idx != tokens.Length - 1)
                                args.RSN += " ";
                        }
                        args.RSNFound = true;
                        // Check to make sure the RSN is shorter than the 12 character limit imposed by Jagex, to save everyone some time.
                        if (args.RSN.Length > 12)
                        {
                            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid command: garbage detected. " +
                                "I detected that you're trying to specify an RSN, but the length is too long to be one. (>12 characters)"));
                        }
                    }
                }
               
                return Task.FromResult(TypeReaderResult.FromSuccess(args));
            }
        }
    }
}
