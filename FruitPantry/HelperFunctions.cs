using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using RSAdventurerLogScraper;
using DataTypes;
using static DataTypes.FruitResources;

namespace FruitPantry
{
    public static class HelperFunctions
    {
        private static FruitPantry _thePantry = FruitPantry.GetFruitPantry();

        public static async Task LastHelper(int numDrops, DiscordSocketClient discordClient)
        {

            FruitPantry thePantry = FruitPantry.GetFruitPantry();


            //DropLogEntry lastEntry = thePantry._masterList.Last().Value;

            int idx = 0;

            foreach (KeyValuePair<string, DropLogEntry> entryPair in thePantry.GetDropLog())
            {
                if (idx == numDrops)
                    break;
                DropLogEntry entry = entryPair.Value;

                string fruit = entry._fruit;
                string emoji;
                Color color;
                string thumbnail;

                //quick and dirty fix, remove later
                string dropIconURL;
                if (entry._dropIconWEBP == null)
                    dropIconURL = "";
                else if (entry._dropIconWEBP.Equals("https://runepixels.com/assets/images/runescape/activities/drop.webp"))
                    dropIconURL = "";
                else
                    dropIconURL = entry._dropIconWEBP;

                emoji = FruitResources.Emojis.Get(fruit);
                color = FruitResources.Colors.Get(fruit);
                thumbnail = FruitResources.Logos.Get(fruit);

                var builder = new EmbedBuilder()
                    .WithThumbnailUrl(thePantry._itemDatabase[entry._dropName.ToLower()]._imageURL)
                    .WithTitle(entry._dropName ?? "null")
                    .WithColor(thePantry._classificationColorList[entry._dropName.ToLower()])
                    .AddField("Player Name", entry._playerName ?? "null", true)
                    //.AddField("Drop", entry._dropName ?? "null", true)
#if FRUITWARSMODE
                    .AddField("Points", entry._pointValue, true)
#endif
                    .AddField("Boss", entry._bossName, true)
                    .AddField("Dropped At", entry._timestamp, true)
                    //.AddField("Fruit", entry._fruit == "" ? "null" : entry._fruit, true)
                    //.AddField("Drop Timestamp", entry._timestamp ?? "null", true)
                    //.AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                    //.WithCurrentTimestamp()
                    ;

                var embed = builder.Build();

                await discordClient.GetGuild(769476224363397140).GetTextChannel(862385904719364096).SendMessageAsync(null, false, embed);
                idx++;
            }

        }

        //public static async Task<EmbedBuilder> BuildEmbedFrom

        public static async Task<List<string>> BuildLastDropList(int numDrops, string playerName = null, string fruit = null)
        {
            // List of messages due to max length of 2000 chars
            List<string> messages = new();
            List<string> lines = new();
            // Code box entry
            string message = "";

            // Headers


            //output += "|------------------------------------------------------------------------------|" + "\n";

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
                        //continue;
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
                //if (idx % 22 == 0)
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

                //if (idx == numDrops)
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

        class MyComparer : IComparer<KeyValuePair<string, float>>
        {
            public int Compare(KeyValuePair<string, float> x, KeyValuePair<string, float> y)
            {
                int result = y.Value.CompareTo(x.Value);

                if (result == 0)
                    result = y.Key.CompareTo(x.Key);

                return result;
            }
        }

        public static async Task<List<string>> BuildPointsList(SortedDictionary<string, float> teamMembers)
        {
            List<string> messages = new();
            List<string> lines = new();
            List<KeyValuePair<string, float>> sortedByPoints = teamMembers.ToList();


            //foreach (KeyValuePair<string, float> teamMember in teamMembers)
            //    sortedByPoints.Add(new(teamMember.Key, teamMember.Value));

            //IOrderedEnumerable<KeyValuePair<string, float>> query = teamMembers.OrderByDescending(x => x.Value);

            //sortedByPoints.Sort((x, y) => 

            //(y.Value.CompareTo(x.Value)));

            //sortedByPoints.Sort(new MyComparer());

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
                    //lines.Reverse();
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
                        //lines.Reverse();
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

            //messages.Reverse();
            return messages;

        }
    }
}
