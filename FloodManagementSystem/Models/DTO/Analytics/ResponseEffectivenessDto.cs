namespace GlobalDisasterManagement.Models.DTO.Analytics
{
    /// <summary>
    /// Emergency response effectiveness metrics
    /// </summary>
    public class ResponseEffectivenessDto
    {
        public double AverageResponseTime { get; set; } // minutes
        public double MedianResponseTime { get; set; }
        public int ResponsesUnder15Min { get; set; }
        public int ResponsesUnder30Min { get; set; }
        public int ResponsesUnder60Min { get; set; }
        public int ResponsesOver60Min { get; set; }
        
        public List<ResponseTimeByTypeDto> ResponseTimeByDisasterType { get; set; } = new();
        public List<ResponseTimeByLocationDto> ResponseTimeByLocation { get; set; } = new();
        public List<TimeSeriesDataDto> ResponseTimeTrend { get; set; } = new();
        
        public double IncidentResolutionRate { get; set; } // percentage
        public int TotalIncidentsResolved { get; set; }
        public int TotalIncidentsPending { get; set; }
    }

    /// <summary>
    /// Response time breakdown by disaster type
    /// </summary>
    public class ResponseTimeByTypeDto
    {
        public string DisasterType { get; set; } = string.Empty;
        public double AverageResponseTime { get; set; }
        public int IncidentCount { get; set; }
    }

    /// <summary>
    /// Response time breakdown by location
    /// </summary>
    public class ResponseTimeByLocationDto
    {
        public string Location { get; set; } = string.Empty;
        public double AverageResponseTime { get; set; }
        public int IncidentCount { get; set; }
    }
}
