namespace GlobalDisasterManagement.Models.DTO.Analytics
{
    /// <summary>
    /// Shelter utilization and performance analytics
    /// </summary>
    public class ShelterAnalyticsDto
    {
        public int TotalShelters { get; set; }
        public int ActiveShelters { get; set; }
        public int InactiveShelters { get; set; }
        public int TotalCapacity { get; set; }
        public int CurrentOccupancy { get; set; }
        public double UtilizationRate { get; set; } // percentage
        
        public List<ShelterUtilizationDto> ShelterUtilization { get; set; } = new();
        public List<ShelterCapacityDto> SheltersByCapacity { get; set; } = new();
        public List<TimeSeriesDataDto> OccupancyTrend { get; set; } = new();
        
        // Check-in statistics
        public int TotalCheckIns { get; set; }
        public int CheckInsToday { get; set; }
        public int CheckInsThisWeek { get; set; }
        public double AverageStayDuration { get; set; } // in days
    }

    /// <summary>
    /// Individual shelter utilization
    /// </summary>
    public class ShelterUtilizationDto
    {
        public int ShelterId { get; set; }
        public string ShelterName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int CurrentOccupancy { get; set; }
        public double UtilizationPercentage { get; set; }
        public bool HasMedicalFacility { get; set; }
        public bool HasFood { get; set; }
        public bool HasWater { get; set; }
    }

    /// <summary>
    /// Shelter capacity breakdown
    /// </summary>
    public class ShelterCapacityDto
    {
        public string CapacityRange { get; set; } = string.Empty;
        public int ShelterCount { get; set; }
        public int TotalCapacity { get; set; }
    }
}
