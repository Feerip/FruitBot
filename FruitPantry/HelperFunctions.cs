﻿using DataTypes;

using Discord;
using Discord.WebSocket;

using RS3APIDropLog;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace FruitPantry
{
    public static class HelperFunctions
    {
        private static readonly FruitPantry _thePantry = FruitPantry.GetFruitPantry();

        //public static async Task LastHelper(int numDrops, DiscordSocketClient discordClient)
        //{
        //    FruitPantry thePantry = FruitPantry.GetFruitPantry();


        //    int idx = 0;

        //    foreach (KeyValuePair<string, DropLogEntry> entryPair in thePantry.GetDropLog())
        //    {
        //        if (idx == numDrops)
        //        {
        //            break;
        //        }
        //        await DropAnnouncementAsync(entryPair, discordClient);


        //        idx++;
        //    }
        //}
        public static async Task DropAnnouncementAsync(KeyValuePair<string, DropLogEntry> entryPair, DiscordSocketClient discordClient)
        {
#if DEBUGa
            ulong guild = 1088977050750173207;
            ulong dropsChannel = 1088984348549713961;
            ulong generalChannel = 1088977051945545770;
#else
            ulong guild = 769476224363397140;
            ulong dropsChannel = 862385904719364096;
            ulong generalChannel = 769476224363397144;
#endif
            FruitPantry thePantry = FruitPantry.GetFruitPantry();
            DropLogEntry entry = entryPair.Value;

            string fruit = entry._fruit;
            string emoji;
            Color color;
            string thumbnail;

            //quick and dirty fix, remove later
            string dropIconURL;
            if (entry._dropIconWEBP == null)
            {
                dropIconURL = "";
            }
            else if (entry._dropIconWEBP.Equals("https://runepixels.com/assets/images/runescape/activities/drop.webp"))
            {
                dropIconURL = "";
            }
            else
            {
                dropIconURL = entry._dropIconWEBP;
            }

            emoji = FruitResources.Emojis.Get(fruit);
            color = FruitResources.Colors.Get(fruit);
            thumbnail = FruitResources.Logos.Get(fruit);
            DateTime parsedTime;

            // Runemetrics times are in Europe/London time zone
            var englandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
            DateTime.TryParseExact(s: entry._timestamp, format: "MM'-'dd'-'yyyy HH:mm", style: System.Globalization.DateTimeStyles.None, result: out parsedTime, provider: null);
            var utcTimeStamp = TimeZoneInfo.ConvertTimeToUtc(parsedTime, englandTimeZone);

            EmbedBuilder builder;

            if (entry._bossName.Equals("Unknowns", StringComparison.OrdinalIgnoreCase))
            {
                builder = new EmbedBuilder()
                    .WithThumbnailUrl(thePantry._itemDatabase["unknown"]._imageURL)
                    .WithTitle(entry._dropName ?? "null")
                    .WithColor(thePantry._classificationColorList[entry._bossName])
                    ;

            }
            else
            {

                builder = new EmbedBuilder()
                    .WithThumbnailUrl(thePantry._itemDatabase[entry._dropName.ToLower()]._imageURL)
                    .WithTitle(entry._dropName ?? "null")
                    .WithColor(thePantry._classificationColorList[entry._bossName])                
                    ;

            }
            EmbedAuthorBuilder authorBuilder = new();
            authorBuilder.WithName(entry._playerName);
            string urlPlayerName = entry._playerName.Replace(' ', '+');
            authorBuilder.WithUrl($"https://apps.runescape.com/runemetrics/app/overview/player/{urlPlayerName}");
            authorBuilder.WithIconUrl($"https://secure.runescape.com/m=avatar-rs/{urlPlayerName}/chat.png");

            builder.WithAuthor(authorBuilder);
            builder.Url = thePantry._itemDatabase[entry._dropName.ToLower()]._wikiLink;
            builder.Timestamp = utcTimeStamp;
#if FRUITWARSMODE
            builder.AddField("Points", entry._pointValue, true);
#endif
            builder.AddField("Boss", entry._bossName, true);
            //builder.AddField("\u200B", "\u200B", true);
            
            // If user not signed up for fruit wars, that's fine, just don't add the field
            try
            {
                string discordMention = $"<@{thePantry._runescapePlayers[entry._playerName.ToLower()][1]}>";
                builder.AddField("Discord", discordMention, true);
            }
            catch (Exception e)
            {

            }
#if FRUITWARSMODE
            builder.AddField("Team", entry.GetFruitMention(discordClient.GetGuild(guild)) ?? "Fruitless Heathens", true);
#endif

            Embed embed = builder.Build();
            string message = null;
#if FRUITWARSMODE
            try
            {
                message = $"`{entry._pointValue}` points awarded to <@{thePantry._runescapePlayers[entry._playerName.ToLower()][1]}>!";
            }
            catch (Exception e)
            {
                // If the Discord ID is not in the database, then nothing to worry about, just leave the message as null
                if (!e.Message.Contains("was not present in the dictionary"))
                    throw new Exception(e.Message);
            }
#endif

            if (entry._bossName.Equals("Unknowns", StringComparison.OrdinalIgnoreCase))
            {
                message = "<@&856709182514397194> halp <a:MEOW:881462772636995595> I found an unfamiliar item WHAT DO I DOOOOO";
            }

            await discordClient.GetGuild(guild).GetTextChannel(dropsChannel).SendMessageAsync(message, false, embed);


            if (entry._bossName.Equals("InsaneRNG", StringComparison.OrdinalIgnoreCase))
            {
                await discordClient.GetGuild(guild).GetTextChannel(generalChannel).SendMessageAsync(null, false, embed);
            }
        }

        public static async Task<List<string>> BuildLastDropList(int numDrops, string playerName = null, string fruit = null)
        {
            // List of messages due to max length of 2000 chars
            List<string> messages = new();
            List<string> lines = new();
            // Code box entry
            string message = "";

            // Headers
            int idx = 0;
            int linesAdded = 0;

            var dropLogReversed = _thePantry.GetDropLog();
            dropLogReversed.Reverse();

            foreach (DropLogEntry entry in dropLogReversed)
            {
                bool addLine = true;
                if (idx % 22 == 0)
                {
                    message = "```\n";
                    message += "-------------------------------------------------------------------------" + "\n";
                    message += $"| {FruitResources.Emojis.fruitlessHeathen}Name         | Drop            | Boss   |  Pts   |    Timestamp     |" + "\n";
                }
                DropLogEntry newEntry = entry;
                if ((playerName != null))
                {
                    if (!newEntry._playerName.ToLower().Equals(playerName.ToLower()))
                    {
                        addLine = false;
                    }
                }
                if (fruit != null)
                {
                    if (!newEntry._fruit.ToLower().Equals(fruit.ToLower()))
                    {
                        addLine = false;
                    }
                }

                if (addLine)
                {
                    lines.Add(string.Format(
                        "| {0,-14} | {1,-15} | {2,-6} | {3,6} | {4,16} |",
                        FruitResources.Emojis.Get(newEntry._fruit) + newEntry._playerName,
                        newEntry._dropName.Substring(0, Math.Min(newEntry._dropName.Length, 15)),
                        newEntry._bossName.Substring(0, Math.Min(newEntry._bossName.Length, 6)),
                        float.Parse(newEntry._pointValue).ToString("0.00"),
                        newEntry._timestamp
                        ) + "\n");

                    linesAdded++;
                    idx++;
                }

                if (lines.Count == 22)
                {
                    lines.Reverse();
                    foreach (string line in lines)
                    {
                        message += line;
                    }

                    message += "-------------------------------------------------------------------------```";
                    messages.Add(message);
                    lines = new();
                    message = "";
                }

                if ((linesAdded == numDrops) || (entry == _thePantry.GetDropLog().Last()))
                {
                    if (!message.Equals(""))
                    {
                        lines.Reverse();
                        foreach (string line in lines)
                        {
                            message += line;
                        }
                        message += "-------------------------------------------------------------------------```";
                        messages.Add(message);
                    }
                    break;
                }
            }

            messages.Reverse();
            return messages;
        }

        private class MyComparer : IComparer<KeyValuePair<string, float>>
        {
            public int Compare(KeyValuePair<string, float> x, KeyValuePair<string, float> y)
            {
                int result = y.Value.CompareTo(x.Value);

                if (result == 0)
                {
                    result = y.Key.CompareTo(x.Key);
                }

                return result;
            }
        }

        public static async Task<List<string>> BuildPointsList(SortedDictionary<string, float> teamMembers, string fruit)
        {
            List<string> messages = new();
            List<string> lines = new();
            List<KeyValuePair<string, float>> sortedByPoints = teamMembers.ToList();

            IEnumerable<KeyValuePair<string, float>> query = sortedByPoints.OrderByDescending(x => x.Value);


            string message = "";

            int idx = 0;
            int linesAdded = 0;
            float totalPointsInList = 0;

            foreach (KeyValuePair<string, float> player in query)
            {
                totalPointsInList += player.Value;
                string playerName = player.Key;
                float playerPoints = player.Value;

                if (idx % 60 == 0)
                {
                    message += "```\n";
                    message += $"----------------------------" + "\n";
                    message += $"| {FruitResources.Emojis.fruitlessHeathen}Name         |   Pts   |" + "\n";
                }

                lines.Add(string.Format("| {0,-14} | {1,7} |", FruitResources.Emojis.Get(_thePantry._runescapePlayers[playerName][0]) + playerName, playerPoints.ToString("0.00")) + "\n");

                linesAdded++;
                idx++;

                if (lines.Count == 60)
                {
                    foreach (string line in lines)
                    {
                        message += line;
                    }

                    message += "----------------------------```";
                    messages.Add(message);
                    lines = new();
                    message = "";
                }

                if (player.Key.Equals(query.Last().Key, StringComparison.OrdinalIgnoreCase))
                {
                    if (!message.Equals(""))
                    {
                        foreach (string line in lines)
                        {
                            message += line;
                        }
                        message += "----------------------------\n";
                        message += string.Format("| {0,-14} | {1,7} |", FruitResources.Emojis.Get(fruit) + "Total: ", totalPointsInList.ToString("0.00")) + "\n";
                        message += "----------------------------```";
                        messages.Add(message);
                    }
                    break;
                }
            }
            return messages;
        }
    }
}
