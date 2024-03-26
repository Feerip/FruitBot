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
                if (fruit.Equals(Text.pineapple))
                {
                    return pineapple;
                }

                if (fruit.Equals(Text.kiwi))
                {
                    return kiwi;
                }

                if (fruit.Equals(Text.apple))
                {
                    return apple;
                }

                if (fruit.Equals(Text.bean))
                {
                    return bean;
                }
                else
                {
                    return fruitlessHeathen;
                }
            }
            public static Discord.Color pineapple = new(128, 00, 128);
            public static Discord.Color kiwi = new(255, 255, 0);
            public static Discord.Color apple = new(255, 0, 0);
            public static Discord.Color bean = new(255, 192, 203);
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
                if (fruit.Equals(pineapple))
                {
                    return pineapple;
                }

                if (fruit.Equals(kiwi))
                {
                    return kiwi;
                }

                if (fruit.Equals(apple))
                {
                    return apple;
                }

                if (fruit.Equals(bean))
                {
                    return bean;
                }
                else
                {
                    return fruitlessHeathen;
                }
            }
            public static string pineapple { get; } = "Pineapple";
            public static string kiwi { get; } = "Kiwi";
            public static string apple { get; } = "Apple";
            public static string bean { get; } = "Bean";
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
                else if (input.Equals(Text.pineapple, StringComparison.OrdinalIgnoreCase) || input.Equals(TextPlural.pineapple, StringComparison.OrdinalIgnoreCase))
                {
                    fruit = pineapple;
                    return true;
                }
                else if (input.Equals(Text.kiwi, StringComparison.OrdinalIgnoreCase) || input.Equals(TextPlural.kiwi, StringComparison.OrdinalIgnoreCase))
                {
                    fruit = kiwi;
                    return true;
                }
                else if (input.Equals(Text.apple, StringComparison.OrdinalIgnoreCase) || input.Equals(TextPlural.apple, StringComparison.OrdinalIgnoreCase))
                {
                    fruit = apple;
                    return true;
                }
                else if (input.Equals(Text.bean, StringComparison.OrdinalIgnoreCase) || input.Equals(TextPlural.bean, StringComparison.OrdinalIgnoreCase))
                {
                    fruit = bean;
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
                if (fruit.Equals(Text.pineapple))
                {
                    return pineapple;
                }

                if (fruit.Equals(Text.kiwi))
                {
                    return kiwi;
                }

                if (fruit.Equals(Text.apple))
                {
                    return apple;
                }

                if (fruit.Equals(Text.bean))
                {
                    return bean;
                }
                else
                {
                    return fruitlessHeathen;
                }
            }
            public static string pineapple { get; } = "Pineapples";
            public static string kiwi { get; } = "Kiwis";
            public static string apple { get; } = "Apples";
            public static string bean { get; } = "Beans";
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
                if (fruit.Equals(Text.pineapple))
                {
                    return pineapple;
                }

                if (fruit.Equals(Text.kiwi))
                {
                    return kiwi;
                }

                if (fruit.Equals(Text.apple))
                {
                    return apple;
                }

                if (fruit.Equals(Text.bean))
                {
                    return bean;
                }
                else
                {
                    return fruitlessHeathen;
                }
            }
            public static string pineapple { get; }
                = "https://cdn.discordapp.com/attachments/856679881547186196/859869023607717898/PineappleThing.png";

            public static string kiwi { get; }
                = "https://cdn.discordapp.com/attachments/856679881547186196/859869014507257896/KiwiThing.png";

            public static string apple { get; }
                = "https://cdn.discordapp.com/attachments/856679881547186196/859869003572838420/AppleThing.png";

            public static string bean { get; }
                = "https://cdn.discordapp.com/attachments/856679881547186196/859868992339968050/BeanThing.png";
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
                else if (fruit.Equals(Text.pineapple))
                {
                    return pineapple;
                }

                else if (fruit.Equals(Text.kiwi))
                {
                    return kiwi;
                }

                else if (fruit.Equals(Text.apple))
                {
                    return apple;
                }

                else if (fruit.Equals(Text.bean))
                {
                    return bean;
                }
                else
                {
                    return fruitlessHeathen;
                }

            }
            public static string pineapple { get; } = "🍍";

            public static string kiwi { get; } = "🥝";

            public static string apple { get; } = "🍎";

            public static string bean { get; } = "🫘";
            public static string fruitlessHeathen { get; } = "💩";


        }
        //🍍🥝🍎🫘💩
    }
}
