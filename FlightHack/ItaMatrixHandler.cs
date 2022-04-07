using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using FlightHack.Query;
using System.IO;
using System.Threading.Tasks;
using log4net;

namespace FlightHack
{
    public class ItaMatrixHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));

        [JsonProperty("Browser")]
        public string Browser { get; set; }

        [JsonProperty("IsTakingResultScreenshots")]
        public bool IsTakingResultScreenshots { get; set; }

        [JsonProperty("IsUsingJobTimer")]
        public bool IsUsingJobTimer { get; set; }

        [JsonProperty("IsUsingQueryTimers")]
        public bool IsUsingQueryTimers { get; set; }

        [JsonProperty("JsonFileLocation")]
        public string JsonFileLocation { get; set; }

        [JsonProperty("NoOfParallelSearches")]
        public int NoOfParallelSearches { get; set; }

        [JsonProperty("SearchLimitNoResults")]
        public int SearchLimitNoResults { get; set; }

        [JsonProperty("SearchLimitWithResults")]
        public int SearchLimitWithResults { get; set; }

        [JsonProperty("SleepTimer")]
        public int SleepTimer { get; set; }

        [JsonProperty("WebElementTimeout")]
        public int WebElementTimeout { get; set; }

        [JsonProperty("URL")]
        public string URL { get; set; }

        // This should really go into the Job Class
        public static int LoopNo { get; set; }

        // Inital Search Form IDs and XPaths
        const string MultiCityTabID = "mat-tab-label-0-2";
        const string AddFlightButtonXPath = "/html/body/app-root/matrix-search-page/mat-card[1]/mat-card-content/form/matrix-select-flight-tabs/mat-tab-group/div/mat-tab-body[3]/div/matrix-multi-city-search-tab/div/div[2]/mat-chip";
        const string SearchButtonXpath = "/html/body/app-root/matrix-search-page/mat-card[1]/mat-card-content/form/div[2]/button";
        const string AdvancedButtonID = "/html/body/app-root/matrix-search-page/mat-card[1]/mat-card-content/form/matrix-select-flight-tabs/button";

        // Result Page IDs and Xpaths
        const string QueryResultXPath = "/html/body/app-root/matrix-flights-page/mat-card/mat-card-content/mat-tab-group/div/mat-tab-body[1]/div/div/matrix-result-set-panel/div/div/table/tbody/tr[1]/td[1]/div/button/span[1]";
        const string NoResults = "/html/body/app-root/matrix-flights-page/mat-card/mat-card-content/mat-tab-group/div/mat-tab-body[1]/div/div/matrix-result-set-panel/div/matrix-no-flights-found/div[1]";

        //
        const string CurrencyID = "mat-input-6";
        const string DumpLegDateFlexIDButton = "mat-select-32";
        const string PlusMinus2days = "/html/body/div[3]/div[2]/div/div/div/mat-option[5]/span";

        // Add this to the query input
        public double OriginalFare { get; set; }

        public ItaMatrixHandler() { }

        public ItaMatrixHandler(string FilePath)
        {
            StreamReader r = new StreamReader(FilePath);
            ItaMatrixHandler temp = JsonConvert.DeserializeObject<ItaMatrixHandler>(r.ReadToEnd());

            // Assign general properties
            this.Browser = temp.Browser;
            this.IsTakingResultScreenshots = temp.IsTakingResultScreenshots;
            this.IsUsingJobTimer = temp.IsUsingJobTimer;
            this.IsUsingQueryTimers = temp.IsUsingQueryTimers;
            this.JsonFileLocation = temp.JsonFileLocation;
            this.NoOfParallelSearches = temp.NoOfParallelSearches;
            this.SearchLimitNoResults = temp.SearchLimitNoResults;
            this.SearchLimitWithResults = temp.SearchLimitWithResults;
            this.SleepTimer = temp.SleepTimer;
            this.WebElementTimeout = temp.WebElementTimeout;
            this.URL = temp.URL;
        }

        public async Task<int> StartJobAsync(Input Input, List<Result> Results, string AirortFileLocation)
        {
            List<Task> allTasks = new List<Task>();
            List<Tuple<Airport, Airport>> AllDumpLegs = Airport.GetAllDumpConnections(AirortFileLocation, Input.Airport.MinNoCarriers, Input.Airport.MinDist, Input.Airport.MaxDist);
            List<Input> InputList = new List<Input>();

            int JobTimeTaken = 0;
            
            LoopNo = 0;

            if(AllDumpLegs.Count < 1)
            {
                log.InfoFormat("No dump legs found, aborting the job");
            }
            else 
            {
                var watch = Stopwatch.StartNew();

                if (AllDumpLegs.Count == 1)
                {
                    log.InfoFormat("{0}/{1} : dump leg connection {2} -> {3}", ++LoopNo, AllDumpLegs.Count, AllDumpLegs[0].Item1.Code, AllDumpLegs[0].Item2.Code);

                    IssueQueryAsync(AllDumpLegs[0], Input, Results);
                }
                else
                {
                    

                    if (AllDumpLegs.Count < NoOfParallelSearches)
                    {
                        NoOfParallelSearches = AllDumpLegs.Count;

                        log.InfoFormat("Running {0} queries in parallel, lowered to the number of dump legs found", NoOfParallelSearches);
                    }
                    else
                    {
                        log.InfoFormat("Running {0} queries in parallel", NoOfParallelSearches);
                    }

                    var throttler = new SemaphoreSlim(initialCount: NoOfParallelSearches);

                    foreach (var DumpLeg in AllDumpLegs)
                    {
                        await throttler.WaitAsync();

                        allTasks.Add(
                            Task.Run(() =>
                            {
                                try
                                {
                                    log.InfoFormat("{0}/{1} : dump leg connection {2} -> {3}", LoopNo, AllDumpLegs.Count, DumpLeg.Item1.Code, DumpLeg.Item2.Code);

                                    IssueQueryAsync(DumpLeg, Input, Results);
                                    Thread.Sleep(30);
                                }
                                finally
                                {
                                    throttler.Release();
                                }
                            }));
                    }

                    // Do the reverse search here
                    List<Tuple<Airport, Airport>> ReverseDumpLegs = new List<Tuple<Airport, Airport>>();

                    foreach (var DumpLeg in AllDumpLegs)
                    {
                        ReverseDumpLegs.Add(new Tuple<Airport, Airport>(DumpLeg.Item2, DumpLeg.Item1));
                    }
                }

                watch.Stop();

                JobTimeTaken = (int)watch.Elapsed.TotalSeconds;

                KillChromeDrivers();
            } 

            return JobTimeTaken;
        }

        public void KillChromeDrivers()
        {
            log.Info("Killing chrome drivers");
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "taskkill /F /IM chromedriver.exe /T";
            process.StartInfo = startInfo;
            process.Start();
        }

        public void IssueQueryAsync(Tuple<Airport, Airport> DumpConnection, Input Input, List<Result> Results)
        {
            LoopNo++;

            Result CurrentSearch = new Result();
            CurrentSearch.DistanceBetweenDumpAirports = Airport.DistanceBetweenAirports(DumpConnection.Item1, DumpConnection.Item2);
            CurrentSearch.NewFare = 0;
            CurrentSearch.TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            CurrentSearch.SumOfCarriers = (Int32.Parse(DumpConnection.Item1.Carriers) + Int32.Parse(DumpConnection.Item2.Carriers)).ToString();
            CurrentSearch.DumpLegOriginCode = DumpConnection.Item1.Code;
            CurrentSearch.DumpLegDestCode = DumpConnection.Item2.Code;

            var QueryTimer = Stopwatch.StartNew();

            try
            {
                ChromeOptions options = new ChromeOptions();
                options.AddArgument("log-level=3");
                options.AddArgument("silent");
                options.AddArgument("no-sandbox");
                options.AddArgument("ignore-certificate-errors");
                options.AddArgument("ignore-ssl-errors");
                options.AddArgument("headless");
                options.AddArgument("disable-extensions");
                options.AddArgument("test-type");
                options.AddArgument("excludeSwitches");
                options.AddArgument("start-maximized");
                options.AddArgument("disable-infobars");
                options.AddArgument("--disable-extensions");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-application-cache");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--disable-dev-shm-usage");

                ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                service.SuppressInitialDiagnosticInformation = true;
                service.HideCommandPromptWindow = true;

                IWebDriver Driver = new ChromeDriver(service, options, TimeSpan.FromMinutes(3));
                Driver.Url = URL;

                WebDriverWait w = new WebDriverWait(Driver, TimeSpan.FromMilliseconds(WebElementTimeout));
                w.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.Id(MultiCityTabID)));

                IWebElement EMultiButton = Driver.FindElement(By.Id(MultiCityTabID));
                EMultiButton.Click();

                IWebElement EAddFlights = Driver.FindElement(By.XPath(AddFlightButtonXPath));
                for (int i = 0; i < Input.FixedLegs.Count; i++) { EAddFlights.Click(); }

                Thread.Sleep(SleepTimer); // Might not be needed

                IWebElement EAdvancedSearchButton = Driver.FindElement(By.XPath(AdvancedButtonID));
                EAdvancedSearchButton.Click();

                Thread.Sleep(SleepTimer); // Might not be needed

                PupulateLegs(Driver, Input, DumpConnection);

                Thread.Sleep(SleepTimer);

                IWebElement ECurrencyInput = Driver.FindElement(By.Id(CurrencyID));
                ECurrencyInput.SendKeys(Input.General.Currency);
                ECurrencyInput.SendKeys(Keys.Enter);

                IWebElement EFinalSearchButton = Driver.FindElement(By.XPath(SearchButtonXpath));
                EFinalSearchButton.Click();

                // TODO: Change this so we check if either NoResults or QueryResults are returned - asynch maybe?
                try
                {
                    w = new WebDriverWait(Driver, TimeSpan.FromSeconds(SearchLimitNoResults));
                    w.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(NoResults)));
#if DEBUG
                    log.DebugFormat("No flights found for {0} -> {1}", DumpConnection.Item1.Code, DumpConnection.Item2.Code);
#endif
                }
                catch (Exception ex)
                {
                    try
                    {
#if DEBUG
                        log.DebugFormat("{0} -> {1} - searching for flight prices", DumpConnection.Item1.Code, DumpConnection.Item2.Code);
#endif

                        w = new WebDriverWait(Driver, TimeSpan.FromSeconds(SearchLimitWithResults));
                        w.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(QueryResultXPath)));

                        IWebElement ENewPrice = Driver.FindElement(By.XPath(QueryResultXPath));

                        string FoundResult;
                        CurrentSearch.NewFare = double.Parse(ENewPrice.Text.Trim('£'));

                        if (CurrentSearch.NewFare < OriginalFare)
                            FoundResult = "We've got a cheaper fare: " + CurrentSearch.NewFare + " compared to the original " + Input.General.OriginalFarePrice;
                        else
                            FoundResult = "New fare (" + CurrentSearch.NewFare + ") is more expensive than the original " + Input.General.OriginalFarePrice;

                        log.InfoFormat("{0} -> {1} dump leg found a fare. {2}", DumpConnection.Item1.Code, DumpConnection.Item2.Code, FoundResult);
                    }
                    catch (Exception e)
                    {
#if DEBUG
                        log.DebugFormat("{0} -> {1} reached timeout for fare finding. Trying to see if the original timeout (for no results) was too short", DumpConnection.Item1.Code, DumpConnection.Item2.Code);
#endif
                        try
                        {
                            w = new WebDriverWait(Driver, TimeSpan.FromSeconds(5));
                            w.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(NoResults)));

#if DEBUG
                            log.DebugFormat("{0} -> {1} - he original no result timeout was too short", DumpConnection.Item1.Code, DumpConnection.Item2.Code);
#endif
                            CurrentSearch.QueryMessage = "The original no result timeout was too short";
                        }
                        catch (Exception exc)
                        {
                            log.ErrorFormat("{0} -> {1} - the no results timeout wasn't too short. Error: {2}", DumpConnection.Item1.Code, DumpConnection.Item2.Code, exc.Message);
                            CurrentSearch.QueryMessage = exc.Message;
                        }
                    }
                }

                Driver.Quit();
            }
            catch (Exception e)
            {
                log.ErrorFormat("Exception in Overall Query: {0}", e.Message);
                CurrentSearch.QueryMessage = e.Message;
                CurrentSearch.NewFare = -1;
            }
            finally
            {
                //Driver.Quit();
            }

            Thread.Sleep(100);

            QueryTimer.Stop();
            TimeSpan elapsed = QueryTimer.Elapsed;
            CurrentSearch.QueryTime = ((int)elapsed.TotalSeconds).ToString();

            Results.Add(CurrentSearch);
        }

        public void PupulateLegs(IWebDriver Driver, Input Input, Tuple<Airport, Airport> DumpConnection)
        {
            IWebElement WebEl;

            for (int LegNo = 0; LegNo < Input.FixedLegs.Count; LegNo++)
            {
                LegIDs Leg = new LegIDs(LegNo);

                WebEl = Driver.FindElement(By.Id(Leg.OriginCityID));
                WebEl.SendKeys(Input.FixedLegs[LegNo].OriginCity);
                WebEl.SendKeys(Keys.Tab);

                WebEl = Driver.FindElement(By.Id(Leg.DestinationCityID));
                WebEl.SendKeys(Input.FixedLegs[LegNo].DestinationCity);
                WebEl.SendKeys(Keys.Tab);

                WebEl = Driver.FindElement(By.Id(Leg.DateID));
                WebEl.SendKeys(Input.FixedLegs[LegNo].Date);
                WebEl.SendKeys(Keys.Tab);

                if (!String.IsNullOrEmpty(Input.FixedLegs[LegNo].RoutingCode))
                {
                    WebEl = Driver.FindElement(By.Id(Leg.RoutingCodeID));
                    WebEl.SendKeys(Input.FixedLegs[LegNo].RoutingCode);
                    WebEl.SendKeys(Keys.Tab);
                }

                if (!String.IsNullOrEmpty(Input.FixedLegs[LegNo].ExtensionCode))
                {
                    WebEl = Driver.FindElement(By.Id(Leg.ExtensionCodeID));
                    WebEl.SendKeys(Input.FixedLegs[LegNo].ExtensionCode);
                    WebEl.SendKeys(Keys.Tab);
                }

                if (!String.IsNullOrEmpty(Input.FixedLegs[LegNo].DateOption) && Input.FixedLegs[LegNo].DateOption != "Departure")
                {
                    WebEl = Driver.FindElement(By.Id(Leg.DateOptionID));
                    WebEl.SendKeys(Input.FixedLegs[LegNo].DateOption);
                    WebEl.SendKeys(Keys.Tab);
                }

                if (!String.IsNullOrEmpty(Input.FixedLegs[LegNo].DateFlexibility) && Input.FixedLegs[LegNo].DateFlexibility != "This day only")
                {
                    WebEl = Driver.FindElement(By.Id(Leg.DateFlexibilityID));
                    WebEl.SendKeys(Input.FixedLegs[LegNo].DateFlexibility);
                    WebEl.SendKeys(Keys.Tab);
                }
            }

            int LegN = Input.FixedLegs.Count;

            LegIDs DLeg = new LegIDs(LegN);

            WebEl = Driver.FindElement(By.Id(DLeg.OriginCityID));
            WebEl.SendKeys(DumpConnection.Item1.Code);
            WebEl.SendKeys(Keys.Tab);

            WebEl = Driver.FindElement(By.Id(DLeg.DestinationCityID));
            WebEl.SendKeys(DumpConnection.Item2.Code);
            WebEl.SendKeys(Keys.Tab);

            WebEl = Driver.FindElement(By.Id(DLeg.DateID));
            WebEl.SendKeys(Input.DumpLeg.Date);
            WebEl.SendKeys(Keys.Tab);

            if (!String.IsNullOrEmpty(Input.DumpLeg.RoutingCode))
            {
                WebEl = Driver.FindElement(By.Id(DLeg.RoutingCodeID));
                WebEl.SendKeys(Input.DumpLeg.RoutingCode);
                WebEl.SendKeys(Keys.Tab);
            }

            if (!String.IsNullOrEmpty(Input.DumpLeg.ExtensionCode))
            {
                WebEl = Driver.FindElement(By.Id(DLeg.ExtensionCodeID));
                WebEl.SendKeys(Input.DumpLeg.ExtensionCode);
                WebEl.SendKeys(Keys.Tab);
            }

            if (!String.IsNullOrEmpty(Input.DumpLeg.DateOption) && Input.DumpLeg.DateOption != "Departure")
            {
                WebEl = Driver.FindElement(By.Id(DLeg.DateOptionID));
                WebEl.SendKeys(Input.DumpLeg.DateOption);
                WebEl.SendKeys(Keys.Tab);
            }

            if (!String.IsNullOrEmpty(Input.DumpLeg.DateFlexibility) && Input.DumpLeg.DateFlexibility != "This day only")
            {
                // TODO: Fix this so it's actually dynamic and flexible
                IWebElement EFlexIDButton = Driver.FindElement(By.Id(DumpLegDateFlexIDButton));
                EFlexIDButton.Click();

                Thread.Sleep(SleepTimer);

                WebDriverWait w = new WebDriverWait(Driver, TimeSpan.FromMilliseconds(WebElementTimeout));
                w.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(PlusMinus2days)));
                IWebElement EDumpDateFlex = Driver.FindElement(By.XPath(PlusMinus2days));
                EDumpDateFlex.Click();

                /*                WebEl = Driver.FindElement(By.Id(DLeg.DateFlexibilityID));
                                WebEl.SendKeys(Input.DumpLeg.DateFlexibility);
                                WebEl.SendKeys(Keys.Tab);*/
            }
        }
    }

    public class LegIDs
    {
        public string OriginCityID { get; set; }
        public string DestinationCityID { get; set; }
        public string RoutingCodeID { get; set; }
        public string ExtensionCodeID { get; set; }
        public string DateID { get; set; }
        public string DateOptionID { get; set; }
        public string DateFlexibilityID { get; set; }

        private int OInit = 4;
        private int RInit = 22;
        private int DInit = 27;

        public LegIDs()
        {
            OriginCityID = "mat-chip-list-input-";
            DestinationCityID = "mat-chip-list-input-";
            RoutingCodeID = "mat-input-";
            ExtensionCodeID = "mat-input-";
            DateID = "mat-input-";
            DateOptionID = "mat-select-value-";
            DateFlexibilityID = "mat-select-value-";
        }

        public LegIDs(int LegNo)
        {
            OriginCityID = "mat-chip-list-input-" + (LegNo * 2 + OInit);
            DestinationCityID = "mat-chip-list-input-" + (LegNo * 2 + OInit + 1);

            if (LegNo == 0)
            {
                RoutingCodeID = "mat-input-19";
                ExtensionCodeID = "mat-input-20";
                DateID = "mat-input-21";
                DateOptionID = "mat-select-value-23";
                DateFlexibilityID = "mat-select-value-25";
            }
            else
            {
                RoutingCodeID = "mat-input-" + ((LegNo - 1) * 3 + RInit); // 22
                ExtensionCodeID = "mat-input-" + ((LegNo - 1) * 3 + RInit + 1); //23
                DateID = "mat-input-" + ((LegNo - 1) * 3 + RInit + 2); //24
                DateOptionID = "mat-select-value-" + ((LegNo - 1) * 4 + DInit); //27
                DateFlexibilityID = "mat-select-value-" + ((LegNo - 1) * 4 + DInit + 1); //28
            }
        }
    }
}
