/*using Discord;
using Discord.Webhook;*/
using System;
using System.Collections.Generic;
using System.IO;
using Discord;
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

        public void SendResults(string FilePath, ItaMatrixHandler Matrix, int bin, int MinDist, int MaxDist, int MinNoOfCarriers, int FlightsScanned, string TotalTime)
        {
            Discord.DiscordMessage message = new Discord.DiscordMessage();
            message.AvatarUrl = "https://w7.pngwing.com/pngs/205/97/png-transparent-airplane-icon-a5-takeoff-computer-icons-flight-airplane.png";

            string FirstLeg = "1st Leg [" + Matrix.LegOneDepartureDate + "] " + Matrix.LegOneOriginCityCode + " -> " + Matrix.LegOneDestinationCityCode + "\n";
            string SecondLeg = "2nd Leg [" + Matrix.LegTwoDepartureDate + "] " + Matrix.LegTwoOriginCityCode + " -> " + Matrix.LegTwoDestinationCityCode + "\n";
            string Price = "Original Fare Price: " + Matrix.OriginalFare + " Currency: " + Matrix.Currency + "\n";

            string SearchTitle = "Search Parameters" + "\n";
            string Carriers = "Airports Minimum No Of Carriers: " + MinNoOfCarriers + "\n";
            string SearchParam = "SleepTimer: " + Matrix.SleepTimer + "ms" + " Search Timeout: " + Matrix.MaxSearchTimeLimit + " (+/-20) seconds. \n";
            string Distance = "Distance Between Dump Ariports. Min: " + MinDist + " Max: " + MaxDist + " Bin size: " + bin + "\n";
            string ResultParam = "Flights Scanned: " + FlightsScanned + " Total Time Taken: " + TotalTime + "\n ";

            Discord.DiscordEmbed embed = new Discord.DiscordEmbed();
            embed.Title = "Fuel Dump Scrapper";
            embed.Description = FirstLeg + SecondLeg + Price + SearchTitle + Carriers + SearchParam + Distance + ResultParam;
            embed.Url = "https://matrix.itasoftware.com";
            embed.Timestamp = DateTime.Now;
            embed.Color = Color.Red;

            message.Embeds = new List<DiscordEmbed>();
            message.Embeds.Add(embed);

            hook.Send(message);

            message = new DiscordMessage();
            hook.Send(message, new FileInfo(FilePath));
        }
    }
}
