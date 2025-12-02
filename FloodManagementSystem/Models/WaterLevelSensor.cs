using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalDisasterManagement.Models
{
    /// <summary>
    /// Water level sensor for monitoring rivers, dams, and flood-prone areas
    /// </summary>
    public class WaterLevelSensor : IoTSensor
    {
        // Threshold Configuration (in meters)
        public double NormalLevel { get; set; } = 0;
        public double WarningLevel { get; set; } = 2.0;
        public double DangerLevel { get; set; } = 3.0;
        public double CriticalLevel { get; set; } = 4.0;

        // Sensor Specifications
        public double MinMeasurableLevel { get; set; } = 0;
        public double MaxMeasurableLevel { get; set; } = 10.0;
        public double Accuracy { get; set; } = 0.01; // meters

        [StringLength(50)]
        public string Unit { get; set; } = "meters";

        // Location Context
        [StringLength(100)]
        public string? WaterBodyName { get; set; } // e.g., "Ogun River", "Epe Lagoon"

        public WaterBodyType? WaterBodyType { get; set; }

        // Current Reading
        public double? CurrentLevel { get; set; }
        public DateTime? CurrentReadingTime { get; set; }
        public WaterLevelStatus? CurrentStatus { get; set; }

        // Navigation
        public virtual ICollection<WaterLevelReading> Readings { get; set; } = new List<WaterLevelReading>();
    }

    public enum WaterBodyType
    {
        River,
        Lake,
        Dam,
        Lagoon,
        Canal,
        Reservoir,
        Stream,
        Ocean
    }

    public enum WaterLevelStatus
    {
        Normal,
        BelowNormal,
        Warning,
        Danger,
        Critical
    }

    /// <summary>
    /// Individual water level reading from sensor
    /// </summary>
    public class WaterLevelReading
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid SensorId { get; set; }

        [ForeignKey("SensorId")]
        public virtual WaterLevelSensor? Sensor { get; set; }

        [Required]
        public double Level { get; set; } // in meters

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public WaterLevelStatus Status { get; set; }

        // Rate of change (meters per hour)
        public double? RateOfChange { get; set; }

        // Data Quality
        public bool IsValid { get; set; } = true;
        public bool IsCalibrated { get; set; } = true;

        [StringLength(200)]
        public string? Notes { get; set; }

        // Alert Triggered
        public bool AlertTriggered { get; set; } = false;
        public DateTime? AlertTriggeredAt { get; set; }
    }
}
