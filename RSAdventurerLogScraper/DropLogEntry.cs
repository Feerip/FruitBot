using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace RSAdventurerLogScraper
{
    public class DropLogEntry
    {
        public string _playerName { get; }
        public string _fruit { get; }
        public string _dropName { get; }
        public string _timestamp { get; }
        public string _playerAvatarPNG { get; set; }
        public string _dropIconWEBP { get; set; }
        public string _bossName { get; set; }
        public string _runemetricsDropID { get; set; }
        public string _pointValue { get; set; }
        



        public string _entryKey { get; }



        // Standard ctor
        public DropLogEntry(object playerName, object fruit, object dropName, object timestamp,
                            object playerAvatarPNG = null, object dropIconWEBP = null,
                            object bossName = null, object runemetricsDropID = null, object pointValue = null, object entryKey = null)
        {
            // Required
            _playerName = playerName.ToString();
            _fruit = fruit.ToString();
            _dropName = SanitizeDropName(dropName.ToString());
            _timestamp = timestamp.ToString();

            // Image links
            if (playerAvatarPNG != null)
                _playerAvatarPNG = playerAvatarPNG.ToString();
            else
                _playerAvatarPNG = null;
            if (dropIconWEBP != null)
                _dropIconWEBP = dropIconWEBP.ToString() ?? null;
            else
                _dropIconWEBP = null;


            // Runemetrics stuff
            if (bossName != null)
                _bossName = bossName.ToString();
            else
                _bossName = null;
            if (runemetricsDropID != null)
                _runemetricsDropID = runemetricsDropID.ToString();
            else
                _runemetricsDropID = null;
            if (pointValue != null)
                _pointValue = pointValue.ToString();
            else
                _pointValue = null;

            // Calculate unique ID to ensure no duplicates
            // playerName + timestamp is an easy way to get a unique ID for each drop
            _entryKey = _timestamp + " " + _playerName;

           
            if (entryKey != null)
            {
                if (!String.Equals((_timestamp + " " + _playerName), _entryKey))
                {
                    throw new DataException("DropLogEntry data corrupted: entry key verification error while downloading entry");
                }
            }

            if (EntryKeyCorrupted)
                throw new DataException("DropLogEntry data corrupted: Entry key does not match expected result.");

        }

        // Automatic ctor (pass in an instance of the javascript class "activity" on runepixels)
        public DropLogEntry(IWebElement activity)
        {
            // Pull stuff from javascript generated xml first
            IWebElement playerNameElement = activity.FindElement(By.XPath("./a/player-name/span"));
            IWebElement dropNameElement = activity.FindElement(By.ClassName("text"));
            IWebElement timestampElement = activity.FindElement(By.ClassName("date"));
            IWebElement playerAvatarPNGHyperlinkElement = activity.FindElement(By.ClassName("avatar"));
            IWebElement dropItemWEBPHyperlinkElement = activity.FindElement(By.ClassName("icon"));

            // Pull strings from those elements
             _playerAvatarPNG = playerAvatarPNGHyperlinkElement.GetAttribute("src");
             _dropIconWEBP = dropItemWEBPHyperlinkElement.GetAttribute("src");
             _playerName = playerNameElement.Text;
             _dropName = SanitizeDropName(dropNameElement.Text);
             _timestamp = timestampElement.Text;

            // Calculate unique ID to ensure no duplicates
            // playerName + timestamp is an easy way to get a unique ID for each drop
            _entryKey = _timestamp + " " + _playerName;

            if (EntryKeyCorrupted)
                throw new DataException("DropLogEntry data corrupted: Entry key does not match expected result.");
        }

        public static bool operator ==(DropLogEntry lhs, DropLogEntry rhs)
        {
            if (lhs._entryKey == rhs._entryKey)
                return true;
            else
                return false;
        }
        public static bool operator !=(DropLogEntry lhs, DropLogEntry rhs)
        {
            if (lhs._entryKey != rhs._entryKey)
                return true;
            else
                return false;
        }

        public static List<DropLogEntry> CreateListFromWebElements(IList<IWebElement> input)
        {
            List<DropLogEntry> output = new();

            foreach (IWebElement activity in input)
            {
                output.Add(new(activity));
            }

            return output;
        }


        // Automatically generates a full list of the last 50 drops in the clan.
        public static async Task<List<DropLogEntry>> CreateListFullAuto()
        {
            List<DropLogEntry> output;

            // Add option to use chrome in headless mode because the constant browsers crowding my screen while working was getting real annoying
            ChromeOptions chromeOptions = new();
            chromeOptions.AddArguments(new List<string>() { "headless", "disable-gpu", "--window-size=1920,1080" });


            var driver = new ChromeDriver(chromeOptions);
            // Set to clan Vought, change url if different
            driver.Url = "https://runepixels.com/clans/vought/about";
            // Open the main page
            driver.Navigate();
            // Wait until table loads
            new WebDriverWait(driver, TimeSpan.FromMinutes(5)).Until(ExpectedConditions.ElementExists(By.ClassName("activities")));
            // Find the activities table
            IWebElement activitiesTable = driver.FindElementByClassName("activities");
            // Find the activity selector interface
            IWebElement switchElement = activitiesTable.FindElement(By.TagName("switch"));
            // Find the drops button within the selector interface
            IWebElement dropsButton = switchElement.FindElement(By.XPath("//span[5]"));
            // Click the drops button once found
            dropsButton.Click();
            // Wait for the drops table to populate
            new WebDriverWait(driver, TimeSpan.FromMinutes(5)).Until(ExpectedConditions.ElementExists(By.ClassName("activity")));
            // Table loaded and pulled into memory, send it to CreateListFromWebElements for processing
            output = CreateListFromWebElements(driver.FindElementsByClassName("activity"));
            // Once processed, we have a populated list of drop entries!
            return output;
        }

        // Pull missing data if needed (boss name and runemetrics dropID)
        public static DropLogEntry PullBossNameAndRunemetricsID(DropLogEntry input)
        {
            if (input.EntryKeyCorrupted)
                throw new DataException("DropLogEntry data corrupted: Entry key does not match expected result.");

            if (input._bossName == null || input._runemetricsDropID == null)
            {
                List<DropLogEntry> playerLog = JagexScraper.Scrape(input._playerName);

                

            }



            return input;
        }
        public bool EntryKeyCorrupted 
        { 
            get 
            {
                return !String.Equals((_timestamp + " " + _playerName), _entryKey);
            } 
        }

        // sanitization taken care of internally, pass DropLogEntry the drop name as is. 
        private string SanitizeDropName(string dropName)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;

            return ti.ToTitleCase(dropName.Replace("some ", "").Replace("pair of ", ""));
        }
    }
}
