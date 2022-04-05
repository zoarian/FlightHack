using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FlightHack
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Discord Client
            string WebhookURL = "https://discord.com/api/webhooks/959509111035289640/z3S1WJ7LVnI7TFgQGZdCrLHziUwPNkuUglxhmxRKegiP4XiCbd7WdeMMUiOzfmN0Kp_f";
            DiscordClient Disc = new DiscordClient(WebhookURL); 

            // ITA Matrix Client
            string URL = "https://matrix.itasoftware.com/search";
            string DumpLegRoutingCode = "N";
            int SleepTimer = 10;
            int SearchLimitNoResults = 70;
            int SearchLimitWithResults = 30;

            // Airport & Pruning Details
            string AirortFileLocation = "Input/airports.json";
            int MinNoOfCarriers = 20;
            int MinDistance = 1;
            int MaxDistance = 600;
            int BinSize = 8;

            // Search Parameters
            double OriginalFare = 252;
            string FirstLegRouting = "F:WS51 F:WS273";
            string SecondLegRouting = "F:WS16";
            string DumpLegDepartureDate = "08/26/2022";
            string ResultsFile;

            // Used for searches
            List<QueryResult> Results = new List<QueryResult>();
            List<Task> allTasks = new List<Task>();
            ItaMatrixHandler MatrixClient = new ItaMatrixHandler(SleepTimer, SearchLimitNoResults, SearchLimitWithResults, URL, OriginalFare, DumpLegRoutingCode, FirstLegRouting, SecondLegRouting);
            List<Tuple<Airport, Airport>> AllDumpLegs = Airport.GetAllDumpConnections(AirortFileLocation, MinNoOfCarriers, MinDistance, MaxDistance, BinSize);

            var throttler = new SemaphoreSlim(initialCount: BinSize);

            var watch = Stopwatch.StartNew();

            foreach (var DumpLeg in AllDumpLegs)
            {
                // do an async wait until we can schedule again
                await throttler.WaitAsync();

                // using Task.Run(...) to run the lambda in its own parallel flow on the threadpool
                allTasks.Add(
                    Task.Run(async () =>
                    {
                        try
                        {
                            MatrixClient.IssueAQueryAsync(DumpLeg.Item1, DumpLeg.Item2, DumpLegDepartureDate, OriginalFare, Results);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
            }

            watch.Stop();

            Console.WriteLine("Completed Flight Searches - saving a file now");

            // Save results in a file
            ResultsFile = QueryResult.SaveResultsToFile("", Results);

            // Send file to discord
            Disc.SendResults(ResultsFile, MatrixClient, BinSize, MinDistance, MaxDistance, MinNoOfCarriers, AllDumpLegs.Count, watch.Elapsed.ToString(@"m\:ss"));
        }

        private void StartUp()
        {
            // Read datatypes such as the airport codes
            // Initialize Clients
        }

        private void Shutdown()
        {

        }
    }
}