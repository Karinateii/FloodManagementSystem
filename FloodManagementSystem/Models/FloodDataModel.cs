using Microsoft.ML.Data;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace GlobalDisasterManagement.Models
{
    public class FloodDataModel
    {
        [Key]
        [LoadColumn(0)]
        [Required]
        [StringLength(100)]
        public string City { get; set; }
        
        [LoadColumn(1)]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12")]
        public float Month { get; set; }

        [LoadColumn(2)]
        [Range(-50, 60, ErrorMessage = "Maximum temperature must be between -50 and 60°C")]
        public float MaxTemp { get; set; }
        
        [LoadColumn(3)]
        [Range(-50, 60, ErrorMessage = "Minimum temperature must be between -50 and 60°C")]
        public float MinTemp { get; set; }

        [LoadColumn(4)]
        [Range(-50, 60, ErrorMessage = "Mean temperature must be between -50 and 60°C")]
        public float MeanTemp { get; set; }

        [LoadColumn(5)]
        [Range(0, 1000, ErrorMessage = "Precipitation must be between 0 and 1000mm")]
        public float Precipitation { get; set; }

        [LoadColumn(6)]
        [Range(0, 100, ErrorMessage = "Precipitation cover must be between 0 and 100%")]
        public float PrecipCover { get; set; }

        [LoadColumn(7)]
        public bool FloodRisk { get; set; }

        public string SetMonthString(string monthString)
        {
            Month = DateTime.ParseExact(monthString, "MMMM", CultureInfo.CurrentCulture).Month;
            return Month.ToString();
        }
    }
}
