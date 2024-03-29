﻿using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.Linq;

namespace FlightHack
{
    public class Airport
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Airport));

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("lat")]
        public string Lat { get; set; }

        [JsonProperty("lon")]
        public string Lon { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("woeid")]
        public string Woeid { get; set; }

        [JsonProperty("tz")]
        public string Tz { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("runway_length")]
        public object RunwayLength { get; set; }

        [JsonProperty("elev")]
        public object Elev { get; set; }

        [JsonProperty("icao")]
        public string Icao { get; set; }

        [JsonProperty("direct_flights")]
        public string DirectFlights { get; set; }

        [JsonProperty("carriers")]
        public string Carriers { get; set; }

        public Airport() {}

        public static List<Airport> ProcessFile(string FileLocation)
        {
            StreamReader r = new StreamReader(FileLocation);
            string jsonString = r.ReadToEnd();

            return JsonConvert.DeserializeObject<List<Airport>>(jsonString);
        }

        public static int DistanceBetweenAirports(Airport A, Airport B)
        {
            var sCoord = new GeoCoordinate(double.Parse(A.Lat), double.Parse(A.Lon));
            var eCoord = new GeoCoordinate(double.Parse(B.Lat), double.Parse(B.Lon));

            return (int)Math.Round(sCoord.GetDistanceTo(eCoord)/1000.0);
        }

        /// <summary>
        /// Calculates an average distance between all airports in the list.
        /// </summary>
        /// <param name="Airports">Distance in km</param>
        /// <returns></returns>
        public static int AverageDistanceBetweenAllAirports(List<Airport> Airports)
        {
            int AverageDistance = 0;

            for(int i = 0; i < Airports.Count; i++)
            {
                for(int j = (i+1); j < Airports.Count; j++)
                {
                    AverageDistance += DistanceBetweenAirports(Airports[i], Airports[j]);
                }
            }

            int NumberOfConnections = Airports.Count*(Airports.Count - 1)/2;

            // Divide by the no of connections to get avg, covert to km
            AverageDistance /= NumberOfConnections;

            return (int)Math.Round(AverageDistance/1000.0);
        }

        private static void PruneAirportsBasedOnCarriers(List<Airport> Airports, int MinNoOfCarriers)
        {
            for (int i = Airports.Count - 1; i > 0; i--)
            {
                if (Int32.Parse(Airports[i].Carriers) > MinNoOfCarriers)
                {
                    // Passed the test
                }
                else
                {
                    Airports.RemoveAt(i);
                }
            }
        }

        public static List<Tuple<Airport, Airport>> PruneDumpConnections(List<Airport> Airports, int MinDistance, int MaxDistance)
        {
            List<Tuple<Airport, Airport>> DumpConnections = new List<Tuple<Airport, Airport>>();

            for (int i = 0; i < Airports.Count; i++)
            {
                for (int j = (i + 1); j < Airports.Count; j++)
                {
                    int DistanceBetweenPair = DistanceBetweenAirports(Airports[i], Airports[j]);

                    if(DistanceBetweenPair > MinDistance && DistanceBetweenPair < MaxDistance)
                    {
                        DumpConnections.Add(new Tuple<Airport, Airport>(Airports[i], Airports[j]));
                        DumpConnections.Add(new Tuple<Airport, Airport>(Airports[j], Airports[i]));
                    }
                    else
                    {
                        // Skip this pair
                    }
                }
            }

            return DumpConnections;
        }

        /// <summary>
        /// Returns a chunked list of dump connections
        /// </summary>
        /// <param name="AirportFileLocation">Location of the json file containing airport details</param>
        /// <param name="MinNoOfCarriers">No of carriers used for restricting the search space - anything below this will be discarded</param>
        /// <param name="MinDistance">Minimum distance for the search</param>
        /// <param name="MaxDistance">Maximum distance for the search</param>
        /// <param name="BinSize">Used for splitting the list into managable chunks, so we don't kill the chrome driver</param>
        /// <returns></returns>
        public static List<Tuple<Airport, Airport>> GetAllDumpConnections(string AirportFileLocation, int MinNoOfCarriers, int MinDistance, int MaxDistance)
        {
            List<Airport> Airports = ProcessFile(AirportFileLocation);

            int InitialNoOfAirports = Airports.Count;

            log.InfoFormat(" {0} airports from file", InitialNoOfAirports);

            PruneAirportsBasedOnCarriers(Airports, MinNoOfCarriers);

            log.InfoFormat(" {0} airports after carrier pruning", Airports.Count);

            List<Tuple<Airport, Airport>> DumpConnections = PruneDumpConnections(Airports, MinDistance, MaxDistance);

            log.InfoFormat(" {0} dump leg connections, based on the distance of {1} to {2}", DumpConnections.Count, MinDistance, MaxDistance);

            return DumpConnections;
        }

        /// <summary>
        /// Returns a chunked list of dump connections
        /// </summary>
        /// <param name="AirportFileLocation">Location of the json file containing airport details</param>
        /// <param name="MinNoOfCarriers">No of carriers used for restricting the search space - anything below this will be discarded</param>
        /// <param name="MinDistance">Minimum distance for the search</param>
        /// <param name="MaxDistance">Maximum distance for the search</param>
        /// <param name="BinSize">Used for splitting the list into managable chunks, so we don't kill the chrome driver</param>
        /// <returns></returns>
        public static List<List<Tuple<Airport, Airport>>> CompleteAirportPruning(string AirportFileLocation, int MinNoOfCarriers, int MinDistance, int MaxDistance, int BinSize)
        {
            List<Airport> Airports = ProcessFile(AirportFileLocation);

            int InitialNoOfAirports = Airports.Count;

            log.InfoFormat(" {0} airports from file", InitialNoOfAirports);

            PruneAirportsBasedOnCarriers(Airports, MinNoOfCarriers);

            log.InfoFormat(" {0} airports after carrier pruning", Airports.Count);

            List<Tuple<Airport, Airport>> DumpConnections = PruneDumpConnections(Airports, MinDistance, MaxDistance);

            log.InfoFormat(" {0} dump leg connections, based on the distance of {1} to {2}", DumpConnections.Count, MinDistance, MaxDistance);

            return Helpers.ChunkBy(DumpConnections, BinSize);
        }
    }

    public static class Helpers
    {
        /// <summary>
        /// Used for breaking up a list into chunks
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}
