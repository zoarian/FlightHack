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
            // Scan:
            // - discord
            // - REST request?
            // - file location


        }

        public void CheckQueueStatus()
        {
#if DEBUG
            log.DebugFormat("Checking Queue Status");
#endif
            bool AreThereJobsInProgress = false;

            if (JobQueue.Count > 0)
            {
                // First, check if we have any jobs
            }
            else
            {
                Job BottomOfStack = JobQueue.Peek();

#if DEBUG
                log.DebugFormat("Bottom of the queue stack. Job {0} is {1}", BottomOfStack, BottomOfStack.Status);
#endif

                if (BottomOfStack.Status == Status.InProgress)
                {
                    AreThereJobsInProgress = true;
                }
                else if(BottomOfStack.Status == Status.Completed)
                {
                    JobsCompleted++;
                    JobsInProgress = 0;

                    // Complete Job (async) -> then dequeue 
                    JobQueue.Dequeue();
                }
                else if(BottomOfStack.Status == Status.Failed)
                {
                    JobsFailed++;
                    JobsInProgress = 0;

                    // Send Failure code (async) -> then dequeue
                    JobQueue.Dequeue();
                }
                else if(BottomOfStack.Status == Status.InQueue)
                {
                    // Initialize jobs
                }
                else
                {
                    // Bottom of the stack is NEITHER In Progress, Completed, Failed or In Queue
                    // Check other jobs to see what's going on

                    log.ErrorFormat("Bottom of the queue stack. Job {0} is {1}", BottomOfStack, BottomOfStack.Status);
                }
            }

            

            // Check if there's any jobs in progress

            if(AreThereJobsInProgress)
            {
                if(false)
                {

                }
            }
            else
            {

            }
        }
    }
}
