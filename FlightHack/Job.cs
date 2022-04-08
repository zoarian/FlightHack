using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int CurrentQueryNo { get; set; }
        public int TotalNoOfQueries { get; set; } // This is #DumpLegx x 2 (A->B, B->A)
    }
}
