using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace FruitBot
{
    public static class EmbedGenerator
    {
        private static FruitPantry.FruitPantry _thePantry = FruitPantry.FruitPantry.GetFruitPantry();


        public static Embed SignupEmbed(IUser user)
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle($"Fruit Wars Signup")
                .WithDescription($"Hello {user.Mention}, Please click on one of the fruits below to sign up for a team.")
                
                ;










        


            return builder.Build();
        }
    }
}
