using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalDisasterManagement.Models
{
    /// <summary>
    /// Base class for all IoT sensors in the disaster management system
    /// </summary>
    public abstract class IoTSensor
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public SensorType SensorType { get; set; }

        [Required]
        public SensorStatus Status { get; set; } = SensorStatus.Active;

        // Location
        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        public int? CityId { get; set; }
        [ForeignKey("CityId")]
        public virtual City? City { get; set; }

        public int? LGAId { get; set; }
        [ForeignKey("LGAId")]
        public virtual LGA? LGA { get; set; }

        // Device Information
        [StringLength(100)]
        public string? Manufacturer { get; set; }

        [StringLength(100)]
        public string? Model { get; set; }

        [StringLength(50)]
        public string? FirmwareVersion { get; set; }

        // Connection Details
        [Required]
        public DateTime InstallationDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastMaintenanceDate { get; set; }

        public DateTime? NextMaintenanceDate { get; set; }

        public DateTime LastCommunicationDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastDataReceivedDate { get; set; }

        // Battery and Power
        public int? BatteryLevel { get; set; } // 0-100%

        public PowerSource PowerSource { get; set; } = PowerSource.Mains;

        // Alert Configuration
        public bool AlertsEnabled { get; set; } = true;

        [StringLength(500)]
        public string? AlertRecipients { get; set; } // JSON array of email/phone

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        public bool IsDeleted { get; set; } = false;
    }

    public enum SensorType
    {
        WaterLevel,
        Rainfall,
        Weather,
        FlowRate,
        Humidity,
        Temperature,
        WindSpeed,
        Pressure,
        SoilMoisture,
        Seismic,
        AirQuality
    }

    public enum SensorStatus
    {
        Active,
        Inactive,
        Maintenance,
        Faulty,
        Offline,
        Calibrating
    }

    public enum PowerSource
    {
        Mains,
        Battery,
        Solar,
        Hybrid
    }
}
