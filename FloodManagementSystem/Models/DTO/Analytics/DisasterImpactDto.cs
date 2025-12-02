namespace GlobalDisasterManagement.Models.DTO.Analytics
{
    /// <summary>
    /// Disaster impact analysis data
    /// </summary>
    public class DisasterImpactDto
    {
        public string DisasterType { get; set; } = string.Empty;
        public int TotalIncidents { get; set; }
        public int AffectedPeople { get; set; }
        public int EvacuatedPeople { get; set; }
        public double AverageSeverity { get; set; }
        public List<GeographicImpactDto> GeographicImpacts { get; set; } = new();
        public List<TimeSeriesDataDto> Timeline { get; set; } = new();
    }

    /// <summary>
    /// Geographic impact by location
    /// </summary>
    public class GeographicImpactDto
    {
        public string Location { get; set; } = string.Empty;
        public int IncidentCount { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Severity { get; set; } = string.Empty;
    }

    /// <summary>
    /// Time series data point
    /// </summary>
    public class TimeSeriesDataDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public double Value { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}
