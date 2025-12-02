using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalDisasterManagement.Models
{
    /// <summary>
    /// Early warning alerts for predicted disasters
    /// </summary>
    public class DisasterAlert
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DisasterType DisasterType { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public AlertSeverity Severity { get; set; }

        [Required]
        public AlertStatus Status { get; set; } = AlertStatus.Active;

        // Affected Area
        public string? AffectedCountries { get; set; } // JSON array
        public string? AffectedRegions { get; set; } // JSON array
        public string? AffectedCities { get; set; } // JSON array
        public string? AffectedLGAs { get; set; } // JSON array

        // Geographic Bounds (for mapping)
        public double? MinLatitude { get; set; }
        public double? MaxLatitude { get; set; }
        public double? MinLongitude { get; set; }
        public double? MaxLongitude { get; set; }

        // Timing
        [Required]
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public DateTime? EffectiveFrom { get; set; }
        
        public DateTime? ExpiresAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Source Information
        [StringLength(200)]
        public string? IssuedBy { get; set; } // Organization/Agency

        [StringLength(500)]
        public string? SourceUrl { get; set; }

        public bool IsOfficial { get; set; } // From government/official source

        // Actions and Instructions
        public string? RecommendedActions { get; set; }
        public string? EvacuationInstructions { get; set; }
        public string? SafetyTips { get; set; }

        // Statistics
        public int ViewCount { get; set; }
        public int ShareCount { get; set; }
        public int AcknowledgmentCount { get; set; }

        // Linked Data
        public Guid? RelatedIncidentId { get; set; }
        
        [ForeignKey("RelatedIncidentId")]
        public DisasterIncident? RelatedIncident { get; set; }

        public string? RelatedAlertIds { get; set; } // JSON array of related alert IDs
    }

    public enum AlertSeverity
    {
        Advisory = 1,      // General information
        Watch = 2,         // Conditions are favorable
        Warning = 3,       // Expected to occur
        Emergency = 4,     // Imminent or occurring
        Extreme = 5        // Extraordinary threat
    }

    public enum AlertStatus
    {
        Draft = 0,
        Active = 1,
        Updated = 2,
        Expired = 3,
        Cancelled = 4
    }
}
