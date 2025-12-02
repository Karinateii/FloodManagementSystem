using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalDisasterManagement.Models
{
    public class DisasterIncident
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DisasterType DisasterType { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public IncidentStatus Status { get; set; } = IncidentStatus.Reported;

        [Required]
        public IncidentSeverity Severity { get; set; } = IncidentSeverity.Low;

        // Location Information
        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        public int? CityId { get; set; }
        
        [ForeignKey("CityId")]
        public City? City { get; set; }

        public int? LGAId { get; set; }
        
        [ForeignKey("LGAId")]
        public LGA? LGA { get; set; }

        // Disaster-Specific Information (JSON for flexibility)
        public string? DisasterSpecificData { get; set; } // JSON: water depth, magnitude, wind speed, etc.

        // Impact Assessment
        public bool RoadBlocked { get; set; }
        public bool PowerOutage { get; set; }
        public bool StructuralDamage { get; set; }
        public bool PeopleTrapped { get; set; }
        public bool CasualtiesReported { get; set; }
        public bool EnvironmentalHazard { get; set; }
        
        [Range(0, 10000)]
        public int? AffectedPeople { get; set; }

        // Media
        public string? PhotoUrls { get; set; } // JSON array of URLs
        public string? VideoUrls { get; set; } // JSON array of URLs

        // Reporter Information
        [Required]
        public string ReporterId { get; set; } = string.Empty;
        
        [ForeignKey("ReporterId")]
        public User? Reporter { get; set; }

        [StringLength(20)]
        public string? ReporterPhone { get; set; }

        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedBy { get; set; }

        // Emergency Response
        public bool EmergencyServicesNotified { get; set; }
        public DateTime? EmergencyResponseTime { get; set; }
        public string? AssignedResponders { get; set; } // JSON array of responder IDs

        // Timestamps
        [Required]
        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        public DateTime? LastUpdated { get; set; }

        // Notes and Updates
        public string? ResponseNotes { get; set; }
        public string? AdminNotes { get; set; }

        // Tags for categorization
        public string? Tags { get; set; } // JSON array
    }

    public enum DisasterType
    {
        Flood = 1,
        Earthquake = 2,
        Fire = 3,
        Hurricane = 4,
        Tornado = 5,
        Tsunami = 6,
        Landslide = 7,
        Drought = 8,
        Wildfire = 9,
        VolcanicEruption = 10,
        Storm = 11,
        Cyclone = 12,
        Avalanche = 13,
        HeatWave = 14,
        ColdWave = 15,
        Epidemic = 16,
        Industrial = 17,
        Chemical = 18,
        Nuclear = 19,
        Terrorism = 20,
        CivilUnrest = 21,
        BuildingCollapse = 22,
        Explosion = 23,
        GasLeak = 24,
        OilSpill = 25,
        Other = 99
    }

    public enum IncidentStatus
    {
        Reported = 0,
        Verified = 1,
        InProgress = 2,
        EmergencyDispatched = 3,
        UnderControl = 4,
        Resolved = 5,
        Closed = 6,
        Duplicate = 7,
        False = 8
    }

    public enum IncidentSeverity
    {
        Low = 1,        // Minor incident, no immediate danger
        Moderate = 2,   // Some disruption, precautions advised
        High = 3,       // Significant threat, evacuation advised
        Critical = 4,   // Life-threatening, immediate evacuation required
        Catastrophic = 5 // Widespread disaster, multiple casualties, major infrastructure damage
    }
}
