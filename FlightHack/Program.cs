using System;
using System.Collections.Generic;
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
            int SleepTimer = 10;
            int MaxSearchTimeLimit = 40;

            // Airport & Pruning Details
            string AirortFileLocation = "airports.json";
            int MinNoOfCarriers = 10;
            int MinDistance = 200;
            int MaxDistance = 240;
            int BinSize = 6;

            // Search Parameters
            double OriginalFare = 408.60;
            string DumpLegDepartureDate = "11/12/2022";
            string ResultsFile;

            // Used for searches
            List<QueryResult> Results = new List<QueryResult>();
            List<Task> TaskList = new List<Task>();
            List<List<Tuple<Airport, Airport>>> ChunkedDumLegs = Airport.CompleteAirportPruning(AirortFileLocation, MinNoOfCarriers, MinDistance, MaxDistance, BinSize);
            ItaMatrixHandler MatrixClient = new ItaMatrixHandler(SleepTimer, MaxSearchTimeLimit, URL, OriginalFare);
            List<Tuple<Airport, Airport>> AllDumpLegs = Airport.GetAllDumpConnections(AirortFileLocation, MinNoOfCarriers, MinDistance, MaxDistance, BinSize);

            /*            List<Tuple<Airport, Airport>> AllDumpLegs = Airport.GetAllDumpConnections(AirortFileLocation, MinNoOfCarriers, MinDistance, MaxDistance, BinSize);
                        Parallel.ForEach(AllDumpLegs, new ParallelOptions { MaxDegreeOfParallelism = BinSize },
                        DumpLeg =>
                        {
                            // logic
                            var LastTask = new Task(() => MatrixClient.IssueAQueryAsync(DumpLeg.Item1, DumpLeg.Item2, DumpLegDepartureDate, OriginalFare, Results));
                            LastTask.Start();
                        });*/

            var allTasks = new List<Task>();
            var throttler = new SemaphoreSlim(initialCount: BinSize);

            foreach (var DumpLeg in AllDumpLegs)
            {
                // do an async wait until we can schedule again
                await throttler.WaitAsync();

                // using Task.Run(...) to run the lambda in its own parallel
                // flow on the threadpool
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

            // won't get here until all urls have been put into tasks
/*            await Task.WhenAll(allTasks);

            using (SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(BinSize))
            {
                List<Task> tasks = new List<Task>();
                foreach (var DumpLeg in AllDumpLegs)
                {
                    concurrencySemaphore.Wait();

                    var t = Task.Factory.StartNew(() => MatrixClient.IssueAQueryAsync(DumpLeg.Item1, DumpLeg.Item2, DumpLegDepartureDate, OriginalFare, Results));

                    tasks.Add(t);
                }

                Task.WaitAll(tasks.ToArray());
            }*/

            // Change this so that when we finigh the no results search, we add in another task to the list
/*            for (int i = 0; i < ChunkedDumLegs.Count; i++)
            {
                Console.WriteLine("Going Through Chunk: " + i);

                foreach (Tuple<Airport, Airport> DumpLeg in ChunkedDumLegs[i])
                {
                    var LastTask = new Task(() => MatrixClient.IssueAQueryAsync(DumpLeg.Item1, DumpLeg.Item2, DumpLegDepartureDate, OriginalFare, Results));
                    LastTask.Start();
                    TaskList.Add(LastTask);
                }

                Task.WaitAll(TaskList.ToArray());

                Thread.Sleep(SleepTimer);
            }*/

            Console.WriteLine("Completed Flight Searches - saving a file now");

            // Save results in a file
            ResultsFile = QueryResult.SaveResultsToFile("", Results);

            // Send file to discord
            Disc.SendResults(ResultsFile, MatrixClient, BinSize, MinDistance, MaxDistance, MinNoOfCarriers);

        }
    }
}