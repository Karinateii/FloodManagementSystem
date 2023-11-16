using Microsoft.ML.Data;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace NewLagosFloodDetectionSystem.Models
{
    public class FloodDataModel
    {
        [Key]
        [LoadColumn(0)]
        public string City { get; set; }
        [LoadColumn(1)]
        public float Month { get; set; }
        //[LoadColumn(2)]
        //public string MonthString => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(Month));

        [LoadColumn(2)]
        public float MaxTemp { get; set; }
        //public string MaxTempString => MaxTemp.ToString();
        [LoadColumn(3)]
        public float MinTemp { get; set; }
        //public string MinTempString => MinTemp.ToString();

        [LoadColumn(4)]
        public float MeanTemp { get; set; }
        //public string MeanTempString => MeanTemp.ToString();

        [LoadColumn(5)]
        public float Precipitation { get; set; }
        //public string PrecipitationString => Precipitation.ToString();

        [LoadColumn(6)]
        public float PrecipCover { get; set; }
        //public string PrecipCoverString => PrecipCover.ToString();


        [LoadColumn(7)]
        public bool FloodRisk { get; set; }

        public string SetMonthString(string monthString)
        {
            Month = DateTime.ParseExact(monthString, "MMMM", CultureInfo.CurrentCulture).Month;
            return Month.ToString();
        }

    }
}

        //[LoadColumn(6)]
        //public float Humidity { get; set; }

        //[LoadColumn(7)]
        //public float WindSpeed { get; set; }
        //[LoadColumn(0)]
        //public int Year { get; set; }
        //[LoadColumn(2)]
        //public float Longitude { get; set; }

        //[LoadColumn(3)]
        //public float Elevation { get; set; }