using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalDisasterManagement.Models
{
    /// <summary>
    /// Rainfall sensor for monitoring precipitation levels
    /// </summary>
    public class RainfallSensor : IoTSensor
    {
        // Threshold Configuration (in millimeters)
        public double LightRainThreshold { get; set; } = 2.5;
        public double ModerateRainThreshold { get; set; } = 10.0;
        public double HeavyRainThreshold { get; set; } = 50.0;
        public double VeryHeavyRainThreshold { get; set; } = 100.0;

        // Sensor Specifications
        public double CollectionArea { get; set; } = 1000; // cm²
        public double Resolution { get; set; } = 0.2; // mm

        [StringLength(50)]
        public string Unit { get; set; } = "millimeters";

        // Measurement Period
        public int MeasurementIntervalMinutes { get; set; } = 15;

        // Current Reading
        public double? CurrentRainfall { get; set; } // last interval
        public double? HourlyRainfall { get; set; }
        public double? DailyRainfall { get; set; }
        public DateTime? CurrentReadingTime { get; set; }
        public RainfallIntensity? CurrentIntensity { get; set; }

        // Navigation
        public virtual ICollection<RainfallReading> Readings { get; set; } = new List<RainfallReading>();
    }

    public enum RainfallIntensity
    {
        None,
        Light,
        Moderate,
        Heavy,
        VeryHeavy,
        Extreme
    }

    /// <summary>
    /// Individual rainfall reading from sensor
    /// </summary>
    public class RainfallReading
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid SensorId { get; set; }

        [ForeignKey("SensorId")]
        public virtual RainfallSensor? Sensor { get; set; }

        [Required]
        public double Rainfall { get; set; } // millimeters for the period

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        public RainfallIntensity Intensity { get; set; }

        // Cumulative totals
        public double? HourlyCumulative { get; set; }
        public double? DailyCumulative { get; set; }
        public double? MonthlyCumulative { get; set; }

        // Data Quality
        public bool IsValid { get; set; } = true;

        [StringLength(200)]
        public string? Notes { get; set; }

        // Alert Triggered
        public bool AlertTriggered { get; set; } = false;
        public DateTime? AlertTriggeredAt { get; set; }
    }
}
