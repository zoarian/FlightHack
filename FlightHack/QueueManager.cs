using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlightHack.Query;
using log4net;
using Newtonsoft.Json;
using static FlightHack.Globals;

namespace FlightHack
{
    public class QueueManager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));

        public QStatus QStatus;
        public static int JobsInQueue;
        public static int JobsInProgress;
        public static int JobsCompleted;
        public static int JobsFailed;

        public string NewInputFile;
        public static Input InputBuffer;
        public static Queue<Job> JobQueue; // This will act as out job list

        // Need some way to know which jobs are new

        public QueueManager(string NewFilePath)
        {
            CreateWatcher(NewFilePath);
            JobQueue = new Queue<Job>();
        }

        /// <summary>
        /// Used to scan the resources for new jobs
        /// </summary>
        public void ScanForNewJobs()
        {
            bool NewJobs = false;
            string NewFilePath = "";

            // Scan:
            // - discord
            // - REST request?
            // - file location

            if (NewJobs)
            {
                //CheckAndSanitizeInput(NewFilePath);
            }
            else
            {

            }
        }
       
        /// <summary>
        /// A watcher that will scan for new files 
        /// </summary>
        /// <param name="NewFilePath"></param>
        private void CreateWatcher(string NewFilePath)
        {
            //Create a new FileSystemWatcher.
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.json";
            //watcher.Created += new FileSystemEventHandler(NewFileDetected);
            watcher.Changed += new FileSystemEventHandler(NewFileDetected);
            watcher.Path = NewFilePath;
            watcher.EnableRaisingEvents = true;
        }

        private void NewFileDetected(object sender, FileSystemEventArgs e)
        {
            log.InfoFormat("Found new files: {0}, Path: {1}", e.Name, e.FullPath);

            CheckAndSanitizeInput(e.FullPath, e.Name);
            // Check the 
        }


        private void CheckAndSanitizeInput(string NewFilePath, string FileName)
        {
            // We're only getting JSON files from the filter - no need to check for file type
            bool InputFailed = false;
            string InputProcessMessage = "File " + FileName + " \n";

            try
            {
                // Try deserializing the file
                StreamReader r = new StreamReader(NewFilePath);
                Input Input = JsonConvert.DeserializeObject<Input>(r.ReadToEnd());

                log.Info("Deserialized the new input file");
                InputProcessMessage += "JSON deserialized successfully\n";

                // Check for crucial input data
                // Conditions are:
                // - we need at least 1 passenger
                // - we need at least 1 leg, with origin & destination cities + date
                // - we need at least 1 dump leg with date
                // - we need ALL search space constraints
                try
                {
                    int NoOfPassengers = Input.General.NoOfInfantsInLap + Input.General.NoOfInfantsInSeat + Input.General.NoOfYouths + Input.General.NoOfChildren + Input.General.NoOfAdults + Input.General.NoOfSeniors;
                    
                    if(NoOfPassengers < 1)
                    {
                        InputFailed = true;
                        InputProcessMessage += "There must be at least 1 passenger in the input file\n";
                    }
                    else
                    {
                        InputProcessMessage += "Passed the no of passengers sanity check\n";
                    }




                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Failed to parse file {0}, error: {1}", FileName, e.Message);
            }

            // Input file is ok, but the criteria is too strict and there's no dump legs
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
                log.DebugFormat("Bottom of the queue stack. Job {0} is {1}", BottomOfStack, BottomOfStack.Status);
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
