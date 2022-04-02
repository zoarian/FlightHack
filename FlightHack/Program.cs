using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using java.lang.management;

namespace FlightHack
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int SleepTimer = 10;
            int MaxSearchTimeLimit = 40;
            int InitialNoOfAirports = 0;
            int NoOfCarriersThreshhold = 10;
            double AvgDistBtwAirports = 8406.2;
            double OriginalFare = 408.60;
            string AirortFileLocation = "airports.json";
            string URL = "https://matrix.itasoftware.com/search";

            string ResultsFileBaseName = "_Results.csv";
            string ResultsFileFullPath = @"C:\Users\MP\Documents\FlightHack\";

            List<Airport> Airports = Airport.ProcessFile(AirortFileLocation);
            List<Tuple<Airport, Airport>> DumpConnections = new List<Tuple<Airport, Airport>>();
            List<QueryResult> Results = new List<QueryResult>();
            ItaMatrixHandler MatrixClient = new ItaMatrixHandler(SleepTimer, MaxSearchTimeLimit, URL);

            //Console.WriteLine("Calculating AVG Distance");
            //double AvgDistance = Airport.AverageDistanceBetweenAllAirports(Airports);
            //Console.WriteLine("Average Distance Between Airports Is: " + AvgDistance);

            InitialNoOfAirports = Airports.Count;

            Console.WriteLine("Initial No Of Airports: " + InitialNoOfAirports);

            // We need to prune our airport list so we don't spend years searching
            // Start by removing small and unpopular ones first, since the likelihood
            // Of them habing a flight is small anyway

            int EligableAirports = 0;

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

            // Now go through each pair and check the distance.
            // Remove the airports that are too far from each other.
            // You only need to do distance comparison once (though it shouldn't take too long anyway).

            //DumpConnections = Airport.PruneDumpConnections(Airports, AvgDistBtwAirports-6000, AvgDistBtwAirports-6005);

            DumpConnections = Airport.PruneDumpConnections(Airports, 65, 60);

            Console.WriteLine("We have: " + DumpConnections.Count + " Dump Connections, based on distance pruning");

            // TODO: Run these asynchronously - see below for psudocode
            for (int i = 0; i < DumpConnections.Count; i++)
            {

                //create and start tasks, then add them to the list
                //tasks.Add(Task.Run(() => Results.Add(MatrixClient.IssueAQuery(DumpConnections, OriginalFare, DumpConnections[i].Item1.Code, DumpConnections[i].Item2.Code, i))));


                Results.Add(MatrixClient.IssueAQuery(DumpConnections, OriginalFare, DumpConnections[i].Item1.Code, DumpConnections[i].Item2.Code, i));
            }

            // TODO: Running queries asynchronously
            /*            List<Task> TaskList = new List<Task>();
                        foreach (...)
                        {
                            var LastTask = new Task(SomeFunction);
                            LastTask.Start();
                            TaskList.Add(LastTask);
                        }
                        Task.WaitAll(TaskList.ToArray());*/


            ResultsFileFullPath += DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ResultsFileBaseName;

            // Create the file, or overwrite if the file exists.
            using (FileStream fs = File.Create(ResultsFileFullPath))
            {
                byte[] info = new UTF8Encoding(true).GetBytes("This is some text in the file.");
                fs.Write(info, 0, info.Length);
            }

            StreamWriter writer = new StreamWriter(ResultsFileFullPath);
            var csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture);

            csvWriter.WriteHeader<QueryResult>();
            csvWriter.NextRecord(); // adds new line after header
            csvWriter.WriteRecords(Results);

            writer.Flush();

            // Save the results into a CSV file after we're done
        }
    }
}