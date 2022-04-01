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
            string URL = "https://oldmatrix.itasoftware.com/";

            string ClearCitySuggestion = "javascript:document.getElementsByClassName('gwt-SuggestBoxPopup')[0].setAttribute('hidden','')";
            string ClearCalendarSuggestion = "javascript: document.getElementsByClassName('dateBoxPopup')[0].setAttribute('hidden', '')";

            string MultiCityTabID = "/html/body/div[1]/div/div/div/div/div/div[2]/div[1]/div/table/tbody/tr/td[1]/div/div/table/tbody/tr[1]/td/table/tbody/tr/td[4]/div";
            string AddFlightButtonXPath = "/html/body/div[1]/div/div/div/div/div/div[2]/div[1]/div/table/tbody/tr/td[1]/div/div/table/tbody/tr[2]/td/div/div[3]/div/div[4]/div[2]/a";
            string SearchButtonXpath = "/html/body/div[1]/div/div/div/div/div/div[2]/div[1]/div/table/tbody/tr/td[1]/div/div/div/button";

            string LegOneOriginCityCodeID = "/html/body/div[1]/div/div/div/div/div/div[2]/div[1]/div/table/tbody/tr/td[1]/div/div/table/tbody/tr[2]/td/div/div[3]/div/div[2]/div[1]/div[2]/div/div/div/input";
            string LegOneDestinationCityCodeID = "/html/body/div[1]/div/div/div/div/div/div[2]/div[1]/div/table/tbody/tr/td[1]/div/div/table/tbody/tr[2]/td/div/div[3]/div/div[2]/div[1]/div[4]/div/div/div/input";
            string LegOneDepartureDateID = "/html/body/div[1]/div/div/div/div/div/div[2]/div[1]/div/table/tbody/tr/td[1]/div/div/table/tbody/tr[2]/td/div/div[3]/div/div[2]/div[1]/div[5]/div[1]/div[2]/input";

            string LegTwoOriginCityCodeID = "/html/body/div[1]/div/div/div/div/div/div[2]/div[1]/div/table/tbody/tr/td[1]/div/div/table/tbody/tr[2]/td/div/div[3]/div/div[2]/div[2]/div[2]/div/div/div/input";
            string LegTwoDestinationCityCodeID = "/html/body/div[1]/div/div/div/div/div/div[2]/div[1]/div/table/tbody/tr/td[1]/div/div/table/tbody/tr[2]/td/div/div[3]/div/div[2]/div[2]/div[4]/div/div/div/input";
            string LegTwoDepartureDateID = "/html/body/div[1]/div/div/div/div/div/div[2]/div[1]/div/table/tbody/tr/td[1]/div/div/table/tbody/tr[2]/td/div/div[3]/div/div[2]/div[2]/div[5]/div[1]/div[2]/input";

            string DumpLegOriginCityCodeID = "/html/body/div[1]/div/div/div/div/div/div[2]/div[1]/div/table/tbody/tr/td[1]/div/div/table/tbody/tr[2]/td/div/div[3]/div/div[2]/div[3]/div[2]/div/div/div/input";
            string DumpLegDestinationCityCodeID = "/html/body/div[1]/div/div/div/div/div/div[2]/div[1]/div/table/tbody/tr/td[1]/div/div/table/tbody/tr[2]/td/div/div[3]/div/div[2]/div[3]/div[4]/div/div/div/input";
            string DumpLegDepartureDateID = "/html/body/div[1]/div/div/div/div/div/div[2]/div[1]/div/table/tbody/tr/td[1]/div/div/table/tbody/tr[2]/td/div/div[3]/div/div[2]/div[3]/div[5]/div[1]/div[2]/input";

            // 1st Leg Human Details
            string LegOneOriginCityCode = "GVA";
            string LegOneDestinationCityCode = "JFK";
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

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            IJavaScriptExecutor js1 = (IJavaScriptExecutor)driver;

            Thread.Sleep(SleepTimer*20);

            IWebElement EMultiButton = driver.FindElement(By.XPath(MultiCityTabID));
            EMultiButton.Click();

            Console.WriteLine("Changed Tabs");

            Thread.Sleep(SleepTimer * 10);

            /*            IWebElement EAddFlights = driver.FindElement(By.XPath(AddFlightButtonXPath));
                        EAddFlights.Click();*/

            Console.WriteLine("Added Flight Fields");

            IWebElement ELeg1CityCode = driver.FindElement(By.XPath(LegOneOriginCityCodeID));
            ELeg1CityCode.SendKeys(LegOneOriginCityCode);
            ELeg1CityCode.SendKeys(Keys.Enter);

            Thread.Sleep(SleepTimer * 10);

            js.ExecuteScript(ClearCitySuggestion);

            Thread.Sleep(SleepTimer * 5);

            IWebElement ELeg1DepCode = driver.FindElement(By.XPath(LegOneDestinationCityCodeID));
            ELeg1DepCode.SendKeys(LegOneDestinationCityCode);
            ELeg1DepCode.SendKeys(Keys.Enter);

            Thread.Sleep(SleepTimer * 5);

            js1.ExecuteScript(ClearCitySuggestion);

            Thread.Sleep(SleepTimer * 5);

            IWebElement ELeg1DepartureDate = driver.FindElement(By.XPath(LegOneDepartureDateID));
            ELeg1DepartureDate.SendKeys(LegOneDepartureDate);
            ELeg1DepartureDate.SendKeys(Keys.Enter);

            Thread.Sleep(SleepTimer * 20);

            //js1.ExecuteScript(ClearCalendarSuggestion);

            Console.WriteLine("Populated First Leg");

            IWebElement ELeg2CityCode = driver.FindElement(By.XPath(LegTwoOriginCityCodeID));
            ELeg2CityCode.SendKeys(LegTwoOriginCityCode);
            ELeg2CityCode.SendKeys(Keys.Enter);

            Thread.Sleep(SleepTimer * 5);

            js.ExecuteScript(ClearCitySuggestion);

            Thread.Sleep(SleepTimer * 5);

            IWebElement ELeg2DepCode = driver.FindElement(By.XPath(LegTwoDestinationCityCodeID));
            ELeg2DepCode.SendKeys(LegTwoDestinationCityCode);
            ELeg2DepCode.SendKeys(Keys.Enter);

            Thread.Sleep(SleepTimer * 5);

            js.ExecuteScript(ClearCitySuggestion);

            Thread.Sleep(SleepTimer * 5);

            Thread.Sleep(SleepTimer);

            IWebElement ELeg2DepartureDate = driver.FindElement(By.XPath(LegTwoDepartureDateID));
            ELeg2DepartureDate.SendKeys(LegTwoDepartureDate);
            ELeg2DepartureDate.SendKeys(Keys.Enter);

            Console.WriteLine("Populated Second Leg");

/*            IWebElement EDumpOriginCityCode = driver.FindElement(By.XPath(DumpLegOriginCityCodeID));
            EDumpOriginCityCode.SendKeys(DumpLegOriginCityCode);
            EDumpOriginCityCode.SendKeys(Keys.Tab);

            Thread.Sleep(SleepTimer);

            IWebElement EDumpDepCode = driver.FindElement(By.XPath(DumpLegDestinationCityCodeID));
            EDumpDepCode.SendKeys(DumpLegDestinationCityCode);
            EDumpDepCode.SendKeys(Keys.Tab);
            
            Thread.Sleep(SleepTimer);

            IWebElement EDumpDepartureDate = driver.FindElement(By.XPath(DumpLegDepartureDateID));
            EDumpDepartureDate.SendKeys(DumpLegDepartureDate);
            EDumpDepartureDate.SendKeys(Keys.Tab);

            Thread.Sleep(SleepTimer);

            Console.WriteLine("Populated Dump Leg");*/

            IWebElement EFinalSearchButton = driver.FindElement(By.XPath(SearchButtonXpath));
            EFinalSearchButton.Click();

            Console.WriteLine("Searching For Flights...");
        }
    }
}