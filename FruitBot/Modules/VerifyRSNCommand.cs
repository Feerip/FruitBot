using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FruitBot.Modules
{
    public class VerifyRSNCommand : InteractionModuleBase
    {
        [SlashCommand("verify-rsn", "register and verify your in-game name")]
        public async Task VerifyRSN(string RSN)
        {
            await Context.Interaction.DeferAsync(ephemeral: true);

            try
            {
                int[] worlds = RunescapePlayerInfoPuller.RunescapePlayerInfoPuller.Instance.GenerateHopWorldList(RSN);
                RunescapePlayerInfoPuller.RSPlayerQueryResult playerData = RunescapePlayerInfoPuller.RunescapePlayerInfoPuller.Instance.Query(RSN);
                if (playerData.world == null)
                {
                    throw new Exception("Verification failed. Please make sure that:\n" +
                        $"> 1. Your Runescape name `{RSN}` is spelled correctly, with exact punctuation.\n" +
                        $"> 2. You are currently logged in (lobby is okay too).\n" +
                        $"> 3. Your online status in-game is **NOT** set to `Off` or `Friends Only` (hop worlds after you change it to refresh).\n" +
                        $"If all of the above are true, then it's probably a bug and you should cry for help.");
                }

                var embedBuilder = new EmbedBuilder()
                    .WithTitle($"RSN Verification - {RSN}")
                    .WithDescription($"Hello {Context.User.Mention}, you are trying to register the Runescape username `{RSN}`. " +
                    $"Please hop to the following worlds to verify that you own the account.")
                    .AddField($"World {worlds[0]}", "⚠️ Pending", true)
                    .AddField($"World {worlds[1]}", "⚠️ Pending", true)
                    .AddField($"World {worlds[2]}", "⚠️ Pending", true)
                    .WithColor(new(255, 0, 0))
                    ;

                var componentBuilder = new ComponentBuilder()
                    .WithButton($"I have hopped to World {worlds[0]}", $"confirm-hop:0:{worlds[0]}:{worlds[1]}:{worlds[2]}")
                    ;

                await FollowupAsync(embed: embedBuilder.Build(), component: componentBuilder.Build(), ephemeral: true);
            }
            catch (Exception e)
            {
                await FollowupAsync(text: e.Message);
            }
        }

        [ComponentInteraction("confirm-hop:*:*:*:*")]
        public async Task ConfirmHop1(string hop, string world1, string world2, string world3)
        {
            //await Context.Interaction.DeferAsync();

            int hopNumber = int.Parse(hop);
            int[] worlds = new int[] { int.Parse(world1), int.Parse(world2), int.Parse(world3) };

            var originalMsg = (Context.Interaction as SocketMessageComponent).Message;

            string originalRSN = originalMsg.Embeds.First().Title.Replace("RSN Verification - ", "");

            try
            {
                int? currentPlayerWorld = RunescapePlayerInfoPuller.RunescapePlayerInfoPuller.Instance.QueryWorldOnly(originalRSN);
                if ((currentPlayerWorld != worlds[hopNumber]))
                {
                    throw new Exception("Hop not detected. Please try again.");
                }

                await (Context.Interaction as SocketMessageComponent).UpdateAsync(msg =>
                {
                    if (hopNumber < 2)
                    {
                        var embedBuilder = originalMsg.Embeds.First().ToEmbedBuilder();
                        embedBuilder.Fields[hopNumber].Value = "✅ Confirmed";
                        msg.Embed = embedBuilder.Build();

                        var componentBuilder = new ComponentBuilder()
                        .WithButton($"I have hopped to World {worlds[hopNumber + 1]}", $"confirm-hop:{hopNumber + 1}:{world1}:{world2}:{world3}");

                        msg.Components = componentBuilder.Build();
                    }
                    else
                    {
                        var embedBuilder = originalMsg.Embeds.First().ToEmbedBuilder();
                        embedBuilder.Description = $"RSN Verified for {Context.User.Mention}!";
                        embedBuilder.Fields.Clear();
                        embedBuilder.WithColor(new(0, 255, 0))
                        ;

                        msg.Embed = embedBuilder.Build();

                        msg.Components = new ComponentBuilder().Build();
                    }
                });
            }
            catch (Exception e)
            {
                await RespondAsync(text: e.Message, ephemeral: true);
            }
        }
    }
}
