using Newtonsoft.Json;
using System.Collections.Generic;

namespace FlightHack.Query
{
    public class Input
    {
        [JsonProperty("General")]
        public General General { get; set; }

        [JsonProperty("SearchSpaceConstraints")]
        public AirportParm Airport { get; set; }

        [JsonProperty("FixedLegs")]
        public List<Leg> FixedLegs { get; set; }

        [JsonProperty("DumpLeg")]
        public Leg DumpLeg { get; set; }
    }

    public class General
    {
        [JsonProperty("NoOfAdults")]
        public int NoOfAdults { get; set; }

        [JsonProperty("NoOfSeniors")]
        public int NoOfSeniors { get; set; }

        [JsonProperty("NoOfYouths")]
        public int NoOfYouths { get; set; }

        [JsonProperty("NoOfChildren")]
        public int NoOfChildren { get; set; }

        [JsonProperty("NoOfInfantsInSeat")]
        public int NoOfInfantsInSeat { get; set; }

        [JsonProperty("NoOfInfantsInLap")]
        public int NoOfInfantsInLap { get; set; }

        [JsonProperty("Stops")]
        public string Stops { get; set; }

        [JsonProperty("ExtraStops")]
        public string ExtraStops { get; set; }

        [JsonProperty("Cabin")]
        public string Cabin { get; set; }

        [JsonProperty("Currency")]
        public string Currency { get; set; }

        [JsonProperty("SalesCity")]
        public string SalesCity { get; set; }

        [JsonProperty("AllowAirportChanges")]
        public bool AllowAirportChanges { get; set; }

        [JsonProperty("OnlyShowFlightsAndPricesWithAvailableSeats")]
        public bool OnlyShowFlightsAndPricesWithAvailableSeats { get; set; }

        [JsonProperty("OriginalFarePrice")]
        public double OriginalFarePrice { get; set; }
    }

    public class AirportParm
    {
        [JsonProperty("MinimumDistanceBetweenAirports")]
        public int MinDist { get; set; }

        [JsonProperty("MaximumDistanceBetweenAirports")]
        public int MaxDist { get; set; }

        [JsonProperty("MinimumNoOfCarriersServicingAnAirport")]
        public int MinNoCarriers { get; set; }
    }

    public class Leg
    {
        [JsonProperty("OriginCity")]
        public string OriginCity { get; set; }

        [JsonProperty("DestinationCity")]
        public string DestinationCity { get; set; }

        [JsonProperty("RoutingCode")]
        public string RoutingCode { get; set; }

        [JsonProperty("ExtensionCode")]
        public string ExtensionCode { get; set; }

        [JsonProperty("Date")]
        public string Date { get; set; }

        [JsonProperty("DateOption")]
        public string DateOption { get; set; }

        [JsonProperty("DateFlexibility")]
        public string DateFlexibility { get; set; }

        [JsonProperty("IsFixed")]
        public bool IsFixed { get; set; }
    }
}

