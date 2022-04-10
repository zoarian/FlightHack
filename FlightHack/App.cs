using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FlightHack.Query;
using Newtonsoft.Json;
using log4net;
using log4net.Config;
using System.Configuration;
using System.Threading;

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

            // Client initialization
            Globals.Disc = new DiscordClient(Globals.AppSettings["DiscordWebhookURL"], Globals.AppSettings["AvatarUrl"]);
            Globals.MatrixClient = new ItaMatrixHandler(Globals.AppSettings["ItaMatrixConfig"], Globals.AppSettings["ChromeDriverPath"]);
            Globals.Airports = Airport.ProcessFile(Globals.AppSettings["AirortDataFile"]);

            // Start the queue
            QueueManager JobQueue = new QueueManager();

            bool IsRunning = true;

            while (IsRunning)
            {
                JobQueue.CheckQueueStatus();

                JobQueue.QueueManagement();

                Thread.Sleep(10000);
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