using Discord;
using Discord.WebSocket;

using System;
using System.Linq;

namespace DataTypes
{
    public static class FruitResources
    {
        public static class Mention
        {
            public static string Get(string fruit, SocketGuild guild)
            {
                if (fruit is null)
                    return null;
                else
                {
                    IRole role = guild.Roles.FirstOrDefault(x => x.Name == fruit);
                    if (role is not null)
                        return role.Mention;
                    else
                        return null;
                }
            }
        }
        public static class Colors
        {
            public static Discord.Color Get(string fruit)
            {
                if (fruit is null)
                {
                    return fruitlessHeathen;
                }
                if (fruit.Equals(Text.banana))
                {
                    return banana;
                }

                if (fruit.Equals(Text.kiwi))
                {
                    return kiwi;
                }

                if (fruit.Equals(Text.cherry))
                {
                    return cherry;
                }

                if (fruit.Equals(Text.apple))
                {
                    return apple;
                }

                if (fruit.Equals(Text.peach))
                {
                    return peach;
                }

                return fruitlessHeathen;
                
            }
            public static Discord.Color banana = new(128, 00, 128);
            public static Discord.Color kiwi = new(255, 255, 0);
            public static Discord.Color cherry = new(255, 0, 0);
            public static Discord.Color apple = new(255, 192, 203);
            public static Discord.Color peach = new(255, 229, 180);
            public static Discord.Color fruitlessHeathen = new(150, 75, 0);
        }
        public static class Text
        {
            public static string Get(string fruit)
            {
                if (fruit is null)
                {
                    return fruitlessHeathen;
                }
                if (fruit.Equals(banana))
                {
                    return banana;
                }

                if (fruit.Equals(kiwi))
                {
                    return kiwi;
                }

                if (fruit.Equals(cherry))
                {
                    return cherry;
                }

                if (fruit.Equals(apple))
                {
                    return apple;
                }

                if (fruit.Equals(peach))
                {
                    return peach;
                }

                if (fruit.Equals(Text.peach))
                {
                    return peach;
                }

                return fruitlessHeathen;
 
            }
            public static string banana { get; } = "Banana";
            public static string kiwi { get; } = "Kiwi";
            public static string cherry { get; } = "Cherry";
            public static string apple { get; } = "Apple";
            public static string peach { get; } = "Peach";
            public static string fruitlessHeathen { get; } = "Fruitless Heathen";


            public static bool TryParse(string input, out string fruit)
            {
                if (input is null)
                {
                    fruit = null;
                    return false;
                }
                else if (string.IsNullOrEmpty(input))
                {
                    fruit = fruitlessHeathen;
                    return true;
                }
                else if (input.Equals(Text.banana, StringComparison.OrdinalIgnoreCase) || input.Equals(TextPlural.banana, StringComparison.OrdinalIgnoreCase))
                {
                    fruit = banana;
                    return true;
                }
                else if (input.Equals(Text.kiwi, StringComparison.OrdinalIgnoreCase) || input.Equals(TextPlural.kiwi, StringComparison.OrdinalIgnoreCase))
                {
                    fruit = kiwi;
                    return true;
                }
                else if (input.Equals(Text.cherry, StringComparison.OrdinalIgnoreCase) || input.Equals(TextPlural.cherry, StringComparison.OrdinalIgnoreCase))
                {
                    fruit = cherry;
                    return true;
                }
                else if (input.Equals(Text.apple, StringComparison.OrdinalIgnoreCase) || input.Equals(TextPlural.apple, StringComparison.OrdinalIgnoreCase))
                {
                    fruit = apple;
                    return true;
                }
                else if (input.Equals(Text.peach, StringComparison.OrdinalIgnoreCase) || input.Equals(TextPlural.peach, StringComparison.OrdinalIgnoreCase))
                {
                    fruit = peach;
                    return true;
                }
                else
                {
                    fruit = null;
                    return false;
                }

            }
        }
        public static class TextPlural
        {
            public static string Get(string fruit)
            {
                if (fruit is null)
                {
                    return fruitlessHeathen;
                }
                if (fruit.Equals(Text.banana))
                {
                    return banana;
                }

                if (fruit.Equals(Text.kiwi))
                {
                    return kiwi;
                }

                if (fruit.Equals(Text.cherry))
                {
                    return cherry;
                }

                if (fruit.Equals(Text.apple))
                {
                    return apple;
                }
                
                if (fruit.Equals(Text.peach))
                {
                    return peach;
                }
                
                return fruitlessHeathen;
            }
            public static string banana { get; } = "Bananas";
            public static string kiwi { get; } = "Kiwis";
            public static string cherry { get; } = "Cherries";
            public static string apple { get; } = "Apples";
            public static string peach { get; } = "Peaches";
            public static string fruitlessHeathen { get; } = "Fruitless Heathens";


        }

        public static class Logos
        {
            public static string Get(string fruit)
            {
                if (fruit is null)
                {
                    return fruitlessHeathen;
                }
                if (fruit.Equals(Text.banana))
                {
                    return banana;
                }

                if (fruit.Equals(Text.kiwi))
                {
                    return kiwi;
                }

                if (fruit.Equals(Text.cherry))
                {
                    return cherry;
                }

                if (fruit.Equals(Text.apple))
                {
                    return apple;
                }

                if (fruit.Equals(Text.peach))
                {
                    return peach;
                }

                return fruitlessHeathen;
            }
            public static string banana { get; }
                = "https://cdn.discordapp.com/attachments/856679881547186196/859869023607717898/BananaThing.png";

            public static string kiwi { get; }
                = "https://cdn.discordapp.com/attachments/856679881547186196/859869014507257896/KiwiThing.png";

            public static string cherry { get; }
                = "https://cdn.discordapp.com/attachments/856679881547186196/859869003572838420/CherryThing.png";

            public static string apple { get; }
                = "https://cdn.discordapp.com/attachments/856679881547186196/859868992339968050/AppleThing.png";

            public static string peach { get; }
                = "https://cdn.discordapp.com/attachments/856679881547186196/859868992339968050/PeachThing.png";

            public static string fruitlessHeathen { get; }
                = "https://runescape.wiki/images/b/b8/Ugthanki_dung_detail.png";

        }
        public static class Emojis
        {
            public static string Get(string fruit)
            {
                if (fruit is null)
                {
                    return fruitlessHeathen;
                }
                else if (fruit.Equals(Text.banana))
                {
                    return banana;
                }

                else if (fruit.Equals(Text.kiwi))
                {
                    return kiwi;
                }

                else if (fruit.Equals(Text.cherry))
                {
                    return cherry;
                }

                else if (fruit.Equals(Text.apple))
                {
                    return apple;
                }
                
                if (fruit.Equals(Text.peach))
                {
                    return peach;
                }

                return fruitlessHeathen;
                

            }
            public static string banana { get; } = "🍌";

            public static string kiwi { get; } = "🥝";

            public static string cherry { get; } = "🍒";

            public static string apple { get; } = "🍎";

            public static string peach { get; } = "🍑";

            public static string fruitlessHeathen { get; } = "💩";


        }
        //🍌🥝🍒🍎🍑💩
    }
}
