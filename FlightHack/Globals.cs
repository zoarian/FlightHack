using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace FlightHack
{
    public class Globals
    {
        public static ItaMatrixHandler MatrixClient;
        public static DiscordClient Disc;
        public static List<Airport> Airports;
        public static System.Collections.Specialized.NameValueCollection AppSettings; // = ConfigurationManager.AppSettings;

        public enum Status
        {
            Initial,
            InQueue,
            InProgress,
            Completed,
            Failed
        }

        public enum QStatus
        {
            Initial,
            Idling,
            NewJobData,
            JobReadyForProcessing,
            JobInProgress,
            JobReadyForDeletion,
            ErrorMode
        }
    }
}
