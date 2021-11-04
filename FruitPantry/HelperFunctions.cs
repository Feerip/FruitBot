using DataTypes;
using Discord;
using Discord.WebSocket;
using RS3APIDropLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FruitPantry
{
    public static class HelperFunctions
    {
        private static readonly FruitPantry _thePantry = FruitPantry.GetFruitPantry();

        public static IEnumerable<DropLogEntry> GetLastNDrops(int numDrops, string? rsn = null, string? fruit = null)
        {
            return _thePantry.GetDropLog()
                .Where(p => rsn != null ? p.Value._playerName.Equals(rsn, StringComparison.OrdinalIgnoreCase) : true)
                .Where(p => fruit != null ? p.Value._fruit.Equals(fruit, StringComparison.OrdinalIgnoreCase) : true)
                .Take(numDrops)
                .Select(p => p.Value);
        }

        public static Embed BuildDropEmbed(DropLogEntry entry)
        {
            string fruit = entry._fruit;
            string emoji = FruitResources.Emojis.Get(fruit);
            Color color = FruitResources.Colors.Get(fruit);
            string thumbnail = FruitResources.Logos.Get(fruit);

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

            var imageUrl = entry._bossName.Equals("Unknowns", StringComparison.OrdinalIgnoreCase)
                ? _thePantry._itemDatabase["unknown"]._imageURL
                : _thePantry._itemDatabase[entry._dropName.ToLower()]._imageURL;

            var builder = new EmbedBuilder()
                .WithThumbnailUrl(imageUrl)
                .WithTitle(entry._dropName ?? "null")
                .WithColor(_thePantry._classificationColorList[entry._bossName])
                .AddField("Player Name", entry._playerName ?? "null", true);
#if FRUITWARSMODE
            builder.AddField("Points", entry._pointValue, true);
#endif
            builder.AddField("Boss", entry._bossName, true);
            builder.AddField("Dropped At", entry._timestamp, true);

            return builder.Build();
        }

        public static async Task LastHelper(int numDrops, DiscordSocketClient discordClient)
        {
            var drops = GetLastNDrops(numDrops);

            foreach (DropLogEntry entry in drops)
            {
                var embed = BuildDropEmbed(entry);

                string message = null;
                ulong channel = 862385904719364096;

                // send to bitching channel if it's unknown
                if (entry._bossName.Equals("Unknowns", StringComparison.OrdinalIgnoreCase))
                {
                    message = "<@&856709182514397194> halp <a:MEOW:881462772636995595> I found an unfamiliar item WHAT DO I DOOOOO";
                    channel = 856679881547186196;
                }

                await discordClient.GetGuild(769476224363397140).GetTextChannel(channel).SendMessageAsync(message, false, embed);

                // if someone gets hsr broadcast in general as well
                if (entry._bossName.Equals("InsaneRNG", StringComparison.OrdinalIgnoreCase))
                {
                    await discordClient.GetGuild(769476224363397140).GetTextChannel(769476224363397144).SendMessageAsync(null, false, embed);
                }
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
            foreach (KeyValuePair<string, DropLogEntry> entryPair in _thePantry.GetDropLog())
            {
                bool addLine = true;
                if (idx % 22 == 0)
                {
                    message = "```\n";
                    message += "-------------------------------------------------------------------------" + "\n";
                    message += $"| {FruitResources.Emojis.fruitlessHeathen}Name         | Drop            | Boss   |  Pts   |    Timestamp     |" + "\n";
                }
                DropLogEntry entry = entryPair.Value;
                if ((playerName != null))
                {
                    if (!entry._playerName.ToLower().Equals(playerName.ToLower()))
                    {
                        addLine = false;
                    }
                }
                if (fruit != null)
                {
                    if (!entry._fruit.ToLower().Equals(fruit.ToLower()))
                    {
                        addLine = false;
                    }
                }

                if (addLine)
                {
                    lines.Add(string.Format(
                        "| {0,-14} | {1,-15} | {2,-6} | {3,6} | {4,16} |",
                        FruitResources.Emojis.Get(entry._fruit) + entry._playerName,
                        entry._dropName.Substring(0, Math.Min(entry._dropName.Length, 15)),
                        entry._bossName.Substring(0, Math.Min(entry._bossName.Length, 6)),
                        float.Parse(entry._pointValue).ToString("0.00"),
                        entry._timestamp
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

                if ((linesAdded == numDrops) || (entryPair.Key == _thePantry.GetDropLog().Last().Key))
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

        public static async Task<List<string>> BuildPointsList(SortedDictionary<string, float> teamMembers)
        {
            List<string> messages = new();
            List<string> lines = new();
            List<KeyValuePair<string, float>> sortedByPoints = teamMembers.ToList();

            IEnumerable<KeyValuePair<string, float>> query = sortedByPoints.OrderByDescending(x => x.Value);


            string message = "";

            int idx = 0;
            int linesAdded = 0;


            foreach (KeyValuePair<string, float> player in query)
            {
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
