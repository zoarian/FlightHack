/*using Discord;
using Discord.Webhook;*/
using System;
using System.Collections.Generic;
using System.IO;
using Discord;
using FlightHack.Query;
using log4net;

namespace FlightHack
{
    public class DiscordClient
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DiscordClient));

        public string WebhookURL { get; set; }
        public string AvatarURL { get; set; }

        public Discord.Webhook.DiscordWebhook hook;

        public DiscordClient(string WebhookUrl, string AvatarURL)
        {
            hook = new Discord.Webhook.DiscordWebhook();
            hook.Url = WebhookUrl;
            this.AvatarURL = AvatarURL;
        }

        public void SendResults(string FilePath, Input Input, ItaMatrixHandler Matrix, int FlightsScanned, string TotalTime)
        {
            Discord.DiscordMessage message = new Discord.DiscordMessage();
            message.AvatarUrl = AvatarURL;

            string FirstLeg = "1st Leg [" + Input.FixedLegs[0].Date + "] " + Input.FixedLegs[0].OriginCity + " -> " + Input.FixedLegs[0].DestinationCity + "\n";
            string SecondLeg = "2nd Leg [" + Input.FixedLegs[1].Date + "] " + Input.FixedLegs[1].OriginCity + " -> " + Input.FixedLegs[1].DestinationCity + "\n";
            string Price = "Original Fare Price: " + Input.General.OriginalFarePrice + " Currency: " + Input.General.Currency + "\n";

            string SearchTitle = "Search Parameters" + "\n";
            string Carriers = "Airports Minimum No Of Carriers: " + Input.Airport.MinNoCarriers + "\n";
            string SearchParam = "SleepTimer: " + Matrix.SleepTimer + "ms" + " No Results Timeout: " + Matrix.SearchLimitNoResults + " Results Timeout: " + Matrix.SearchLimitWithResults + " \n";
            string Distance = "Distance Between Dump Ariports. Min: " + Input.Airport.MinDist + " Max: " + Input.Airport.MaxDist + " Bin size: " + Matrix.NoOfParallelSearches + "\n";
            string DumpParam = "Dump Leg Routing Codes: " + Input.DumpLeg.RoutingCode + "\n";
            string ResultParam = "Flights Scanned: " + FlightsScanned + " Total Time Taken: " + TotalTime + "s\n ";

            Discord.DiscordEmbed embed = new Discord.DiscordEmbed();
            embed.Title = "Fuel Dump Scrapper";
            embed.Description = FirstLeg + SecondLeg + Price + SearchTitle + Carriers + SearchParam + Distance + DumpParam + ResultParam;
            embed.Url = Matrix.URL;
            embed.Timestamp = DateTime.Now;
            embed.Color = Color.Red;

            message.Embeds = new List<DiscordEmbed>();
            message.Embeds.Add(embed);

            hook.Send(message, new FileInfo(Matrix.JsonFileLocation));

            message = new DiscordMessage();
            hook.Send(message, new FileInfo(FilePath));
        }

        public void SendResults(string FilePath, Job Job)
        {
            Discord.DiscordMessage message = new Discord.DiscordMessage();
            message.AvatarUrl = AvatarURL;

            string JobReport = "Job " + Job.ID + " " + Job.Name + " Job Report\n";
            JobReport += "Status: " + Job.Status + ". Completed " + Job.CurrentQueryNo + " out of " + Job.TotalNoOfQueries + " queries.";
            JobReport += "Total time taken (s): " + (Job.TotalQueuingTime + Job.TotalProcessingTime) + "\n ";
            JobReport += "Queue time (s): " + Job.TotalQueuingTime + " (predicted " + Job.EstimatedQueuingTime + ").";
            JobReport += "Procesing time (s): " + Job.TotalProcessingTime + " (predicted " + Job.EstimatedProcessingTime + ").";
            JobReport += "1st Leg [" + Job.Input.FixedLegs[0].Date + "] " + Job.Input.FixedLegs[0].OriginCity + " -> " + Job.Input.FixedLegs[0].DestinationCity + "\n";
            JobReport += "2nd Leg [" + Job.Input.FixedLegs[1].Date + "] " + Job.Input.FixedLegs[1].OriginCity + " -> " + Job.Input.FixedLegs[1].DestinationCity + "\n";
            JobReport += "Original Fare Price: " + Job.Input.General.OriginalFarePrice + " Currency: " + Job.Input.General.Currency + "\n";

            string SearchTitle = "Search Parameters" + "\n";
            SearchTitle += "Airports Minimum No Of Carriers: " + Job.Input.Airport.MinNoCarriers + "\n";
            SearchTitle += "SleepTimer: " + Globals.MatrixClient.SleepTimer + "ms" + " No Results Timeout: " + Globals.MatrixClient.SearchLimitNoResults + " Results Timeout: " + Globals.MatrixClient.SearchLimitWithResults + " \n";
            SearchTitle += "Distance Between Dump Ariports. Min: " + Job.Input.Airport.MinDist + " Max: " + Job.Input.Airport.MaxDist + " Bin size: " + Job.NoOfParallelSearches + "\n";
            SearchTitle += "Dump Leg Routing Codes: " + Job.Input.DumpLeg.RoutingCode + "\n";

            Discord.DiscordEmbed embed = new Discord.DiscordEmbed();
            embed.Title = "Fuel Dump Scrapper";
            embed.Description = JobReport + SearchTitle;
            embed.Url = Globals.MatrixClient.URL;
            embed.Timestamp = DateTime.Now;
            embed.Color = Color.Red;

            message.Embeds = new List<DiscordEmbed>();
            message.Embeds.Add(embed);

            hook.Send(message, new FileInfo(Globals.MatrixClient.JsonFileLocation));

            message = new DiscordMessage();
            hook.Send(message, new FileInfo(FilePath));
        }
    }
}
