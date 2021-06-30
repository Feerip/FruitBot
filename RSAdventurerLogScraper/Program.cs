using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace RSAdventurerLogScraper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            List<string> voughtPlayers = new();
            voughtPlayers.Add("Feerip");
            voughtPlayers.Add("9tails");
            //voughtPlayers.Add("Servanok");
            voughtPlayers.Add("x_hadess");
            voughtPlayers.Add("thizly%20wizly");
            //Thread.Sleep(60000);
            voughtPlayers.Add("Dilli");
            //Thread.Sleep(15000);
            //vought = new(voughtPlayers.ToArray());

            //IWebDriver driver = new ChromeDriver(@"D:\Download\chromedriver");


            //var driver = new ChromeDriver();
            //driver.Url = "https://runepixels.com/clans/vought/about";
            //driver.Navigate();
            ////            new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(
            ////d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
            ////            Thread.Sleep(30000);
            ////WebDriverWait wdw2 = (WebDriverWait)new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            ////ChromeWebElement element = wdw2.Until(ExpectedConditions.ElementToBeClickable(By.ClassName("activities")));

            //new WebDriverWait(driver, TimeSpan.FromMinutes(5)).Until(ExpectedConditions.ElementExists(By.ClassName("activities")));
            //IWebElement activitiesTable = driver.FindElementByClassName("activities");
            //IWebElement switchElement = activitiesTable.FindElement(By.TagName("switch"));
            //IWebElement dropsButton = switchElement.FindElement(By.XPath("//span[5]"));


            //dropsButton.Click();

            //new WebDriverWait(driver, TimeSpan.FromMinutes(5)).Until(ExpectedConditions.ElementExists(By.ClassName("activity")));
            ////IList<IWebElement> activities = driver.FindElementsByClassName("activity");



            List<DropLogEntry> scraped = await DropLogEntry.CreateListFullAuto();
            

            foreach(DropLogEntry entry in scraped)
            {
           
            }


            //{Element (id = ab88da5a-8fee-41c7-9482-354fd167084d)}
            Console.WriteLine();
            //var source = driver.PageSource;


        }
    }
}
