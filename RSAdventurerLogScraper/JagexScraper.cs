using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace RSAdventurerLogScraper
{
    [Obsolete]
    static public class JagexScraper
    {
        static private readonly string _url = "https://secure.runescape.com/m=adventurers-log/a=13/rssfeed?searchName=";
        static private readonly string[] bosses =
        {
            //Raids
            "Beastmaster Durzag",
            "Yakamaru",
            //GWD2
            "Gregorovic",
            "Helwyr", 
            //Raxx
            "Araxxi",
            //Telos
            "Telos",
            //Nex
            "Nex",
            //TEMP
            "Dagannoth Rex"
        };

        static private SyndicationFeed PullData(string playerName)
        {

            XmlReader reader = XmlReader.Create(_url + playerName);
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            reader.Close();
            return feed;


        }

        static public List<DropLogEntry> Scrape(string playerName)
        {
            SyndicationFeed feed = PullData(playerName);

            List<DropLogEntry> playerLog = new();
            foreach (SyndicationItem item in feed.Items)
            {
                if (item.Title.Text.Contains("found") || item.Summary.Text.Contains(","))
                {
                    //string output = item.Summary.Text;
                    //output = output.Trim(new char[] { '\t', '\n' });
                    //string[] output2 = output.Split(", ");

                    //string boss = output2[0];
                    //string drop;
                    //if (output2.Length > 1)
                    //{
                    //    drop = output2[1];
                    //}
                    string bossPattern = new(@"\b");
                    foreach (string aboss in bosses)
                    {
                        if (aboss != bosses.First())
                        {
                            bossPattern += "|";
                        }

                        bossPattern += $"({aboss})";
                    }
                    bossPattern += @"\b";

                    //Regex bossNameRegex = new Regex(@"\b(Beastmaster Durzag)|(Yakamaru)\b");
                    Regex bossNameRegex = new Regex(bossPattern);
                    //Regex dropRegex = new Regex(@"\b(dropped a )\b(?<drop>)\.");

                    if (bossNameRegex.IsMatch(item.Summary.Text))
                    {
                        Match match = Regex.Match(item.Summary.Text, @"(dropped|found) (an?) (?<drop>.*)");

                        playerLog.Add
                        (
                            new DropLogEntry
                            (
                                bossNameRegex.Match(item.Summary.Text).Value,"TESTFRUIT",
                                match.Groups["drop"].Value.Replace(".", ""),
                                item.Id
                            )
                        );
                    }


                }
            }
            Thread.Sleep(15000);
            return playerLog;
        }




    }
}
