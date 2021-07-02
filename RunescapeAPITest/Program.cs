using System;
using System.Collections.Generic;
using Runescape.Api;
using Runescape.Api.Model;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using Newtonsoft.Json;
using System.Diagnostics;

namespace RunescapeAPITest
{
    class Program
    {
        public class RSProfile
        {
            public string name { get; set; }
            public string rank { get; set; }
            public int totalskill { get; set; }
            public int totalxp { get; set; }
            public int combatlevel { get; set; }
            public int magic { get; set; }
            public int melee { get; set; }
            public int ranged { get; set; }
            public int questsstarted { get; set; }
            public int questscomplete { get; set; }
            public int questsnotstarted { get; set; }
            public string[][][] activities { get; set; }
            public string[][] skillvalues { get; set; }
            public bool loggedIn { get; set; }
        }


        static async Task Main(string[] args)
        {

            IHiscores scores = ApiFactory.CreateHiscores();

            IReadOnlyList<IClanMember> clan = await scores.GetClanMembersAsync("vought");



            int idx = 0;
            foreach (IClanMember clannie in clan)
            {


                string json;
                using (WebClient wc = new WebClient())
                {
                    Stopwatch stopwatch = new();
                    Stopwatch stopwatch2 = new();

                    stopwatch.Start();
                    json = wc.DownloadString($"https://apps.runescape.com/runemetrics/profile/profile?user={clannie.Name}&activities=20");
                    stopwatch.Stop();
                    TimeSpan ts1 = stopwatch.Elapsed;

                    var deserialized = JsonConvert.DeserializeObject<dynamic>(json);


                    Console.WriteLine($"Data pull took {ts1.Milliseconds} ms for clannie {clannie.Name}.");
                }
                idx++;


            }

            //var deserializedProduct = JsonConvert.DeserializeObject<dynamic>(json);



            Console.WriteLine();

        }
    }
}
