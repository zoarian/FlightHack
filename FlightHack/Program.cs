using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading;

namespace Scraper
{
    class Program
    {
        static void Main(string[] args)
        {
            int SleepTimer = 100;
            string URL = "https://matrix.itasoftware.com/search";

            string MultiCityTabID = "mat-tab-label-0-2";
            string AddFlightButtonXPath = "/html/body/app-root/matrix-search-page/mat-card[1]/mat-card-content/form/matrix-select-flight-tabs/mat-tab-group/div/mat-tab-body[3]/div/matrix-multi-city-search-tab/div/div[2]/mat-chip";
            string SearchButtonXpath = "/html/body/app-root/matrix-search-page/mat-card[1]/mat-card-content/form/div[2]/button";

            string LegOneOriginCityCodeID = "mat-chip-list-input-4";
            string LegOneDestinationCityCodeID = "mat-chip-list-input-5";
            string LegOneDepartureDateID = "mat-input-8";

            string LegTwoOriginCityCodeID = "mat-chip-list-input-6";
            string LegTwoDestinationCityCodeID = "mat-chip-list-input-7";
            string LegTwoDepartureDateID = "mat-input-10";

            string DumpLegOriginCityCodeID = "mat-chip-list-input-8";
            string DumpLegDestinationCityCodeID = "mat-chip-list-input-9";
            string DumpLegDepartureDateID = "mat-input-11";

            // 1st Leg Human Details
            string LegOneOriginCityCode = "GVA;";
            string LegOneDestinationCityCode = "JFK;";
            string LegOneDepartureDate = "10/30/2022";

            // 2nd Leg Human Details
            string LegTwoOriginCityCode = LegOneDestinationCityCode;
            string LegTwoDestinationCityCode = LegOneOriginCityCode;
            string LegTwoDepartureDate = "11/06/2022";

            // Dump Leg Human Details
            string DumpLegOriginCityCode = "JNB";
            string DumpLegDestinationCityCode = "CAI";
            string DumpLegDepartureDate = "11/08/2022";

            // Starting Search
            Console.WriteLine("Initiating the browser");

            IWebDriver driver = new ChromeDriver();
                       driver.Url = URL;

            Thread.Sleep(SleepTimer*20);

            IWebElement EMultiButton = driver.FindElement(By.Id(MultiCityTabID));
            EMultiButton.Click();

            Console.WriteLine("Changed Tabs");

            IWebElement EAddFlights = driver.FindElement(By.XPath(AddFlightButtonXPath));
            EAddFlights.Click();

            Thread.Sleep(SleepTimer*10);

            EAddFlights.Click();

            Console.WriteLine("Added Flight Fields");

            IWebElement ELeg1CityCode = driver.FindElement(By.Id(LegOneOriginCityCodeID));
            ELeg1CityCode.SendKeys(LegOneOriginCityCode);
            ELeg1CityCode.SendKeys(Keys.Tab);

            Thread.Sleep(SleepTimer);

            IWebElement ELeg1DepCode = driver.FindElement(By.Id(LegOneDestinationCityCodeID));
            ELeg1DepCode.SendKeys(LegOneDestinationCityCode);
            ELeg1DepCode.SendKeys(Keys.Tab);

            Thread.Sleep(SleepTimer);

            IWebElement ELeg1DepartureDate = driver.FindElement(By.Id(LegOneDepartureDateID));
            ELeg1DepartureDate.SendKeys(LegOneDepartureDate);
            ELeg1DepartureDate.SendKeys(Keys.Tab);

            Thread.Sleep(SleepTimer);

            Console.WriteLine("Populated First Leg");

            IWebElement ELeg2CityCode = driver.FindElement(By.Id(LegTwoOriginCityCodeID));
            ELeg2CityCode.SendKeys(LegTwoOriginCityCode);
            ELeg2CityCode.SendKeys(Keys.Tab);

            Thread.Sleep(SleepTimer);

            IWebElement ELeg2DepCode = driver.FindElement(By.Id(LegTwoDestinationCityCodeID));
            ELeg2DepCode.SendKeys(LegTwoDestinationCityCode);
            ELeg2DepCode.SendKeys(Keys.Tab);

            Thread.Sleep(SleepTimer);

            IWebElement ELeg2DepartureDate = driver.FindElement(By.Id(LegTwoDepartureDateID));
            ELeg2DepartureDate.SendKeys(LegTwoDepartureDate);
            ELeg2DepartureDate.SendKeys(Keys.Tab);

            Thread.Sleep(SleepTimer);

            Console.WriteLine("Populated Second Leg");

            IWebElement EDumpOriginCityCode = driver.FindElement(By.Id(DumpLegOriginCityCodeID));
            EDumpOriginCityCode.SendKeys(DumpLegOriginCityCode);
            EDumpOriginCityCode.SendKeys(Keys.Tab);

            Thread.Sleep(SleepTimer);

            IWebElement EDumpDepCode = driver.FindElement(By.Id(DumpLegDestinationCityCodeID));
            EDumpDepCode.SendKeys(DumpLegDestinationCityCode);
            EDumpDepCode.SendKeys(Keys.Tab);
            
            Thread.Sleep(SleepTimer);

            IWebElement EDumpDepartureDate = driver.FindElement(By.Id(DumpLegDepartureDateID));
            EDumpDepartureDate.SendKeys(DumpLegDepartureDate);
            EDumpDepartureDate.SendKeys(Keys.Tab);

            Thread.Sleep(SleepTimer);

            Console.WriteLine("Populated Dump Leg");

            IWebElement EFinalSearchButton = driver.FindElement(By.XPath(SearchButtonXpath));
            EFinalSearchButton.Click();

            Console.WriteLine("Searching For Flights...");
        }
    }
}