using FlightHack.Query;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using static FlightHack.Globals;
using System.Diagnostics;
using System.Threading;

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
        private static readonly ILog log = LogManager.GetLogger(typeof(App));

        // To make life easier
        public int JobID { get; set; }
        public string JobName { get; set; }

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
        public Input Input { get; set; }

        public List<Task> AllTasks { get; set; }

        public List<Result> Results { get; set; }

        public Job() 
        {
            Status = Status.Initial;

            CurrentQueryNo = 0;
            TotalNoOfQueries = 0;

            Results = new List<Result>();
            AllTasks = new List<Task>();

            JobID = 0;
            JobName = "Initial Job";
        }

        public Job(int JobID, Input Input)
        {
            this.JobID = JobID;
            this.Input = Input;
        }

        public async Task<int> StartJobAsync(Input Input, List<Result> Results, string AirortFileLocation)
        {
            List<Tuple<Airport, Airport>> AllDumpLegs = Airport.GetAllDumpConnections(AirortFileLocation, Input.Airport.MinNoCarriers, Input.Airport.MinDist, Input.Airport.MaxDist);

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

                        AllTasks.Add(
                            Task.Run(() =>
                            {
                                try
                                {
                                    log.InfoFormat("{0}/{1} : dump leg connection {2} -> {3}", CurrentQueryNo, AllDumpLegs.Count, DumpLeg.Item1.Code, DumpLeg.Item2.Code);

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

                TotalProcessingTime = (int)watch.Elapsed.TotalSeconds;

                KillChromeDrivers();
            }

            return JobTimeTaken;
        }


        public void CompleteJob()
        {

        }
    }
}
