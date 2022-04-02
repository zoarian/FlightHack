using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace FlightHack
{
    public class ItaMatrixHandler
    {
        // Search Variables
        double NewFare { get; set; }
        int SleepTimer { get; set; } 
        int MaxSearchTimeLimit { get; set; }
        string URL { get; set; }

        // Inital Search Form IDs and XPaths
        const string MultiCityTabID = "mat-tab-label-0-2";
        const string AddFlightButtonXPath = "/html/body/app-root/matrix-search-page/mat-card[1]/mat-card-content/form/matrix-select-flight-tabs/mat-tab-group/div/mat-tab-body[3]/div/matrix-multi-city-search-tab/div/div[2]/mat-chip";
        const string SearchButtonXpath = "/html/body/app-root/matrix-search-page/mat-card[1]/mat-card-content/form/div[2]/button";

        const string LegOneOriginCityCodeID = "mat-chip-list-input-4";
        const string LegOneDestinationCityCodeID = "mat-chip-list-input-5";
        const string LegOneDepartureDateID = "mat-input-8";

        const string LegTwoOriginCityCodeID = "mat-chip-list-input-6";
        const string LegTwoDestinationCityCodeID = "mat-chip-list-input-7";
        const string LegTwoDepartureDateID = "mat-input-10";

        const string DumpLegOriginCityCodeID = "mat-chip-list-input-8";
        const string DumpLegDestinationCityCodeID = "mat-chip-list-input-9";
        const string DumpLegDepartureDateID = "mat-input-11";

        const string AdvancedButtonID = "/html/body/app-root/matrix-search-page/mat-card[1]/mat-card-content/form/matrix-select-flight-tabs/button";
        const string AirlineInputID1 = "mat-input-20";
        const string AirlineInputID2 = "mat-input-23";

        const string CurrencyID = "mat-input-6";

        // Result Page IDs and Xpaths
        const string QueryResultXPath = "/html/body/app-root/matrix-flights-page/mat-card/mat-card-content/mat-tab-group/div/mat-tab-body[1]/div/div/matrix-result-set-panel/div/div/table/tbody/tr[1]/td[1]/div/button/span[1]";
        const string StrtNewSearchXPath = "/html/body/app-root/matrix-flights-page/mat-card/mat-card-content/mat-tab-group/div/mat-tab-body[1]/div/div/matrix-result-set-panel/div/matrix-no-flights-found/button";
        const string NoResults = "/html/body/app-root/matrix-flights-page/mat-card/mat-card-content/mat-tab-group/div/mat-tab-body[1]/div/div/matrix-result-set-panel/div/matrix-no-flights-found/div[1]";

        // Input data - this will change depending on the query and the fuel dump flights
        string AirlineInput { get ; set; }
        string Currency { get; set; }

        // 1st Leg Human Details
        string LegOneOriginCityCode { get; set; }
        string LegOneDestinationCityCode { get; set; }
        string LegOneDepartureDate { get; set; }

        // 2nd Leg Human Details
        string LegTwoOriginCityCode { get; set; }
        string LegTwoDestinationCityCode { get; set; }
        string LegTwoDepartureDate { get; set; }

        public ItaMatrixHandler(int SleepTimer, int MaxSearchTimeLimit, string URL)
        {
            this.SleepTimer = SleepTimer;
            this.MaxSearchTimeLimit = MaxSearchTimeLimit;
            this.URL = URL;

            AirlineInput = "AIRLINES AT";
            Currency = "British Pound (GBP)";

            LegOneOriginCityCode = "GVA;";
            LegOneDestinationCityCode = "JFK;";
            LegOneDepartureDate = "10/30/2022";

            // This is not necessarily true
            LegTwoOriginCityCode = LegOneDestinationCityCode;
            LegTwoDestinationCityCode = LegOneOriginCityCode;
            LegTwoDepartureDate = "11/06/2022";
        }

        public QueryResult IssueAQuery(List<Tuple<Airport, Airport>> DumpConnections, double OriginalFare, string AirportCode1, string DumpAirportCode2, int QueryID)
        {
            var watch = Stopwatch.StartNew();

            string QueryTime;
            double DistanceBetweenDumpAirports = 0;
            double NewFare = 0;

            DistanceBetweenDumpAirports = Airport.DistanceBetweenAirports(DumpConnections[QueryID].Item1, DumpConnections[QueryID].Item2);

            // Dump Leg Human Details
            string DumpLegOriginCityCode = AirportCode1;
            string DumpLegDestinationCityCode = DumpAirportCode2;
            string DumpLegDepartureDate = "11/08/2022";

            Console.WriteLine("Doing Dump Leg " + QueryID + ". Connection: " + AirportCode1 + " -> " + DumpAirportCode2);

            ChromeOptions options = new ChromeOptions();
            options.AddArgument("log-level=3");
            options.AddArgument("silent");
            options.AddArgument("no-sandbox");
            options.AddArgument("headless");
            options.AddArgument("disable-extensions");
            options.AddArgument("test-type");

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;

            Thread.Sleep(SleepTimer * 10);

            IWebDriver driver = new ChromeDriver(service, options);
            driver.Url = URL;

            Thread.Sleep(SleepTimer * 20);

            IWebElement EMultiButton = driver.FindElement(By.Id(MultiCityTabID));
            EMultiButton.Click();

            Console.WriteLine("Changed Tabs");

            IWebElement EAddFlights = driver.FindElement(By.XPath(AddFlightButtonXPath));
            EAddFlights.Click();

            Thread.Sleep(SleepTimer * 10);

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

            IWebElement EAdvancedSearchButton = driver.FindElement(By.XPath(AdvancedButtonID));
            EAdvancedSearchButton.Click();

            Thread.Sleep(SleepTimer);

            IWebElement EAirlingInput1 = driver.FindElement(By.Id(AirlineInputID1));
            EAirlingInput1.SendKeys(AirlineInput);
            EAirlingInput1.SendKeys(Keys.Tab);

            Thread.Sleep(SleepTimer);

            IWebElement EAirlingInput2 = driver.FindElement(By.Id(AirlineInputID2));
            EAirlingInput2.SendKeys(AirlineInput);
            EAirlingInput2.SendKeys(Keys.Tab);

            Thread.Sleep(SleepTimer);

            IWebElement ECurrencyInput = driver.FindElement(By.Id(CurrencyID));
            ECurrencyInput.SendKeys(Currency);
            ECurrencyInput.SendKeys(Keys.Enter);

            Thread.Sleep(SleepTimer);

            IWebElement EFinalSearchButton = driver.FindElement(By.XPath(SearchButtonXpath));
            EFinalSearchButton.Click();

            Console.WriteLine("Searching For Flights...");

            // TODO: Change this so we check if either NoResults or QueryResults are returned - asynch maybe?
            try
            {
                WebDriverWait w = new WebDriverWait(driver, TimeSpan.FromSeconds(MaxSearchTimeLimit+20));
                w.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(NoResults)));

                Console.WriteLine("Query didn't return any flights with this dump leg");
            }
            catch (Exception ex)
            {
                try
                {
                    Console.WriteLine("Error: " + ex.Message);
                    Console.WriteLine("New Search Button wasn't found, means we've got results");

                    WebDriverWait w = new WebDriverWait(driver, TimeSpan.FromSeconds(MaxSearchTimeLimit-20));
                    w.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(QueryResultXPath)));

                    IWebElement ENewPrice = driver.FindElement(By.XPath(QueryResultXPath));

                    double.TryParse(ENewPrice.Text.Trim('£'), out NewFare);

                    Console.WriteLine("Price with dump leg: " + DumpLegOriginCityCode + "->" + DumpLegDestinationCityCode + " is: " + ENewPrice.Text);

                    if (NewFare < OriginalFare)
                        Console.WriteLine("We've got a cheaper fare: " + NewFare);
                    else
                        Console.WriteLine("No Luck: " + NewFare);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }

            watch.Stop();
            TimeSpan elapsed = watch.Elapsed;
            QueryTime = elapsed.ToString(@"m\:ss");

            driver.Quit();

            QueryResult Temp = new QueryResult(QueryTime, NewFare, DistanceBetweenDumpAirports, DumpLegOriginCityCode, DumpLegDestinationCityCode, DumpLegDepartureDate, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            return Temp;
        }
    }
}
