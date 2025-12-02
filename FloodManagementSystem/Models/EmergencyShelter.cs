using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalDisasterManagement.Models
{
    public class EmergencyShelter
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public ShelterType Type { get; set; }

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public int? CityId { get; set; }
        
        [ForeignKey("CityId")]
        public City? City { get; set; }

        public int? LGAId { get; set; }
        
        [ForeignKey("LGAId")]
        public LGA? LGA { get; set; }

        // Capacity Information
        [Required]
        [Range(1, 100000)]
        public int TotalCapacity { get; set; }

        [Range(0, 100000)]
        public int CurrentOccupancy { get; set; }

        public int AvailableSpaces => TotalCapacity - CurrentOccupancy;

        public bool IsActive { get; set; } = true;
        public bool IsOperational { get; set; } = true;

        // Facilities
        public bool HasMedicalFacility { get; set; }
        public bool HasFood { get; set; }
        public bool HasWater { get; set; }
        public bool HasPower { get; set; }
        public bool HasSanitation { get; set; }
        public bool HasSecurity { get; set; }
        public bool IsAccessible { get; set; } // Wheelchair accessible

        // Contact Information
        [Required]
        [StringLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ContactEmail { get; set; }

        [StringLength(200)]
        public string? ManagerName { get; set; }

        // Operating Details
        public DateTime? OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public string? SpecialNotes { get; set; }
        public string? Amenities { get; set; } // JSON array

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdated { get; set; }

        // Collection navigation
        public ICollection<ShelterCheckIn>? CheckIns { get; set; }
    }

    public enum ShelterType
    {
        School = 1,
        CommunityCenter = 2,
        Stadium = 3,
        Church = 4,
        Mosque = 5,
        Government = 6,
        Hotel = 7,
        Military = 8,
        Hospital = 9,
        Other = 10
    }
}
