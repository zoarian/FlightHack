/*using Discord;
using Discord.Webhook;*/
using System;
using System.Collections.Generic;
using System.IO;
using Discord;
using FlightHack.Query;
using Newtonsoft.Json;

namespace FlightHack
{
    public class DiscordClient
    {
        public string WebhookURL { get; set; }
        public Discord.Webhook.DiscordWebhook hook;

        public DiscordClient(string Url)
        {
            hook = new Discord.Webhook.DiscordWebhook();
            hook.Url = Url;
        }

        public void SendResults(string FilePath, Input Input, ItaMatrixHandler Matrix, int FlightsScanned, string TotalTime)
        {
            Discord.DiscordMessage message = new Discord.DiscordMessage();
            message.AvatarUrl = "https://w7.pngwing.com/pngs/205/97/png-transparent-airplane-icon-a5-takeoff-computer-icons-flight-airplane.png";

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
            embed.Url = "https://matrix.itasoftware.com";
            embed.Timestamp = DateTime.Now;
            embed.Color = Color.Red;

            message.Embeds = new List<DiscordEmbed>();
            message.Embeds.Add(embed);

            hook.Send(message, new FileInfo(Matrix.JsonFileLocation));

            message = new DiscordMessage();
            hook.Send(message, new FileInfo(FilePath));
        }
    }
}
