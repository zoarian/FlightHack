using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FlightHack.Query;
using Newtonsoft.Json;

namespace FlightHack
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string ResultsFile;
            string AirortFileLocation = "Input/Airports.json";
            string ItaMatrixFile = "Input/ItaMatricClient.json";
            string DiscordWebhookURL = "https://discord.com/api/webhooks/959509111035289640/z3S1WJ7LVnI7TFgQGZdCrLHziUwPNkuUglxhmxRKegiP4XiCbd7WdeMMUiOzfmN0Kp_f";

            // Client Initialization
            List<Result> Results = new List<Result>();

            DiscordClient Disc = new DiscordClient(DiscordWebhookURL);
            ItaMatrixHandler MatrixClient = new ItaMatrixHandler(ItaMatrixFile);

            StreamReader r = new StreamReader(MatrixClient.JsonFileLocation);
            Input Input = JsonConvert.DeserializeObject<Input>(r.ReadToEnd());
            int JobTimeTaken = 0;

            JobTimeTaken = await MatrixClient.StartJobAsync(Input, Results, AirortFileLocation);


            /*            List<Task> allTasks = new List<Task>();
                        List<Tuple<Airport, Airport>> AllDumpLegs = Airport.GetAllDumpConnections(AirortFileLocation, Input.Airport.MinNoCarriers, Input.Airport.MinDist, Input.Airport.MaxDist);

                        var throttler = new SemaphoreSlim(initialCount: MatrixClient.NoOfParallelSearches);

                        int JobTimeTaken = 0;

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
                                        //Console.WriteLine(DumpLeg.Item1.Code + " -> " + DumpLeg.Item2.Code);
                                        MatrixClient.IssueQueryAsync(DumpLeg, Input, Results);
                                    }
                                    finally
                                    {
                                        throttler.Release();
                                    }
                                }));
                        }

                        watch.Stop();

                        MatrixClient.KillChromeDrivers();

                        JobTimeTaken = (int)watch.Elapsed.TotalSeconds;*/

            ResultsFile = Result.SaveResultsToFile("", Results);
            int NoOFQueries = 100;

            // Send file to discord
            Disc.SendResults(ResultsFile, Input, MatrixClient, NoOFQueries, JobTimeTaken.ToString());

            Environment.Exit(0);
        }

        private void StartUp()
        {
            // Read datatypes such as the airport codes
            // Initialize clients
        }

        private void Shutdown()
        {
            // Kill any remaining chromedrivers
        }
    }
}