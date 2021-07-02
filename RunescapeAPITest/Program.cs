using System;
using System.Collections.Generic;
using Runescape.Api;
using Runescape.Api.Model;

namespace RunescapeAPITest
{
    class Program
    {
        static void Main(string[] args)
        {

            IHiscores scores = ApiFactory.CreateHiscores();

            //IReadOnlyList<IClanMember> clan =  scores.GetClanMembers("vought");


            Console.WriteLine();

        }
    }
}
