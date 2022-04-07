using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;

namespace FlightHack.Query
{
    public class Result
    {
        public string QueryTime { get; set; }
        public double NewFare { get; set; }
        public double DistanceBetweenDumpAirports { get; set; }
        public string DumpLegOriginCode { get; set; }
        public string DumpLegDestCode { get; set; }
        public string SumOfCarriers { get; set; }
        public string DumpLegDepartureDate { get; set; }
        public string TimeStamp { get; set; }
        public string QueryMessage { get; set; }

        public Result() 
        {
            NewFare = 0;
            DistanceBetweenDumpAirports = 0;
            QueryTime = "";
            DumpLegOriginCode = "";
            DumpLegDestCode = "";
            SumOfCarriers = "";
            DumpLegDepartureDate = "";
            TimeStamp = "";
            QueryMessage = "";
        }

        public Result(string QueryTime, double NewFare, double DistanceBetweenDumpAirports, string DumpLegOriginCode, string DumpLegDestCode, string DumpLegDepartureDate, string TimeStamp, string QueryMessage, int SumOfCarriers)
        {
            this.QueryTime = QueryTime;
            this.NewFare = NewFare;
            this.DistanceBetweenDumpAirports = DistanceBetweenDumpAirports;
            this.DumpLegOriginCode = DumpLegOriginCode;
            this.DumpLegDestCode = DumpLegDestCode;
            this.DumpLegDepartureDate = DumpLegDepartureDate;
            this.TimeStamp = TimeStamp;
            this.QueryMessage = QueryMessage;
            this.SumOfCarriers = SumOfCarriers.ToString();
        }

        // This really needs to be moved to the job class
        public static string SaveResultsToFile(string ResultsBaseFilePath, List<Result> Results, Input Input)
        {
            string ResultsFileExtension = ".csv";
            string ResultsFileFullPath = ResultsBaseFilePath + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_" + Input.FixedLegs[0].OriginCity + "_TO_" + Input.FixedLegs[0].DestinationCity + ResultsFileExtension;

            // Create the file, or overwrite if the file exists.
            using (FileStream fs = File.Create(ResultsFileFullPath))
            {
                byte[] info = new UTF8Encoding(true).GetBytes("Beep");
                fs.Write(info, 0, info.Length);
            }

            StreamWriter writer = new StreamWriter(ResultsFileFullPath);
            var csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture);

            csvWriter.WriteHeader<Result>();
            csvWriter.NextRecord();
            csvWriter.WriteRecords(Results);

            writer.Flush();
            writer.Close();

            return ResultsFileFullPath;
        }
    }
}
