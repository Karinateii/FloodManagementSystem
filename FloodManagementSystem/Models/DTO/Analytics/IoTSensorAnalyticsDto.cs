namespace GlobalDisasterManagement.Models.DTO.Analytics
{
    /// <summary>
    /// IoT sensor performance and health analytics
    /// </summary>
    public class IoTSensorAnalyticsDto
    {
        public int TotalSensors { get; set; }
        public int ActiveSensors { get; set; }
        public int InactiveSensors { get; set; }
        public int MaintenanceSensors { get; set; }
        public double SensorHealthRate { get; set; } // percentage
        
        public List<SensorTypeBreakdownDto> SensorsByType { get; set; } = new();
        public List<SensorHealthDto> SensorHealth { get; set; } = new();
        public List<TimeSeriesDataDto> DataQualityTrend { get; set; } = new();
        
        // Data statistics
        public int TotalReadingsToday { get; set; }
        public int TotalReadingsThisWeek { get; set; }
        public double AverageReadingFrequency { get; set; } // minutes
        public int AlertsTriggered { get; set; }
    }

    /// <summary>
    /// Sensor breakdown by type
    /// </summary>
    public class SensorTypeBreakdownDto
    {
        public string SensorType { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public double HealthRate { get; set; }
    }

    /// <summary>
    /// Individual sensor health status
    /// </summary>
    public class SensorHealthDto
    {
        public int SensorId { get; set; }
        public string SensorName { get; set; } = string.Empty;
        public string SensorType { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? LastReading { get; set; }
        public int HoursSinceLastReading { get; set; }
        public double BatteryLevel { get; set; }
        public bool RequiresMaintenance { get; set; }
    }
}
