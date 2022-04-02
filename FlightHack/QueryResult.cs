using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightHack
{
    public class QueryResult
    {
        public string QueryTime { get; set; }
        public double NewFare { get; set; }
        public double DistanceBetweenDumpAirports { get; set; }
        public string DumpLegOriginCode { get; set; }
        public string DumpLegDestCode { get; set; }
        public string DumpLegDepartureDate { get; set; }
        public string TimeStamp { get; set; }

        public QueryResult() { }
        public QueryResult(string QueryTime, double NewFare, double DistanceBetweenDumpAirports, string DumpLegOriginCode, string DumpLegDestCode, string DumpLegDepartureDate, string TimeStamp)
        {
            this.QueryTime = QueryTime;
            this.NewFare = NewFare;
            this.DistanceBetweenDumpAirports = DistanceBetweenDumpAirports;
            this.DumpLegOriginCode = DumpLegOriginCode;
            this.DumpLegDestCode = DumpLegDestCode;
            this.DumpLegDepartureDate = DumpLegDepartureDate;
            this.TimeStamp = TimeStamp;
        }
    }
}
