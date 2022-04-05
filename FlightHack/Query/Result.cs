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
            QueryMessage = "Initiated Query";
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

        public static string SaveResultsToFile(string FilePath, List<Result> Results)
        {
            string ResultsFileBaseName = "_Results.csv";
            string ResultsFileFullPath = @"C:\Users\MP\Documents\FlightHack\" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ResultsFileBaseName;

            // Create the file, or overwrite if the file exists.
            using (FileStream fs = File.Create(ResultsFileFullPath))
            {
                byte[] info = new UTF8Encoding(true).GetBytes("This is some text in the file.");
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
