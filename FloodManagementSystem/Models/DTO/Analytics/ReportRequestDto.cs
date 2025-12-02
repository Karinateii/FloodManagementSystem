namespace GlobalDisasterManagement.Models.DTO.Analytics
{
    /// <summary>
    /// Request parameters for generating reports
    /// </summary>
    public class ReportRequestDto
    {
        public ReportType ReportType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ReportFormat Format { get; set; } = ReportFormat.Pdf;
        
        // Filters
        public List<string>? DisasterTypes { get; set; }
        public List<int>? CityIds { get; set; }
        public List<int>? LgaIds { get; set; }
        public string? Severity { get; set; }
        
        // Options
        public bool IncludeCharts { get; set; } = true;
        public bool IncludeRawData { get; set; } = false;
        public string? CustomTitle { get; set; }
    }

    /// <summary>
    /// Types of reports available
    /// </summary>
    public enum ReportType
    {
        DisasterImpact,
        ResponseEffectiveness,
        ShelterUtilization,
        IoTSensorPerformance,
        NotificationDelivery,
        PredictionAccuracy,
        Comprehensive
    }

    /// <summary>
    /// Report output formats
    /// </summary>
    public enum ReportFormat
    {
        Pdf,
        Excel,
        Csv,
        Json
    }

    /// <summary>
    /// Generated report information
    /// </summary>
    public class ReportResultDto
    {
        public string ReportId { get; set; } = Guid.NewGuid().ToString();
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public ReportType ReportType { get; set; }
        public ReportFormat Format { get; set; }
        public DateTime GeneratedAt { get; set; }
        public long FileSizeBytes { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
    }
}
