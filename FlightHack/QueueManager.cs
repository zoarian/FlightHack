using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FlightHack.Query;
using log4net;
using Newtonsoft.Json;
using static FlightHack.Globals;

namespace FlightHack
{
    public class QueueManager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(QueueManager));

        public QStatus QStatus;
        public static int CurrentJobID;
        public static int JobsInQueue;
        public static int JobsInProgress;
        public static int JobsCompleted;
        public static int JobsFailed;

        public string NewInputFile;
        public List<Tuple<Airport, Airport>> BufferDumpLegs { get; set; }

        public static Queue<Job> JobQueue; // First in first out

        public QueueManager()
        {
            CurrentJobID = 0;
            CreateWatcher(Globals.AppSettings["InputBaseDirectory"]);
            JobQueue = new Queue<Job>();
        }
      
        /// <summary>
        /// A watcher that will scan for new files 
        /// </summary>
        /// <param name="NewFilePath"></param>
        private void CreateWatcher(string NewFilePath)
        {
            try
            {
                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.NotifyFilter = NotifyFilters.Attributes |
                                        NotifyFilters.CreationTime |
                                        NotifyFilters.DirectoryName |
                                        NotifyFilters.FileName |
                                        NotifyFilters.LastAccess |
                                        NotifyFilters.LastWrite |
                                        NotifyFilters.Security |
                                        NotifyFilters.Size;
                watcher.Filter = "*.json";
                watcher.Created += new FileSystemEventHandler(NewFileDetected);
                watcher.Path = NewFilePath;
                watcher.EnableRaisingEvents = true;

                log.Info("File watcher initialized");
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed to initialize file watcher: {0}", ex.Message);
            }
        }

        private void NewFileDetected(object sender, FileSystemEventArgs e)
        {
            int EstimatedQueuingTime = 0;

            log.InfoFormat("Found new files: {0}, Path: {1}", e.Name, e.FullPath);

            Input Temp = CheckAndSanitizeInput(e.FullPath, e.Name);

            if(Temp != null)
            {
                // Calculate the estimated queueing time (sum of the process time for all prior jobs)
                EstimatedQueuingTime = JobQueue.Sum(x => x.EstimatedProcessingTime);

                JobQueue.Enqueue(new Job(++CurrentJobID, Temp, Status.InQueue, BufferDumpLegs));
            }

            BufferDumpLegs = null;

            // try copy to see if it works better with creation watcher
            try
            {
                Thread.Sleep(200); // TODO: Need to wait for the file to close -> use an event?
                File.Delete(e.FullPath);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed to copy from {0} to {1} and then delete. Error {2}", e.FullPath, (Globals.AppSettings["InputArchiveBaseDirectory"] + e.Name), ex.Message);
            }

            // TODO: At the moment, file copying affects the file watcher.
            // We will need some way of copying the file (maybe saving the json as new file?)
            /*
                        try
                        {
                            File.Copy(e.FullPath, (Globals.AppSettings["InputArchiveBaseDirectory"] + e.Name));

                            Thread.Sleep(10000);

                            File.Delete(e.FullPath);
                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat("Failed to copy from {0} to {1} and then delete. Error {2}", e.FullPath, (Globals.AppSettings["InputArchiveBaseDirectory"] + e.Name), ex.Message);
                        }*/
        }

        private Input CheckAndSanitizeInput(string NewFilePath, string FileName)
        {
            Input Input = null; 

            // We're only getting JSON files from the filter - no need to check for file type
            bool InputPassedMuster = true;
            string InputProcessMessage = "File " + FileName + " \n";

            try
            {
                // Try deserializing the file
                StreamReader r = new StreamReader(NewFilePath);
                Input = JsonConvert.DeserializeObject<Input>(r.ReadToEnd());

                log.Info("Deserialized the new input file");
                InputProcessMessage += "JSON deserialized successfully\n";

                // Check for crucial input data
                // Conditions are:
                // - we need at least 1 passenger
                // - we need at least 1 leg, with origin & destination cities + date
                // - we need at least 1 dump leg with date
                // - we need ALL search space constraints
                // TODO: Do checks for each leg
                try
                {
                    int NoOfPassengers = Input.General.NoOfInfantsInLap + Input.General.NoOfInfantsInSeat + Input.General.NoOfYouths + Input.General.NoOfChildren + Input.General.NoOfAdults + Input.General.NoOfSeniors;
                    DateTime temp;

                    if (NoOfPassengers < 1)
                    {
                        InputPassedMuster = false;
                        InputProcessMessage += "There must be at least 1 passenger in the input file\n";
                    }
                    else
                    {
                        InputProcessMessage += "Passed the no of passengers sanity check\n";
                    }

                    if(Input.FixedLegs[0].OriginCity != null && Input.FixedLegs[0].DestinationCity != null && Input.FixedLegs[0].Date != null)
                    {
                        // First leg crucial data is not null. Now validate the data
                        if (!Globals.Airports.Any(x => x.Code == Input.FixedLegs[0].OriginCity))
                        {
                            InputPassedMuster = false;
                            InputProcessMessage += "The origin city of 1st leg does not have a match in the airport list\n";
                        }

                        if (!Globals.Airports.Any(x => x.Code == Input.FixedLegs[0].DestinationCity))
                        {
                            InputPassedMuster = false;
                            InputProcessMessage += "The destination city of 1st leg does not have a match in the airport list\n";
                        }

                        bool IsValidDateLeg1 = DateTime.TryParseExact(Input.FixedLegs[0].Date, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out temp);

                        if (!IsValidDateLeg1)
                        {
                            InputPassedMuster = false;
                            InputProcessMessage += "The departure date of first leg is invalid\n";
                        }
                    }
                    else
                    {
                        InputPassedMuster = false;
                        InputProcessMessage += "One of: Origin City, Destination City or Departure Date were not supplied\n";
                    }

                    if(Input.DumpLeg.Date != null)
                    {
                        bool IsValidDateDumpLeg = DateTime.TryParseExact(Input.DumpLeg.Date, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out temp);

                        if (!IsValidDateDumpLeg)
                        {
                            InputPassedMuster = false;
                            InputProcessMessage += "The departure date of first leg is invalid\n";
                        }
                    }
                    else
                    {
                        InputPassedMuster = false;
                        InputProcessMessage += "The departure date of dump leg is invalid\n";
                    }

                    if(Input.Airport.MinDist != null && Input.Airport.MaxDist != null && Input.Airport.MinNoCarriers != null)
                    {
                        if(Input.Airport.MinDist < 0 || Input.Airport.MaxDist < 0 || Input.Airport.MinNoCarriers < 0)
                        {
                            InputPassedMuster = false;
                            InputProcessMessage += "Distance/No of carriers cannot be negative\n";
                        }

                        if (Input.Airport.MinDist >= Input.Airport.MaxDist)
                        {
                            InputPassedMuster = false;
                            InputProcessMessage += "Min distance must be smaller than max distance\n";
                        }
                    }
                    else
                    {
                        InputPassedMuster = false;
                        InputProcessMessage += "One of search paramaters (min, max distance, min no of carriers) has not been defined\n";
                    }
                }
                catch (Exception ex)
                {
                    InputPassedMuster = false;
                    log.ErrorFormat("Failed in processing input: {0}", ex);
                }

                // Input file is ok, but we need to check if we get any dump legs
                if (InputPassedMuster)
                {
                    BufferDumpLegs = Airport.GetAllDumpConnections(AppSettings["AirortDataFile"], Input.Airport.MinNoCarriers, Input.Airport.MinDist, Input.Airport.MaxDist);
               
                    if(BufferDumpLegs.Count == 0)
                    {
                        InputPassedMuster = false;
                    }
                }
    
                // TODO: check if we've already got a job with the same parameters
            }
            catch (Exception e)
            {
                InputPassedMuster = false;
                log.ErrorFormat("Failed to parse file {0}, error: {1}", FileName, e.Message);
            }           

            if(InputPassedMuster)
            {
                InputProcessMessage += "Was processed successfully";
                log.Info(InputProcessMessage);

                // pass the input to new job, queue it
            }
            else
            {
                log.Error(InputProcessMessage);

                Input = null;
            }
        
            return Input;
        }

        public async void CheckQueueStatus()
        {
#if DEBUG
            log.DebugFormat("Checking queue status. Jobs in queue: {0}", JobQueue.Count);
#endif

            if (JobQueue.Count == 0)
            {
#if DEBUG
                log.DebugFormat("Queue manager is idling");
#endif
                QStatus = QStatus.Idling;
            }
            else
            {
                Job BottomOfStack = JobQueue.Peek();

#if DEBUG
                log.DebugFormat("Bottom of the queue stack. Job {0} is {1}", BottomOfStack.Name, BottomOfStack.Status);
#endif

                if (BottomOfStack.Status == Status.InProgress)
                {
                    QStatus = QStatus.JobInProgress;
                }
                else if (BottomOfStack.Status == Status.Completed)
                {
                    JobsCompleted++;
                    JobsInProgress = 0;

                    await BottomOfStack.CompleteJob();

                    QStatus = QStatus.JobReadyForDeletion;

                    // Complete Job (async) -> then dequeue 
                    // Dequeue in Processing ??? JobQueue.Dequeue();
                }
                else if (BottomOfStack.Status == Status.Failed)
                {
                    JobsFailed++;
                    JobsInProgress = 0;

                    await BottomOfStack.CompleteJob();

                    QStatus = QStatus.JobReadyForDeletion;
                    // Send Failure code (async) -> then dequeue
                    //JobQueue.Dequeue();
                }
                else if (BottomOfStack.Status == Status.InQueue)
                {
                    QStatus = QStatus.JobReadyForProcessing;
                }
                else
                {
                    // Bottom of the stack is NEITHER In Progress, Completed, Failed or In Queue
                    // Check other jobs to see what's going on
                    QStatus = QStatus.ErrorMode;
                    log.ErrorFormat("Bottom of the queue stack. Job {0} is {1}", BottomOfStack, BottomOfStack.Status);
                }
            }
        }

        public void QueueManagement()
        {
            int JobID = 0;

            switch (QStatus)
            {
                case QStatus.NewJobData:
                    JobQueue.Enqueue(new Job(JobID, NewInputFile));
                    break;
                case QStatus.JobInProgress:
                    log.Debug("In Progress");
                    break;
                case QStatus.JobReadyForDeletion:
                    JobQueue.Dequeue();
                    break;
                case QStatus.JobReadyForProcessing:
                    JobQueue.Peek().StartJobAsync();
                    break;
            }

        }
    }
}
