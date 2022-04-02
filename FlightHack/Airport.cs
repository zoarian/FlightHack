using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;

namespace FlightHack
{
    public class Airport
    {
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

        public static double DistanceBetweenAirports(Airport A, Airport B)
        {
            var sCoord = new GeoCoordinate(double.Parse(A.Lat), double.Parse(A.Lon));
            var eCoord = new GeoCoordinate(double.Parse(B.Lat), double.Parse(B.Lon));

            return sCoord.GetDistanceTo(eCoord)/1000.0;
        }

        /// <summary>
        /// Calculates an average distance between all airports in the list.
        /// </summary>
        /// <param name="Airports">Distance in km</param>
        /// <returns></returns>
        public static double AverageDistanceBetweenAllAirports(List<Airport> Airports)
        {
            double AverageDistance = 0.0;

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
            AverageDistance /= 1000.0;

            return AverageDistance;
        }

        public static List<Tuple<Airport, Airport>> PruneDumpConnections(List<Airport> Airports, double MaxDistance, double MinDistance)
        {
            List<Tuple<Airport, Airport>> DumpConnections = new List<Tuple<Airport, Airport>>();

            for (int i = 0; i < Airports.Count; i++)
            {
                for (int j = (i + 1); j < Airports.Count; j++)
                {
                    double DistanceBetweenPair = DistanceBetweenAirports(Airports[i], Airports[j]);

                    if(DistanceBetweenPair > MinDistance && DistanceBetweenPair < MaxDistance)
                    {
                        DumpConnections.Add(new Tuple<Airport, Airport>(Airports[i], Airports[j]));
                    }
                    else
                    {
                        // Skip this pair
                    }
                }
            }

            return DumpConnections;
        }
    }
}
