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
using DataTypes;
using Discord;
using RunescapeAPITest;
using DataTypes;

namespace RSAdventurerLogScraper
{
    public class DropLogEntry
    {

        


        public string _playerName { get; }
        public string _fruit { get; set; }
        public string _dropName { get; }
        public string _timestamp { get; }
        public string _playerAvatarPNG { get; set; }
        public string _dropIconWEBP { get; set; }
        public string _bossName { get; set; }
        public string _runemetricsDropID { get; set; }
        public string _pointValue { get; set; }

        public string _fruitLogo
        {
            get
            {
                if (_fruit.Equals(FruitResources.Text.grape))
                    return FruitResources.Logos.grape;
                if (_fruit.Equals(FruitResources.Text.banana))
                    return FruitResources.Logos.banana;
                if (_fruit.Equals(FruitResources.Text.apple))
                    return FruitResources.Logos.apple;
                if (_fruit.Equals(FruitResources.Text.peach))
                    return FruitResources.Logos.peach;
                else
                    return FruitResources.Logos.fruitlessHeathen;
                //placeholder for "fruitless heathen"
            }
        }


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

            output.Reverse();
            return output;
        }
        public DropLogEntry(string playerName, RSDropLog.SanitizedDrop input)
        {

            _playerName = playerName;
            _dropName = input._dropname;
            _timestamp = input._timestamp.ToString("MM-dd-yyyy HH:mm"); //FIX THIS LATER FOR THE LOVE OF GOD
            
            // Calculate unique ID to ensure no duplicates
            // playerName + timestamp is an easy way to get a unique ID for each drop
            _entryKey = _timestamp + " " + _playerName;

            if (EntryKeyCorrupted)
                throw new DataException("DropLogEntry data corrupted: Entry key does not match expected result.");
        }

        // Automatically generates a full list of the last 50 drops in the clan.
        public static async Task<List<DropLogEntry>> CreateListFullAuto()
        {
            Console.WriteLine("===============================================Starting pull from RuneMetrics API");
            List<DropLogEntry> output = new();

            List<RSDropLog> dropLogs = RSDropLog.PullParallelFromJagexAPI(RSDropLog.GetAllVoughtPlayerNames().Result);

            foreach (RSDropLog playerLog in dropLogs)
            {
                foreach (RSDropLog.SanitizedDrop sanitizedDrop in playerLog._sanitizedDropLog)
                {
                    output.Add(new(playerLog._name, sanitizedDrop));
                }
            }



            ////========================REMOVE THIS LOL==================
            ////return new();
            //// Add option to use chrome in headless mode because the constant browsers crowding my screen while working was getting real annoying
            //ChromeOptions chromeOptions = new();
            //chromeOptions.AddArguments(new List<string>() { "detach", "headless", "disable-gpu", "--window-size=1920,1080" });


            //var driver = new ChromeDriver(chromeOptions);
            //// Set to clan Vought, change url if different
            //driver.Url = "https://runepixels.com/clans/vought/about";
            //// Open the main page
            //driver.Navigate();
            //// Wait until table loads
            //new WebDriverWait(driver, TimeSpan.FromMinutes(5)).Until(ExpectedConditions.ElementExists(By.ClassName("activities")));
            //// Find the activities table
            //IWebElement activitiesTable = driver.FindElementByClassName("activities");
            //// Find the activity selector interface
            //IWebElement switchElement = activitiesTable.FindElement(By.TagName("switch"));
            //// Find the drops button within the selector interface
            //IWebElement dropsButton = switchElement.FindElement(By.XPath("//span[5]"));
            //// Click the drops button once found
            //dropsButton.Click();
            //// Wait for the drops table to populate
            //new WebDriverWait(driver, TimeSpan.FromMinutes(5)).Until(ExpectedConditions.ElementExists(By.ClassName("activity")));
            //// Table loaded and pulled into memory, send it to CreateListFromWebElements for processing
            //output = CreateListFromWebElements(driver.FindElementsByClassName("activity"));
            //driver.Quit();
            ////driver.Close();
            //// Once processed, we have a populated list of drop entries!
            return output;
        }

        public static Dictionary<string, ItemDatabaseEntry> PullWikiImages(Dictionary<string, ItemDatabaseEntry> itemDBEntries)
        {


            // Add option to use chrome in headless mode because the constant browsers crowding my screen while working was getting real annoying
            ChromeOptions chromeOptions = new();
            chromeOptions.AddArguments(new List<string>() { "headless", "disable-gpu", "--window-size=1920,1080" });


            var driver = new ChromeDriver(chromeOptions);
            foreach (KeyValuePair<string, ItemDatabaseEntry> entry in itemDBEntries)
            {

                driver.Url = entry.Value._wikiLink;
                // Open the main page
                driver.Navigate();
                try
                {
                    // Wait until table loads
                    new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.ElementExists(By.ClassName("floatleft")));
                    // Find the activities table
                    IWebElement activitiesTable = driver.FindElementByClassName("floatleft");
                    // Find the activity selector interface
                    IWebElement switchElement = activitiesTable.FindElement(By.ClassName("image"));
                    // Find the drops button within the selector interface
                    IWebElement dropsButton = switchElement.FindElement(By.TagName("img"));
                    // Click the drops button once found
                    switchElement.Click();

                    new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.ElementExists(By.CssSelector("img[crossorigin='anonymous']")));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"==============================================Image URL Pull failed for {entry.Key}. Continuing onto next item.");
                    continue;
                }                //IWebElement button = driver.findElement(By.cssSelector("input[value='Submit']"));
                IWebElement imageLargeContainer = driver.FindElementByCssSelector("img[crossorigin='anonymous']");

                //IWebElement imageLarge = imageLargeContainer.FindElement(By.TagName("img"));
                string imageSrc = imageLargeContainer.GetAttribute("src");

                itemDBEntries[entry.Key]._imageURL = imageSrc;
            }
            return itemDBEntries;
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
