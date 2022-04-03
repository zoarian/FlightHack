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
            // ITA Matrix Client
            string URL = "https://matrix.itasoftware.com/search";
            int SleepTimer = 10;
            int MaxSearchTimeLimit = 40;

            // Airport & Pruning Details
            string AirortFileLocation = "airports.json";
            int MinNoOfCarriers = 10;
            int MinDistance = 40;
            int MaxDistance = 60;
            int BinSize = 5;

            double AvgDistBtwAirports = 8406.2;
            double OriginalFare = 408.60;
            string DumpLegDepartureDate = "11/12/2022";

            ItaMatrixHandler MatrixClient = new ItaMatrixHandler(SleepTimer, MaxSearchTimeLimit, URL);
            List<QueryResult> Results = new List<QueryResult>();
            List<Task> TaskList = new List<Task>();

            List<List<Tuple<Airport, Airport>>> ChunkedDumLegs = Airport.CompleteAirportPruning(AirortFileLocation, MinNoOfCarriers, MinDistance, MaxDistance, BinSize);

            for (int i = 0; i < ChunkedDumLegs.Count; i++)
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
            }

            //Console.WriteLine("Calculating AVG Distance");
            //double AvgDistance = Airport.AverageDistanceBetweenAllAirports(Airports);
            //Console.WriteLine("Average Distance Between Airports Is: " + AvgDistance);

            /*            InitialNoOfAirports = Airports.Count;

                        Console.WriteLine("Initial No Of Airports: " + InitialNoOfAirports);*/

            // We need to prune our airport list so we don't spend years searching
            // Start by removing small and unpopular ones first, since the likelihood
            // Of them habing a flight is small anyway

            /*            int EligableAirports = 0;

                        for (int i = Airports.Count - 1; i > 0; i--)
                        {
                            if (Int32.Parse(Airports[i].Carriers) > NoOfCarriersThreshhold)
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
            */
            // TODO: Don't hack this - do proper splits... precalculate maybe? 
            // Split the searches into bins of 10
/*            for (int i = MinDistance; i < MaxDistance; i += BinSize)
            {
                // Now go through each pair and check the distance.
                // Remove the airports that are too far from each other.
                // You only need to do distance comparison once (though it shouldn't take too long anyway).
                DumpConnections = Airport.PruneDumpConnections(Airports, i, i + BinSize);

                Console.WriteLine("We have: " + DumpConnections.Count + " Dump Connections, based on distance pruning");

                foreach (Tuple<Airport, Airport> DumpLeg in DumpConnections)
                {
                    var LastTask = new Task(() => MatrixClient.IssueAQueryAsync(DumpLeg.Item1, DumpLeg.Item2, DumpLegDepartureDate, OriginalFare, Results));
                    LastTask.Start();
                    TaskList.Add(LastTask);
                }

                Task.WaitAll(TaskList.ToArray());

                Thread.Sleep(SleepTimer);
            }*/

            Console.WriteLine("Completed Flight Searches - saving a file now");

            QueryResult.SaveResultsToFile("", Results);
        }
    }
}