using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using static FlightHack.Globals;

namespace FlightHack
{
    public class QueueManager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));

        public int JobsInQueue;
        public int JobsInProgress;
        public int JobsCompleted;
        public int JobsFailed;

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

            if(NewJobs)
            {
                // Go through each new job:
                // check if they pass the data criteria
                // add to the stack
            }
            else
            {
                // do nothing
            }

        }

        public void CheckQueueStatus()
        {

#if DEBUG
            log.DebugFormat("Checking Queue Status");
#endif
            
            if(JobQueue.Count > 0)
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

                }
                else if(BottomOfStack.Status == Status.Completed)
                {
                    JobsCompleted++;
                    JobQueue.Dequeue();
                }
                else if(BottomOfStack.Status == Status.Failed)
                {

                }

                // We've got a job that's in progress
                // Don't need to do anything on that front
            }

            bool JobsInProgress = false;

            // Check if there's any jobs in progress

            if(JobsInProgress)
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
