using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalDisasterManagement.Models
{
    /// <summary>
    /// Weather station for comprehensive environmental monitoring
    /// </summary>
    public class WeatherSensor : IoTSensor
    {
        // Sensor Capabilities
        public bool HasTemperatureSensor { get; set; } = true;
        public bool HasHumiditySensor { get; set; } = true;
        public bool HasPressureSensor { get; set; } = true;
        public bool HasWindSensor { get; set; } = true;
        public bool HasRainGauge { get; set; } = false;

        // Current Reading
        public double? CurrentTemperature { get; set; } // Celsius
        public double? CurrentHumidity { get; set; } // Percentage
        public double? CurrentPressure { get; set; } // hPa
        public double? CurrentWindSpeed { get; set; } // km/h
        public double? CurrentWindDirection { get; set; } // degrees
        public DateTime? CurrentReadingTime { get; set; }

        // Navigation
        public virtual ICollection<WeatherReading> Readings { get; set; } = new List<WeatherReading>();
    }

    /// <summary>
    /// Individual weather reading from sensor
    /// </summary>
    public class WeatherReading
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid SensorId { get; set; }

        [ForeignKey("SensorId")]
        public virtual WeatherSensor? Sensor { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Temperature
        public double? Temperature { get; set; } // Celsius
        public double? FeelsLike { get; set; }
        public double? DewPoint { get; set; }

        // Humidity
        public double? Humidity { get; set; } // Percentage (0-100)

        // Pressure
        public double? Pressure { get; set; } // hPa
        public double? SeaLevelPressure { get; set; }

        // Wind
        public double? WindSpeed { get; set; } // km/h
        public double? WindGust { get; set; } // km/h
        public double? WindDirection { get; set; } // degrees (0-360)

        [StringLength(20)]
        public string? WindDirectionCardinal { get; set; } // N, NE, E, SE, S, SW, W, NW

        // Precipitation
        public double? Rainfall { get; set; } // mm for the period

        // Visibility
        public double? Visibility { get; set; } // km

        // Cloud Cover
        public int? CloudCover { get; set; } // Percentage (0-100)

        // UV Index
        public double? UVIndex { get; set; }

        // Solar Radiation
        public double? SolarRadiation { get; set; } // W/m²

        // Data Quality
        public bool IsValid { get; set; } = true;

        [StringLength(200)]
        public string? Notes { get; set; }

        // Calculated Fields
        public WeatherCondition? Condition { get; set; }
    }

    public enum WeatherCondition
    {
        Clear,
        PartlyCloudy,
        Cloudy,
        Overcast,
        LightRain,
        Rain,
        HeavyRain,
        Thunderstorm,
        Fog,
        Mist,
        Haze,
        Smoke,
        Dust,
        Sand,
        Tornado
    }
}
