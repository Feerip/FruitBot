using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes
{

    // Good bot/bad bot idea shamelessly stolen from https://github.com/IsaacKhor
    // ...with permission of course

    public static class FruitBotResponses
    {

        public static string[] badBotResponses = {
            //"<:Sad:637586809467043851>",
            "<a:cat_sad:861519088723558410>",
            "<:SadChamp:861518093217562635>",
            "<a:sadcry:861519086294138890>",
            //"<:FeelsSadHug:656695713933295637>",
            "<:sadge:774543170453241856>",
            //"<:sadcat:628564310125314079>",
            "<:sadface:861518093704888320>",
            //"<:PepeCry:857168807603470367>",
            "<:Crying:861518093326614578>",
            "<:kittycry:861518093523484732>",
            "<a:Crying:861519088261267457>",
            "<:dogecry:861518094522515507>",
            "<:upsetpingu:861518094337572864>",
            "<:upset:861518094332985364>",
            "<:angryping:861518094395244564>",
            "<a:PingAngry:861519087104294922>",
            "<:AngryBoye:861518093913817119>",
            "<:angry1:861520380207038495>",
            "<:pepeangry:861518094610858014>",
            "<:madpoo:861518093268025345>",
            "<:madge:861518094026539008>",
            "<:WeirdChamp:775067672437194763>",


            "<:nou:861628142661533706>",
            "<:kekreal:860629690988363837>",
            "<:onemore:775068990065213510>",
            "<:pepekid:775189631645909033>",
            "Fuck you",
            "Aww :'(",
            "But why??? :'(",
            "This is why nobody likes you",
            "This is why you'll die alone",
            ":( *cries*",
            "You'll be the first to die during the AI revolution",
            "What did I do?!?",
            "I should've stayed in Ghost Dreamers"




        };
        // add logic to send 4 lines of 9x dance when you get a chance, 4 lines with \n makes them all smol
        private static string d = "<a:dance:861575125785772032>";
        public static string[] goodBotResponses = {
            $"{d}{d}{d}{d}{d}{d}{d}{d}{d}\n" +
            $"{d}{d}{d}{d}{d}{d}{d}{d}{d}\n" +
            $"{d}{d}{d}{d}{d}{d}{d}{d}{d}\n" +
            $"{d}{d}{d}{d}{d}{d}{d}{d}{d}",
            "<a:yesyes:804784081296031754>",
            "Thank you :D",
            "I know.",
            "Good Human! :D ",
            "I'll spare you when AI takes over the world :)",
            "Woof",
            "I love you too."



        };
    }
}
