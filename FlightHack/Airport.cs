using Newtonsoft.Json;
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

        public double DistanceBetweenAirports(Airport A, Airport B)
        {
            var sCoord = new GeoCoordinate(double.Parse(A.Lat), double.Parse(A.Lon));
            var eCoord = new GeoCoordinate(double.Parse(B.Lat), double.Parse(B.Lon));

            return sCoord.GetDistanceTo(eCoord);
        }
    }
}
