using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RS3APIDropLog
{
    public class RSDropLog
    {
        public static List<RSDropLog> PullParallelFromJagexAPI(List<string> playerNames)
        {
            List<RSDropLog> output = new();
            ConcurrentQueue<RSDropLog> fastContainer = new();

            @Parallel.ForEach(playerNames, (playerName) => FastConstructor(playerName, fastContainer));

            output = fastContainer.ToList();

            return output;
        }
        private static void FastConstructor(string playerName, ConcurrentQueue<RSDropLog> list)
        {
            try
            {
                list.Enqueue(new(playerName));
            }
            catch (WebException)
            {
            }
        }
        public RSDropLog(string playerName)
        {
            string test = playerName.Replace(" ", "%20");

            using (WebClient wc = new WebClient())
            {

                string json = wc.DownloadString($"https://apps.runescape.com/runemetrics/profile/profile?user={test}&activities=20");


                RSProfile aDropLog = JsonConvert.DeserializeObject<RSProfile>(json);
                if (aDropLog.name == null)
                {
                    throw new WebException($"Jagex API returned null: player \"{test}\" not found.");
                }
                else
                {
                    _name = aDropLog.name;
                    _sanitizedDropLog = aDropLog.ProcessAndGetDrops();
                }
            }
        }
        public static async Task<List<string>> GetAllVoughtPlayerNames()
        {
            List<string> playerNames = new();

            string url = "http://services.runescape.com/m=clan-hiscores/members_lite.ws?clanName=Vought";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            TextFieldParser parser = new(stream, Encoding.UTF7);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            if (!parser.EndOfData)
            {
                parser.ReadFields();
            }

            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
                playerNames.Add(fields[0]);
            }
            return playerNames;


        }
        public string _name { get; set; }
        public string _timestamp { get; set; }
        public class SanitizedDrop
        {
            public DateTime _timestamp { get; set; }
            public string _dropname { get; set; }
        }
        public List<SanitizedDrop> _sanitizedDropLog { get; set; }

        public class RSProfile
        {
            public string name { get; set; }
            public string rank { get; set; }
            public int totalskill { get; set; }
            public double totalxp { get; set; }
            public int combatlevel { get; set; }
            public double magic { get; set; }
            public double melee { get; set; }
            public double ranged { get; set; }
            public int questsstarted { get; set; }
            public int questscomplete { get; set; }
            public int questsnotstarted { get; set; }
            public RSActivity[] activities { get; set; }
            public RSSkillValues[] skillvalues { get; set; }
            public bool loggedIn { get; set; }

            public class RSActivity
            {
                public DateTime date { get; set; }
                public string details { get; set; }
                public string text { get; set; }
            }
            public class RSSkillValues
            {
                public int level { get; set; }
                public double xp { get; set; }
                public int rank { get; set; }
                public int id { get; set; }
            }

            public List<SanitizedDrop> ProcessAndGetDrops()
            {
                // Clears non-drops
                List<SanitizedDrop> output = new();
                if (activities != null)
                {
                    foreach (RSActivity activity in activities)
                    {
                        if (activity.text.Contains("found", StringComparison.OrdinalIgnoreCase) /*|| activity.text.Contains("challenged by", StringComparison.OrdinalIgnoreCase)*/)
                        {
                            SanitizedDrop drop = new();
                            drop._dropname = activity.text;
                            drop._timestamp = activity.date;

                            output.Add(drop);
                        }
                    }
                }

                // Sanitizes drop names to match with DB
                TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
                foreach (SanitizedDrop drop in output)
                {
                    drop._dropname = ti.ToTitleCase(drop._dropname
                        .Replace("I found a pair of ", "")
                        .Replace("I found a set of ", "")
                        .Replace("I found some ", "")
                        .Replace("I found an ", "")
                        .Replace("I found a ", "")
                        .Replace("I found ", "")
                        .Replace("Found an ", "")
                        .Replace("Found a ", "")
                        .Replace("Challenged By ", "")
                        .Replace(".", ""));
                }
                return output;
            }
        }
    }
}
