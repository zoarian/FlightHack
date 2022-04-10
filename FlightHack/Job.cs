using FlightHack.Query;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using static FlightHack.Globals;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json;
using System.IO;

namespace FlightHack
{
    /// <summary>
    /// Contains the data and methods for a job. This includes:
    ///     ALL the timers
    ///     Input data needed for running queries
    ///     Result set (empty and full)
    /// </summary>
    public class Job
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Job));

        // To make life easier
        public int ID { get; set; }
        public string Name { get; set; }

        // Time data
        public int EstimatedProcessingTime { get; set; } // all these are in seconds
        public int EstimatedQueuingTime { get; set; }
        public int TotalProcessingTime { get; set; }
        public int TotalQueuingTime { get; set; }
        public DateTime Queued { get; set; }
        public DateTime Started { get; set; }
        public DateTime Completed { get; set; }

        // For processing
        public Status Status { get; set; }
        public int CurrentQueryNo { get; set; }
        public int TotalNoOfQueries { get; set; } // This is #DumpLegs x 2 (A->B, B->A)
        public int NoOfParallelSearches { get; set; }
        public string ResultsFileFullPath { get; set; }
        public Input Input { get; set; }
        public List<Task> AllTasks { get; set; }
        public List<Result> Results { get; set; }
        public List<Tuple<Airport, Airport>> AllDumpLegs { get; set; }

        public Job()
        {
            Status = Status.Initial;

            CurrentQueryNo = 0;
            TotalNoOfQueries = 0;

            Results = new List<Result>();
            AllTasks = new List<Task>();

            ID = 0;
            Name = "Initial Job";
        }
        public Job(int JobID, string NewFileLocation)
        {
            // Check if you can process the input for a job and put in queue
            StreamReader r = new StreamReader(NewFileLocation);
            Input Input = JsonConvert.DeserializeObject<Input>(r.ReadToEnd());

            this.ID = JobID;
            this.Name = Input.FixedLegs[0].OriginCity.Substring(0, 1) + Input.FixedLegs[0].DestinationCity.Substring(0, 1) + JobID;
            this.Input = Input;
            this.AllDumpLegs = Airport.GetAllDumpConnections(AppSettings["AirortDataFile"], Input.Airport.MinNoCarriers, Input.Airport.MinDist, Input.Airport.MaxDist);
        }
        public Job(int JobID, Input Input)
        {
            this.ID = JobID;
            this.Name = Input.FixedLegs[0].OriginCity.Substring(0, 1) + Input.FixedLegs[0].DestinationCity.Substring(0, 1) + JobID;
            Status = Status.Initial;
            this.Input = Input;
            this.AllDumpLegs = Airport.GetAllDumpConnections(AppSettings["AirortDataFile"], Input.Airport.MinNoCarriers, Input.Airport.MinDist, Input.Airport.MaxDist);
        }
        public Job(int JobID, Input Input, Status Status)
        {
            this.ID = JobID;
            this.Name = Input.FixedLegs[0].OriginCity.Substring(0, 1) + Input.FixedLegs[0].DestinationCity.Substring(0, 1) + JobID;
            this.Status = Status;
            this.Input = Input;
            this.AllDumpLegs = Airport.GetAllDumpConnections(AppSettings["AirortDataFile"], Input.Airport.MinNoCarriers, Input.Airport.MinDist, Input.Airport.MaxDist);
        }
        public Job(int JobID, Input Input, Status Status, List<Tuple<Airport, Airport>> AllDumpLegs)
        {
            // ID Parameters
            this.ID = JobID;
            this.Name = Input.FixedLegs[0].OriginCity.Substring(0, 1) + Input.FixedLegs[0].DestinationCity.Substring(0, 1) + JobID;

            // Processing
            this.Status = Status;
            this.CurrentQueryNo = 0;
            this.TotalNoOfQueries = AllDumpLegs.Count * 2;
            this.Input = Input;
            this.AllDumpLegs = AllDumpLegs;
            this.NoOfParallelSearches = Globals.MatrixClient.NoOfParallelSearches; // This should really be supplied per job

            // Timekeeping
            EstimatedProcessingTime = (int)Math.Round(TotalNoOfQueries * MatrixClient.AverageQueryTime / (NoOfParallelSearches*1.0));
            EstimatedQueuingTime = 0; // This needs to be fed from QueueManager -> EstimatedProcessingTime of every job ahead of this one
            TotalProcessingTime = 0;
            TotalQueuingTime = 0;
            Queued = DateTime.Now;
            Started = DateTime.Now;
            Completed = DateTime.Now;

            Results = new List<Result>();
            AllTasks = new List<Task>();
        }
        public Job(int JobID, Input Input, Status Status, List<Tuple<Airport, Airport>> AllDumpLegs, int EstimatedQueuingTime)
        {
            // ID Parameters
            this.ID = JobID;
            this.Name = Input.FixedLegs[0].OriginCity.Substring(0, 1) + Input.FixedLegs[0].DestinationCity.Substring(0, 1) + JobID;

            // Processing
            this.Status = Status;
            this.CurrentQueryNo = 0;
            this.TotalNoOfQueries = AllDumpLegs.Count * 2;
            this.Input = Input;
            this.AllDumpLegs = AllDumpLegs;
            this.NoOfParallelSearches = Globals.MatrixClient.NoOfParallelSearches; // This should really be supplied per job

            // Timekeeping
            this.EstimatedProcessingTime = (int)Math.Round(TotalNoOfQueries * MatrixClient.AverageQueryTime / (NoOfParallelSearches * 1.0));
            this.EstimatedQueuingTime = EstimatedQueuingTime;
            this.TotalProcessingTime = 0;
            this.TotalQueuingTime = 0;
            this.Queued = DateTime.Now;
            this.Started = DateTime.Now;
            this.Completed = DateTime.Now;

            this.Results = new List<Result>();
            this.AllTasks = new List<Task>();
        }
        /// <summary>
        /// Starts a job and returns a task that returns 
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="Results"></param>
        /// <param name="AirortFileLocation"></param>
        /// <returns></returns>
        public async Task StartJobAsync()
        {
            // Housekeeping
            Status = Status.InProgress;
            Started = DateTime.Now;
            TotalQueuingTime = (int)Math.Round(Started.Subtract(Queued).TotalSeconds);

            try
            {
                if (AllDumpLegs.Count < 1)
                {
                    log.InfoFormat("No dump legs found, aborting the job");
                }
                else
                {
                    var watch = Stopwatch.StartNew();

                    if (AllDumpLegs.Count == 1)
                    {
                        log.InfoFormat("{0}/{1} : dump leg connection {2} -> {3}", ++CurrentQueryNo, AllDumpLegs.Count, AllDumpLegs[0].Item1.Code, AllDumpLegs[0].Item2.Code);

                        MatrixClient.IssueQueryAsync(AllDumpLegs[0], Input, Results);
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

                            AllTasks.Add(
                                Task.Run(() =>
                                {
                                    try
                                    {
                                        log.InfoFormat("{0}/{1} : dump leg connection {2} -> {3}", CurrentQueryNo, AllDumpLegs.Count, DumpLeg.Item1.Code, DumpLeg.Item2.Code);

                                        MatrixClient.IssueQueryAsync(DumpLeg, Input, Results);
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

                    TotalProcessingTime = (int)watch.Elapsed.TotalSeconds;
                }
            }
            catch (Exception JobException)
            {
                log.ErrorFormat("Job {0}, {1} failed. Exception {2}", ID, Name, JobException.Message);
                Status = Status.Failed;
                // Dump the current job into json file
            }

            //return this;
        }

        public async Task CompleteJob()
        {
            MatrixClient.KillChromeDrivers();

            // Dump the results in a file (temp location?)
            ResultsFileFullPath = Result.SaveResultsToFile(Globals.AppSettings["QueryResultPath"], Results, Input);

            // Send over discord
            Globals.Disc.SendResults(ResultsFileFullPath, this);

            // Housekeeiping
            Completed = DateTime.Now;
            TotalProcessingTime = (int)Math.Round(Completed.Subtract(Started).TotalSeconds);
            Status = Status.Completed; 
        }
    }
}
