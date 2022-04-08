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
            int JobTimeTaken = 0;
            int NoOfQueriesPerformed = 0; 
            string ResultsFileFullPath;

            var AppSettings = ConfigurationManager.AppSettings;
            XmlConfigurator.Configure(new FileInfo(AppSettings["Log4netLocation"]));

            log.Info("FlightHack App Startup Was Successfull");

            // Client Initialization
            List<Result> Results = new List<Result>();

            DiscordClient Disc = new DiscordClient(AppSettings["DiscordWebhookURL"], AppSettings["AvatarUrl"]);
            ItaMatrixHandler MatrixClient = new ItaMatrixHandler(AppSettings["ItaMatrixConfig"], AppSettings["ChromeDriverPath"]);

            StreamReader r = new StreamReader(MatrixClient.JsonFileLocation);
            Input Input = JsonConvert.DeserializeObject<Input>(r.ReadToEnd());

            // TODO: Initialize the queueing system. 
            // - Check file location
            // If no files -> idle
            // If there are files -> check if in queue or not
            // If files not in queue -> put in queue
            // If all files are in queue...

            bool IsRunning = false;
            bool WorkCondition = true;
            
            while(IsRunning)
            {
                // Main Loop
                
                // Queue manager checks for new job conditions (file/discord webhook/rest???)

                // If Queue IS NOT empty and Queue has no job in progress
                // start work
                if(WorkCondition)
                {

                }
                else
                {

                }
            }

            // Perform the job
            JobTimeTaken = await MatrixClient.StartJobAsync(Input, Results, AppSettings["AirortDataFile"]);

            // Save the results
            ResultsFileFullPath = Result.SaveResultsToFile(AppSettings["QueryResultPath"], Results, Input);

            // Send file to discord
            Disc.SendResults(ResultsFileFullPath, Input, MatrixClient, ItaMatrixHandler.LoopNo, JobTimeTaken.ToString());

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