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
        public double OriginalFare { get; set; }
        public int SleepTimer { get; set; } 
        public int SearchLimitNoResults { get; set; }

        public int SearchLimitWithResults { get; set; }
        public string URL { get; set; }

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

        const string DumpLegDateFlexIDButton = "mat-select-32";
        const string PlusMinus2days = "/html/body/div[3]/div[2]/div/div/div/mat-option[5]/span";

        const string AdvancedButtonID = "/html/body/app-root/matrix-search-page/mat-card[1]/mat-card-content/form/matrix-select-flight-tabs/button";
        const string AirlineInputID1 = "mat-input-20";
        const string AirlineInputID2 = "mat-input-23";
        const string DumpLegRoutingID = "mat-input-25";

        const string CurrencyID = "mat-input-6";

        // Result Page IDs and Xpaths
        const string QueryResultXPath = "/html/body/app-root/matrix-flights-page/mat-card/mat-card-content/mat-tab-group/div/mat-tab-body[1]/div/div/matrix-result-set-panel/div/div/table/tbody/tr[1]/td[1]/div/button/span[1]";
        const string StrtNewSearchXPath = "/html/body/app-root/matrix-flights-page/mat-card/mat-card-content/mat-tab-group/div/mat-tab-body[1]/div/div/matrix-result-set-panel/div/matrix-no-flights-found/button";
        const string NoResults = "/html/body/app-root/matrix-flights-page/mat-card/mat-card-content/mat-tab-group/div/mat-tab-body[1]/div/div/matrix-result-set-panel/div/matrix-no-flights-found/div[1]";

        // Input data - this will change depending on the query and the fuel dump flights
        public string AirlineInput { get ; set; }
        public string Currency { get; set; }

        // 1st Leg Human Details
        public string LegOneOriginCityCode { get; set; }
        public string LegOneDestinationCityCode { get; set; }
        public string LegOneDepartureDate { get; set; }

        // 2nd Leg Human Details
        public string LegTwoOriginCityCode { get; set; }
        public string LegTwoDestinationCityCode { get; set; }
        public string LegTwoDepartureDate { get; set; }

        public string DumpLegRouting { get; set; }

        public ItaMatrixHandler(int SleepTimer, int MaxSearchTimeLimit, int SearchLimitWithResults, string URL, double OriginalFare, string DumpLegRouting)
        {
            this.SleepTimer = SleepTimer;
            this.SearchLimitNoResults = MaxSearchTimeLimit;
            this.SearchLimitWithResults = SearchLimitWithResults;
            this.URL = URL;
            this.OriginalFare = OriginalFare;
            this.DumpLegRouting = DumpLegRouting;

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

        public void IssueAQueryAsync(Airport Airport1, Airport Airport2, string DumpLegDepartureDate, double OriginalFare, List<QueryResult> Results)
        {
            string QueryMessage;
            string QueryTime;
            double DistanceBetweenDumpAirports = Airport.DistanceBetweenAirports(Airport1, Airport2);
            double NewFare = 0;
            string DumpLegOriginCityCode = Airport1.Code;
            string DumpLegDestinationCityCode = Airport2.Code;

            var QueryTimer = Stopwatch.StartNew();
            int LoadingTimeout = SleepTimer * 20;

            try
            {
                Console.WriteLine("Doing Dump Leg Connection: " + Airport1.Code + " -> " + Airport2.Code);

                ChromeOptions   options = new ChromeOptions();
                                options.AddArgument("log-level=3");
                                options.AddArgument("silent");
                                options.AddArgument("no-sandbox");
                                options.AddArgument("ignore-certificate-errors");
                                options.AddArgument("ignore-ssl-errors");
                                options.AddArgument("headless");
                                options.AddArgument("disable-extensions");
                                options.AddArgument("test-type");
                                options.AddArgument("excludeSwitches");

                ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                                    service.SuppressInitialDiagnosticInformation = true;
                                    service.HideCommandPromptWindow = true;

                Console.WriteLine("Started Chrome Driver");

                Thread.Sleep(SleepTimer * 10);

                IWebDriver  driver = new ChromeDriver(service, options);
                            driver.Url = URL;

                WebDriverWait w = new WebDriverWait(driver, TimeSpan.FromMilliseconds(LoadingTimeout));
                              w.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.Id(MultiCityTabID)));
                IWebElement EMultiButton = driver.FindElement(By.Id(MultiCityTabID));
                EMultiButton.Click();

                Console.WriteLine("Changed Tabs");

                IWebElement EAddFlights = driver.FindElement(By.XPath(AddFlightButtonXPath));
                EAddFlights.Click();
                EAddFlights.Click();

                Console.WriteLine("Added Flight Fields");
                Thread.Sleep(SleepTimer);

                IWebElement ELeg1CityCode = driver.FindElement(By.Id(LegOneOriginCityCodeID));
                ELeg1CityCode.SendKeys(LegOneOriginCityCode);
                ELeg1CityCode.SendKeys(Keys.Tab);

                IWebElement ELeg1DepCode = driver.FindElement(By.Id(LegOneDestinationCityCodeID));
                ELeg1DepCode.SendKeys(LegOneDestinationCityCode);
                ELeg1DepCode.SendKeys(Keys.Tab);

                IWebElement ELeg1DepartureDate = driver.FindElement(By.Id(LegOneDepartureDateID));
                ELeg1DepartureDate.SendKeys(LegOneDepartureDate);
                ELeg1DepartureDate.SendKeys(Keys.Tab);

                Console.WriteLine("Populated First Leg");
                Thread.Sleep(SleepTimer);

                IWebElement ELeg2CityCode = driver.FindElement(By.Id(LegTwoOriginCityCodeID));
                ELeg2CityCode.SendKeys(LegTwoOriginCityCode);
                ELeg2CityCode.SendKeys(Keys.Tab);

                IWebElement ELeg2DepCode = driver.FindElement(By.Id(LegTwoDestinationCityCodeID));
                ELeg2DepCode.SendKeys(LegTwoDestinationCityCode);
                ELeg2DepCode.SendKeys(Keys.Tab);

                IWebElement ELeg2DepartureDate = driver.FindElement(By.Id(LegTwoDepartureDateID));
                ELeg2DepartureDate.SendKeys(LegTwoDepartureDate);
                ELeg2DepartureDate.SendKeys(Keys.Tab);

                Console.WriteLine("Populated Second Leg");
                Thread.Sleep(SleepTimer);

                IWebElement EDumpOriginCityCode = driver.FindElement(By.Id(DumpLegOriginCityCodeID));
                EDumpOriginCityCode.SendKeys(DumpLegOriginCityCode);
                EDumpOriginCityCode.SendKeys(Keys.Tab);

                IWebElement EDumpDepCode = driver.FindElement(By.Id(DumpLegDestinationCityCodeID));
                EDumpDepCode.SendKeys(DumpLegDestinationCityCode);
                EDumpDepCode.SendKeys(Keys.Tab);

                IWebElement EDumpDepartureDate = driver.FindElement(By.Id(DumpLegDepartureDateID));
                EDumpDepartureDate.SendKeys(DumpLegDepartureDate);
                EDumpDepartureDate.SendKeys(Keys.Tab);

                Console.WriteLine("Populated Dump Leg");
                Thread.Sleep(SleepTimer);

                IWebElement EAdvancedSearchButton = driver.FindElement(By.XPath(AdvancedButtonID));
                EAdvancedSearchButton.Click();

                w = new WebDriverWait(driver, TimeSpan.FromMilliseconds(LoadingTimeout));
                w.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.Id(AirlineInputID1)));

                IWebElement EAirlingInput1 = driver.FindElement(By.Id(AirlineInputID1));
                EAirlingInput1.SendKeys(AirlineInput);
                EAirlingInput1.SendKeys(Keys.Tab);

                IWebElement EAirlingInput2 = driver.FindElement(By.Id(AirlineInputID2));
                EAirlingInput2.SendKeys(AirlineInput);
                EAirlingInput2.SendKeys(Keys.Tab);

                // Add Dump Leg Flexibility
                Console.WriteLine("Adding Other Settings (Flex Date, Currency, etc)");
                Thread.Sleep(SleepTimer);

                IWebElement EFlexIDButton = driver.FindElement(By.Id(DumpLegDateFlexIDButton));
                EFlexIDButton.Click();

                w = new WebDriverWait(driver, TimeSpan.FromMilliseconds(LoadingTimeout));
                w.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(PlusMinus2days)));
                IWebElement EDumpDateFlex = driver.FindElement(By.XPath(PlusMinus2days));
                EDumpDateFlex.Click();

                IWebElement ECurrencyInput = driver.FindElement(By.Id(CurrencyID));
                ECurrencyInput.SendKeys(Currency);
                ECurrencyInput.SendKeys(Keys.Enter);

                IWebElement EDumpRoutingID = driver.FindElement(By.Id(DumpLegRoutingID));
                ECurrencyInput.SendKeys(DumpLegRouting);
                ECurrencyInput.SendKeys(Keys.Enter);

                IWebElement EFinalSearchButton = driver.FindElement(By.XPath(SearchButtonXpath));
                EFinalSearchButton.Click();

                Console.WriteLine("Searching For Flights...");

                // TODO: Change this so we check if either NoResults or QueryResults are returned - asynch maybe?
                try
                {
                    w = new WebDriverWait(driver, TimeSpan.FromSeconds(SearchLimitNoResults));
                    w.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(NoResults)));

                    Console.WriteLine("No flights match the criteria");
                    QueryMessage = "No flights match the criteria";
                }
                catch (Exception ex)
                {
                    try
                    {
                        Console.WriteLine("Error: " + ex.Message);
                        Console.WriteLine("We've not found a new search button - searching for prices now");
                        
                        w = new WebDriverWait(driver, TimeSpan.FromSeconds(SearchLimitWithResults));
                        w.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(QueryResultXPath)));

                        IWebElement ENewPrice = driver.FindElement(By.XPath(QueryResultXPath));

                        double.TryParse(ENewPrice.Text.Trim('£'), out NewFare);

                        Console.WriteLine("Price with dump leg: " + DumpLegOriginCityCode + "->" + DumpLegDestinationCityCode + " is: " + ENewPrice.Text);

                        if (NewFare < OriginalFare)
                            Console.WriteLine("We've got a cheaper fare: " + NewFare);
                        else
                            Console.WriteLine("No Luck: " + NewFare);

                        QueryMessage = "We've not found a new search button - searching for prices now";

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error: " + e.Message);
                        QueryMessage = e.Message;
                    }
                }

                driver.Quit();

           }
            catch(Exception e)
            {
                Console.WriteLine("Exception in Overall Query: " + e.Message);
                QueryMessage = e.Message;
                NewFare = -1;
            }

            QueryTimer.Stop();
            TimeSpan elapsed = QueryTimer.Elapsed;
            QueryTime = elapsed.ToString(@"m\:ss");

            Results.Add(new QueryResult(QueryTime, NewFare, DistanceBetweenDumpAirports, DumpLegOriginCityCode, DumpLegDestinationCityCode, DumpLegDepartureDate, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), QueryMessage, (Int32.Parse(Airport1.Carriers) + Int32.Parse(Airport2.Carriers))));

        }
    }
}
