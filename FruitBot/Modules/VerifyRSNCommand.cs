using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FruitPantry;

namespace FruitBot.Modules
{
    public class VerifyRSNCommand : InteractionModuleBase
    {
        private readonly bool _ephemeral = true;

        [SlashCommand("verify-rsn", "register and verify your in-game name")]
        public async Task VerifyRSN(string RSN)
        {
            // Needs to be deferred because if the token needs to be refreshed it could take up to 30 seconds to get a response
            await Context.Interaction.DeferAsync(ephemeral: _ephemeral);

            // Try/catch makes handling of all bad-input notifications easier 
            try
            {
                // Pull a random list of worlds for the player to hop for verification, based on the players RS membership status
                int[] worlds = RunescapePlayerInfoPuller.RunescapePlayerInfoPuller.Instance.GenerateHopWorldList(RSN);
                // Pulls the initial player data to make sure the bot can see the player as entered by the user. 
                RunescapePlayerInfoPuller.RSPlayerQueryResult playerData = RunescapePlayerInfoPuller.RunescapePlayerInfoPuller.Instance.Query(RSN);
                // If the data shows that world is null, that means barring any freakishly niche bugs, the player data is not visible to the bot.
                if (playerData.world == null)
                {
                    throw new Exception("Verification failed. Please make sure that:\n" +
                        $"> 1. Your Runescape name `{RSN}` is spelled correctly, with exact punctuation.\n" +
                        $"> 2. You are currently logged in (lobby is okay too).\n" +
                        $"> 3. Your online status in-game is **NOT** set to `Off` or `Friends Only` (hop worlds after you change it to refresh).\n" +
                        $"If all of the above are true, then it's probably a bug and you should cry for help.\n" +
                        $"P.S. you can change your online status back after this is finished, I only need to see your status for one-time verification and will save your RSN for future reference.");
                }

                // Takes the randomly generated worlds and player info, and creates the actual verification interaction.
                var embedBuilder = new EmbedBuilder()
                    .WithTitle($"RSN Verification - {RSN}")
                    .WithDescription($"Hello {Context.User.Mention}, you are trying to register the Runescape username `{RSN}`. " +
                    $"Please hop to the following worlds to verify that you own the account.")
                    .AddField($"World {worlds[0]}", "⚠️ Pending", true)
                    .AddField($"World {worlds[1]}", "⚠️ Pending", true)
                    .AddField($"World {worlds[2]}", "⚠️ Pending", true)
                    // Red to show that it's not done yet 
                    .WithColor(new(255, 0, 0))
                    ;

                // Creates the button
                var componentBuilder = new ComponentBuilder()
                    .WithButton($"I have hopped to World {worlds[0]}", $"confirm-hop:0:{worlds[0]}:{worlds[1]}:{worlds[2]}");

                // Sends the interaction
                await FollowupAsync(embed: embedBuilder.Build(), component: componentBuilder.Build(), ephemeral: true);
            }
            catch (Exception e)
            {
                // If user input was incorrect or there was a bug, that info is sent as a message here instead.
                await FollowupAsync(text: e.Message, ephemeral: _ephemeral);
            }
        }

        // Handles the button click for the verify-rsn interaction.
        [ComponentInteraction("confirm-hop:*:*:*:*")]
        public async Task ConfirmHop(string hop, string world1, string world2, string world3)
        {
            // This can take up to 30 seconds if the token needs to be refreshed, so defer is needed.
            await Context.Interaction.DeferAsync(ephemeral: _ephemeral);

            // Parses the input data strings into ints that we can work with
            int hopNumber = int.Parse(hop);
            int[] worlds = new int[] { int.Parse(world1), int.Parse(world2), int.Parse(world3) };

            // Pulls the interaction message to use as reference when changing things
            var originalMsg = (Context.Interaction as SocketMessageComponent).Message;

            // Pulls the player name from the original embed, easiest way to get that data 
            string originalRSN = originalMsg.Embeds.First().Title.Replace("RSN Verification - ", "");

            // Disables the button while we're processing
            await (Context.Interaction as SocketMessageComponent).ModifyOriginalResponseAsync(msg =>
            {
                // Rebuilds the button completely from scratch based on available info, the only difference being the disabled flag set to true
                var componentBuilder = new ComponentBuilder()
                    .WithButton($"I have hopped to World {worlds[hopNumber]}", $"confirm-hop:{hop}:{worlds[0]}:{worlds[1]}:{worlds[2]}", disabled: true);
                    
                msg.Components = componentBuilder.Build();
            });

            // Again using exceptions to simplify displaying the user input errors as well as bugs
            try
            {
                // The user says they hopped, so pull new data to confirm
                int? currentPlayerWorld = RunescapePlayerInfoPuller.RunescapePlayerInfoPuller.Instance.QueryWorldOnly(originalRSN);
                // Checks to see if the player performed the requested hop
                if ((currentPlayerWorld == worlds[hopNumber]))
                {
                    // If so, modify the original message 
                    await (Context.Interaction as SocketMessageComponent).ModifyOriginalResponseAsync(msg =>
                    {
                        // If this is the first or second hop, request the next hop in the sequence
                        if (hopNumber < 2)
                        {
                            // Show the user that we've confirmed the hop status
                            var embedBuilder = originalMsg.Embeds.First().ToEmbedBuilder();
                            embedBuilder.Fields[hopNumber].Value = "✅ Confirmed";

                            // Update the button to reflect the next world they should hop to
                            var componentBuilder = new ComponentBuilder()
                            .WithButton($"I have hopped to World {worlds[hopNumber + 1]}", $"confirm-hop:{hopNumber + 1}:{world1}:{world2}:{world3}");

                            // Send it
                            msg.Embed = embedBuilder.Build();
                            msg.Components = componentBuilder.Build();
                        }
                        // If this is the third hop, time to clean things up
                        else
                        {
                            // Wipe the embed and create a new one that shows confirmation of RSN verification
                            var embedBuilder = originalMsg.Embeds.First().ToEmbedBuilder();
                            embedBuilder.Description = $"RSN Verified for {Context.User.Mention}!";
                            embedBuilder.Fields.Clear();
                            // Green to show that it's done
                            embedBuilder.WithColor(new(0, 255, 0))
                            ;

                            // Send it
                            msg.Embed = embedBuilder.Build();
                            msg.Components = new ComponentBuilder().Build();

                            // Register them in the db
                            FruitPantry.FruitPantry.GetFruitPantry().RegisterPlayer(originalRSN, discordTag: Context.User.Username + "#" + Context.User.Discriminator, discordId: Context.User.Id);

                        }
                    });
                }
                else
                {
                    // If not, tell them
                    throw new Exception("Hop not detected. Please try again.");
                }
            }
            catch (Exception e)
            {
                // Again, if user input was incorrect or there was a bug, that info is sent as a message here instead.
                await FollowupAsync(text: e.Message, ephemeral: _ephemeral);

                // Re-enables the button now that we're done.
                await (Context.Interaction as SocketMessageComponent).ModifyOriginalResponseAsync(msg =>
                {
                    // Button was disabled when we started processing, this re-enables it using the same method but in reverse.
                    var componentBuilder = new ComponentBuilder()
                        .WithButton($"I have hopped to World {worlds[hopNumber]}", $"confirm-hop:{hop}:{worlds[0]}:{worlds[1]}:{worlds[2]}", disabled: false);

                    msg.Components = componentBuilder.Build();
                });
            }
        }
    }
}
