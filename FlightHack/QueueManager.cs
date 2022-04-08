using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FlightHack.Globals;

namespace FlightHack
{
    public class QueueManager
    {
        public int JobsInQueue;
        public int JobsInProgress;
        public int JobsCompleted;

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
            if(JobQueue.Peek().Status == Status.InProgress)
            {
                // We've got a job that's in progress
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
