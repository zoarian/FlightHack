﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;
using System.Collections.Generic;

namespace FlightHack
{
    class Program
    {
        static void Main(string[] args)
        {
            int SleepTimer = 100;
            int InitialNoOfAirports = 0;
            int NoOfCarriersThreshhold = 10;
            double AvgDistBtwAirports = 8406.2; 
            double OriginalFare = 408.60;
            double NewFare;
            string AirortFileLocation = "airports.json";
            string URL = "https://matrix.itasoftware.com/search";
            List<Airport> Airports = Airport.ProcessFile(AirortFileLocation);
            List<Tuple<Airport, Airport>> DumpConnections = new List<Tuple<Airport, Airport>>();

            //Console.WriteLine("Calculating AVG Distance");
            //double AvgDistance = Airport.AverageDistanceBetweenAllAirports(Airports);
            //Console.WriteLine("Average Distance Between Airports Is: " + AvgDistance);

            InitialNoOfAirports = Airports.Count;

            Console.WriteLine("Initial No Of Airports: " + InitialNoOfAirports);

            // We need to prune our airport list so we don't spend years searching
            // Start by removing small and unpopular ones first, since the likelihood
            // Of them habing a flight is small anyway

            int EligableAirports = 0;

            for(int i = Airports.Count-1; i > 0; i--)
            {
                if( Int32.Parse(Airports[i].Carriers) > NoOfCarriersThreshhold)
                {
                    //Console.WriteLine(i.ToString() + " " + Airports[i].Code);
                    EligableAirports++;
                }
                else
                {
                    Airports.RemoveAt(i);
                }
            }

            Console.WriteLine(EligableAirports + " Are Eligible Out Of: " + InitialNoOfAirports + " Based On Number Of Carriers Pruning");

            // Now go through each pair and check the distance.
            // Remove the airports that are too far from each other.
            // You only need to do distance comparison once (though it shouldn't take too long anyway).

            DumpConnections = Airport.PruneDumpConnections(Airports, AvgDistBtwAirports+40000);

            Console.WriteLine("We have: " + DumpConnections.Count + " Dump Connections, based on distance pruning");

            for(int i = 0; i < DumpConnections.Count; i++)
            {
                Console.WriteLine("Doing Dump Leg " + i + ". Connection: " + DumpConnections[i].Item1.Code + " -> " + DumpConnections[i].Item2.Code);

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
                string DumpLegOriginCityCode = DumpConnections[i].Item1.Code;
                string DumpLegDestinationCityCode = DumpConnections[i].Item2.Code;
                string DumpLegDepartureDate = "11/08/2022";

                string AdvancedButtonID = "/html/body/app-root/matrix-search-page/mat-card[1]/mat-card-content/form/matrix-select-flight-tabs/button";
                string AirlineInputID1 = "mat-input-20";
                string AirlineInputID2 = "mat-input-23";

                string AirlineInput = "AIRLINES AT";

                string Currency = "British Pound (GBP)";
                string CurrencyID = "mat-input-6";

                string QueryResultXPath = "/html/body/app-root/matrix-flights-page/mat-card/mat-card-content/mat-tab-group/div/mat-tab-body[1]/div/div/matrix-result-set-panel/div/div/table/tbody/tr[1]/td[1]/div/button/span[1]";

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

                WebDriverWait w = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
                w.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(QueryResultXPath)));

                IWebElement ENewPrice = driver.FindElement(By.XPath(QueryResultXPath));

                double.TryParse(ENewPrice.Text.Trim('£'), out NewFare);

                Console.WriteLine("Price with dump leg: " + DumpLegOriginCityCode + "->" + DumpLegDestinationCityCode + " is: " + ENewPrice.Text);

                if (NewFare < OriginalFare)
                    Console.WriteLine("We've got a cheaper fare: " + NewFare);
                else
                    Console.WriteLine("No Luck: " + NewFare);
            }
        }
    }
}