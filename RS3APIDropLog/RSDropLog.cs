using Discord;

using Microsoft.VisualBasic.FileIO;

using Newtonsoft.Json;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RS3APIDropLog
{
    public class RSDropLog
    {

        public static HttpClient httpClient = new();

        public static List<RSDropLog> PullParallelFromJagexAPI(List<string> playerNames)
        {
            List<RSDropLog> output = new();

            ConcurrentQueue<string> jsonStrings = new();

            Stopwatch JAGXStopwatch = new();
            JAGXStopwatch.Start();
            Parallel.ForEach(playerNames, new ParallelOptions() { MaxDegreeOfParallelism = 10 }, playerName =>
            {
                GetALog(playerName, jsonStrings);
            });
            JAGXStopwatch.Stop();
            TimeSpan JAGXts = JAGXStopwatch.Elapsed;
            string JAGXElapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                JAGXts.Hours, JAGXts.Minutes, JAGXts.Seconds, JAGXts.Milliseconds / 10);
            Console.WriteLine("JAGX time " + JAGXElapsedTime);

            Console.WriteLine($"All ALogs pulled. Starting json deserialization...");

            foreach (string json in jsonStrings)
            {
                try
                {
                    RSProfile aProfile = JsonConvert.DeserializeObject<RSProfile>(json);
                    RSDropLog aDropLog = new(aProfile.name, aProfile);
                    output.Add(aDropLog);
                }
                catch (Exception e)
                {
                    Console.Out.WriteLineAsync(e.Message);
                }
            }

            Console.WriteLine($"Json deserialized, objects back to FruitPantry for processing...");

            return output;
        }
        public static void GetALog(string playerName, ConcurrentQueue<string> jsonStrings)
        {
            string RSN = playerName.Replace(" ", "%20");
            try
            {
                //Console.WriteLine($"Pulling ALog for {playerName}...");
                HttpResponseMessage response = null;
                do
                {
                    if (response is not null)
                    {
                        Console.Out.WriteLineAsync($"Error 503 for {playerName}. Retrying...");
                    }
                    response = httpClient.GetAsync($"https://apps.runescape.com/runemetrics/profile/profile?user={RSN}&activities=20").Result;

                }
                while (response.StatusCode == HttpStatusCode.ServiceUnavailable);
                string json = response.Content.ReadAsStringAsync().Result;
                if (json.Contains("PROFILE_PRIVATE"))
                {
                    Console.Out.WriteLineAsync($"FAILURE: ALog set to private for {playerName}.");
                }
                else
                {
                    //Console.Out.WriteLineAsync($"SUCCESS: Pulled ALog json for {playerName}.");
                    jsonStrings.Enqueue(json);
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLineAsync($"HttpClient failed to pull ALog for RSN {playerName}. Message: {e.Message}\n");
            }
        }
        public static async Task GetALogAsync(string playerName, ConcurrentQueue<string> jsonStrings)
        {
            string RSN = playerName.Replace(" ", "%20");
            try
            {
                //Console.WriteLine($"Pulling ALog for {playerName}...");
                HttpResponseMessage response = null;
                do
                {
                    if (response is not null)
                        await Console.Out.WriteLineAsync($"Error 503 for {playerName}. Retrying...");
                    response = await httpClient.GetAsync($"https://apps.runescape.com/runemetrics/profile/profile?user={RSN}&activities=20");

                }
                while (response.StatusCode == HttpStatusCode.ServiceUnavailable);
                string json = await response.Content.ReadAsStringAsync();
                if (json.Contains("PROFILE_PRIVATE"))
                    await Console.Out.WriteLineAsync($"FAILURE: ALog set to private for {playerName}.");
                else
                {
                    await Console.Out.WriteLineAsync($"SUCCESS: Pulled ALog json for {playerName}.");
                    jsonStrings.Enqueue(json);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"HttpClient failed to pull ALog for RSN {playerName}. Message: {e.Message}\n");
            }
        }

        [Obsolete]
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

        [Obsolete]
        public RSDropLog(string playerName)
        {
            string sanitezedPlayerName = playerName.Replace(" ", "%20");
            string url = $"https://apps.runescape.com/runemetrics/profile/profile?user={sanitezedPlayerName}&activities=20";

            using (WebClient wc = new WebClient())
            {

                string json = wc.DownloadString(url);


                RSProfile aDropLog = JsonConvert.DeserializeObject<RSProfile>(json);
                if (aDropLog.name == null)
                {
                    throw new WebException($"Jagex API returned null: player \"{sanitezedPlayerName}\" not found.");
                }
                else
                {
                    _name = aDropLog.name;
                    _sanitizedDropLog = aDropLog.ProcessAndGetDrops();
                }
            }
        }

        public RSDropLog(string RSN, RSProfile aDropLog)
        {
            if (aDropLog.name == null)
            {
                throw new WebException($"Jagex API returned null: player \"{RSN}\" not found.");
            }
            else
            {
                _name = aDropLog.name;
                _sanitizedDropLog = aDropLog.ProcessAndGetDrops();
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

            int numPlayers = 0;
            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
#if DEBUG
                //if (!fields[0].ToLower().Equals("tygogo"))
                //continue;
#endif
                playerNames.Add(fields[0]);
                numPlayers++;
#if DEBUG
                //if (numPlayers > 20) break;
#endif
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
                        if (activity.text.Contains("found ", StringComparison.OrdinalIgnoreCase) /*|| activity.text.Contains("challenged by", StringComparison.OrdinalIgnoreCase)*/)
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
                        .Replace("Found ", "")
                        .Replace("Challenged By ", "")
                        .Replace(".", ""));
                }
                output.Reverse();
                return output;
            }
        }
    }
}
