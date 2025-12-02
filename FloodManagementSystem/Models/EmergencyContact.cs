using System.ComponentModel.DataAnnotations;

namespace GlobalDisasterManagement.Models
{
    public class EmergencyContact
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string OrganizationName { get; set; } = string.Empty;

        [Required]
        public EmergencyServiceType ServiceType { get; set; }

        [Required]
        [StringLength(20)]
        public string PrimaryPhone { get; set; } = string.Empty;

        [StringLength(20)]
        public string? SecondaryPhone { get; set; }

        [StringLength(20)]
        public string? EmergencyHotline { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Website { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public string? CountryCode { get; set; } = "NG"; // ISO country code

        public string? StateProvince { get; set; }

        public bool IsActive { get; set; } = true;
        public bool Is24Hours { get; set; } = true;

        public string? OperatingHours { get; set; }

        public string? ServiceArea { get; set; } // JSON array of covered LGAs/cities

        public int Priority { get; set; } = 1; // 1 = highest

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdated { get; set; }
    }

    public enum EmergencyServiceType
    {
        FloodEmergency = 1,      // LASEMA, NEMA
        Ambulance = 2,           // Medical emergency
        FireService = 3,         // Fire and rescue
        Police = 4,              // Law enforcement
        RedCross = 5,            // Red Cross/Crescent
        DisasterManagement = 6,  // Disaster management agencies
        Utility = 7,             // Power, water utilities
        Search = 8,             // Search and rescue
        Military = 9,            // Military assistance
        NGO = 10,                // Non-governmental organizations
        Other = 11
    }
}
