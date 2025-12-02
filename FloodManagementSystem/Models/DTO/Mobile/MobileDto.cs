namespace GlobalDisasterManagement.Models.DTO.Mobile
{
    /// <summary>
    /// Lightweight disaster alert for mobile
    /// </summary>
    public class MobileAlertDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; }
        public DisasterType DisasterType { get; set; }
        public string? CityName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// Paginated alerts response
    /// </summary>
    public class PaginatedAlertsResponse
    {
        public List<MobileAlertDto> Alerts { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Mobile incident report request
    /// </summary>
    public class MobileIncidentRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DisasterType DisasterType { get; set; }
        public AlertSeverity Severity { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Location { get; set; }
        public IFormFile? Photo { get; set; }
    }

    /// <summary>
    /// Lightweight incident DTO
    /// </summary>
    public class MobileIncidentDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DisasterType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public DateTime ReportedAt { get; set; }
        public string? ReportedByName { get; set; }
        public int? AffectedPeople { get; set; }
        public List<string>? PhotoUrls { get; set; }
    }

    /// <summary>
    /// Nearby shelter search request
    /// </summary>
    public class NearbySheltersRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RadiusKm { get; set; } = 10;
        public int MaxResults { get; set; } = 20;
    }

    /// <summary>
    /// Mobile shelter DTO
    /// </summary>
    public class MobileShelterDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Distance { get; set; }
        public int Capacity { get; set; }
        public int CurrentOccupancy { get; set; }
        public string? ContactPhone { get; set; }
        public string? Facilities { get; set; }
    }

    /// <summary>
    /// IoT sensor summary for mobile
    /// </summary>
    public class MobileSensorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SensorType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime? LastReading { get; set; }
    }

    /// <summary>
    /// User location update
    /// </summary>
    public class LocationUpdateRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Accuracy { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Dashboard summary for mobile
    /// </summary>
    public class MobileDashboardDto
    {
        public int ActiveAlertsCount { get; set; }
        public int RecentIncidentsCount { get; set; }
        public int NearbySheltersCount { get; set; }
        public List<MobileAlertDto>? RecentAlerts { get; set; }
        public string? UserCityName { get; set; }
        public string? UserLGAName { get; set; }
    }

    /// <summary>
    /// Offline sync data
    /// </summary>
    public class OfflineSyncRequest
    {
        public List<MobileIncidentRequest> IncidentReports { get; set; } = new();
    }
    
    /// <summary>
    /// Offline incident data
    /// </summary>
    public class OfflineIncidentData
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DisasterType DisasterType { get; set; }
        public AlertSeverity Severity { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Location { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Generic mobile API response
    /// </summary>
    public class MobileApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
    }
}
