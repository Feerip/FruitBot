using System;

namespace DataTypes
{
    public static class FruitResources
    {
        public static class Colors
        {
            public static Discord.Color Get(string fruit)
            {
                if (fruit is null)
                {
                    return fruitlessHeathen;
                }
                if (fruit.Equals(Text.grape))
                {
                    return grape;
                }

                if (fruit.Equals(Text.banana))
                {
                    return banana;
                }

                if (fruit.Equals(Text.apple))
                {
                    return apple;
                }

                if (fruit.Equals(Text.peach))
                {
                    return peach;
                }
                else
                {
                    return fruitlessHeathen;
                }
            }
            public static Discord.Color grape = new(128, 00, 128);
            public static Discord.Color banana = new(255, 255, 0);
            public static Discord.Color apple = new(255, 0, 0);
            public static Discord.Color peach = new(255, 192, 203);
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
                if (fruit.Equals(grape))
                {
                    return grape;
                }

                if (fruit.Equals(banana))
                {
                    return banana;
                }

                if (fruit.Equals(apple))
                {
                    return apple;
                }

                if (fruit.Equals(peach))
                {
                    return peach;
                }
                else
                {
                    return fruitlessHeathen;
                }
            }
            public static string grape { get; } = "Grape";
            public static string banana { get; } = "Banana";
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
                else if (input.Equals(Text.grape, StringComparison.OrdinalIgnoreCase) || input.Equals(TextPlural.grape, StringComparison.OrdinalIgnoreCase))
                {
                    fruit = grape;
                    return true;
                }
                else if (input.Equals(Text.banana, StringComparison.OrdinalIgnoreCase) || input.Equals(TextPlural.banana, StringComparison.OrdinalIgnoreCase))
                {
                    fruit = banana;
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
                if (fruit.Equals(Text.grape))
                {
                    return grape;
                }

                if (fruit.Equals(Text.banana))
                {
                    return banana;
                }

                if (fruit.Equals(Text.apple))
                {
                    return apple;
                }

                if (fruit.Equals(Text.peach))
                {
                    return peach;
                }
                else
                {
                    return fruitlessHeathen;
                }
            }
            public static string grape { get; } = "Grapes";
            public static string banana { get; } = "Bananas";
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
                if (fruit.Equals(Text.grape))
                {
                    return grape;
                }

                if (fruit.Equals(Text.banana))
                {
                    return banana;
                }

                if (fruit.Equals(Text.apple))
                {
                    return apple;
                }

                if (fruit.Equals(Text.peach))
                {
                    return peach;
                }
                else
                {
                    return fruitlessHeathen;
                }
            }
            public static string grape { get; }
                = "https://cdn.discordapp.com/attachments/856679881547186196/859869023607717898/GrapeThing.png";

            public static string banana { get; }
                = "https://cdn.discordapp.com/attachments/856679881547186196/859869014507257896/BananaThing.png";

            public static string apple { get; }
                = "https://cdn.discordapp.com/attachments/856679881547186196/859869003572838420/AppleThing.png";

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
                else if (fruit.Equals(Text.grape))
                {
                    return grape;
                }

                else if (fruit.Equals(Text.banana))
                {
                    return banana;
                }

                else if (fruit.Equals(Text.apple))
                {
                    return apple;
                }

                else if (fruit.Equals(Text.peach))
                {
                    return peach;
                }
                else
                {
                    return fruitlessHeathen;
                }

            }
            public static string grape { get; } = "🍇";

            public static string banana { get; } = "🍌";

            public static string apple { get; } = "🍎";

            public static string peach { get; } = "🍑";
            public static string fruitlessHeathen { get; } = "💩";


        }
        //🍇🍌🍎🍑💩
    }
}
