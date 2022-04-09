using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FlightHack.Query;
using Newtonsoft.Json;
using log4net;
using log4net.Config;
using System.Configuration;

namespace FlightHack
{
    class App
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));

        public static async Task Main(string[] args)
        {
            Globals.AppSettings = ConfigurationManager.AppSettings;
            XmlConfigurator.Configure(new FileInfo(Globals.AppSettings["Log4netLocation"]));

            log.Info("FlightHack App Startup Was Successfull");

            // Client Initialization
            Globals.Disc = new DiscordClient(Globals.AppSettings["DiscordWebhookURL"], Globals.AppSettings["AvatarUrl"]);
            Globals.MatrixClient = new ItaMatrixHandler(Globals.AppSettings["ItaMatrixConfig"], Globals.AppSettings["ChromeDriverPath"]);
            Globals.Airports = Airport.ProcessFile(Globals.AppSettings["AirortDataFile"]);

            string NewFiles = "C:\\Users\\mikop\\Documents\\Projects\\FlightHack\\InputTest\\";

            QueueManager JobQueue = new QueueManager(NewFiles);

            // TODO: Initialize the queueing system. 
            // - Check file location
            // If no files -> idle
            // If there are files -> check if in queue or not
            // If files not in queue -> put in queue
            // If all files are in queue...

            bool IsRunning = true;

            Console.Read();

            while (IsRunning)
            {
                JobQueue.ScanForNewJobs();

                JobQueue.CheckQueueStatus();

                JobQueue.QueueManagement();
            }

            Environment.Exit(0);
        }

        private void StartUp()
        {
            // Read datatypes such as the airport codes
            // Initialize clients
        }

        private void Shutdown()
        {
            // Kill any remaining chromedrivers
        }
    }
}