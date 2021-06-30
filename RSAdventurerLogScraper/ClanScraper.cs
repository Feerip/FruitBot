using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSAdventurerLogScraper
{
    public class ClanScraper
    {
        private Dictionary<string, List<DropLogEntry>> _clan_log = new();

        public ClanScraper(string[] players)
        {
            foreach (string playerName in players)
            {
                AddPlayer(playerName);
            }

        }

        public int AddPlayer(string playerName)
        {
            //try
            //{
                _clan_log.Add(playerName, JagexScraper.Scrape(playerName));
                return 0;
            //}
            //catch (Exception e)
            //{
            //    Console.Write(playerName + " ");
            //    Console.WriteLine(e.Message);
            //    return 1;
            //}
        }

        public int UpdatePlayerLog(string playerName)
        {
            _clan_log[playerName] = JagexScraper.Scrape(playerName);
            return 0;
        }

        public int UpdateAllPlayerLogs()
        {
            foreach (string playerName in _clan_log.Keys.ToList<string>())
            {
                UpdatePlayerLog(playerName);
            }
            return 0;
        }




    }
}
