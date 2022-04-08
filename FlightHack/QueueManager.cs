using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
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

        public static Queue<Job> JobQueue; // This will act as out job list

        // Need some way to know which jobs are new

        public QueueManager()
        {

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

            if(NewJobs)
            {
                CheckAndSanitizeInput(NewFilePath);
            }
            else
            {

            }
        }

        private void CheckAndSanitizeInput(string NewFilePath)
        {
            // Input file is invalid

            // Input file doesn't have crucial data

            // Input file is ok, but the criteria is too strict and there's no dump legs
        }

        public async void CheckQueueStatus()
        {
#if DEBUG
            log.DebugFormat("Checking queue status");
#endif
            bool AreThereJobsInProgress = false;

            if (JobQueue.Count > 0)
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
                else if(BottomOfStack.Status == Status.Completed)
                {
                    JobsCompleted++;
                    JobsInProgress = 0;

                    await BottomOfStack.CompleteJob();

                    QStatus = QStatus.JobReadyForDeletion;

                    // Complete Job (async) -> then dequeue 
                    // Dequeue in Processing ??? JobQueue.Dequeue();
                }
                else if(BottomOfStack.Status == Status.Failed)
                {
                    JobsFailed++;
                    JobsInProgress = 0;

                    await BottomOfStack.CompleteJob();

                    QStatus = QStatus.JobReadyForDeletion;
                    // Send Failure code (async) -> then dequeue
                    //JobQueue.Dequeue();
                }
                else if(BottomOfStack.Status == Status.InQueue)
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
        }
    }
}
