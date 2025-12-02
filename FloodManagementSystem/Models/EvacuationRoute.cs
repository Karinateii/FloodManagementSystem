using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalDisasterManagement.Models
{
    public class EvacuationRoute
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? CityId { get; set; }
        
        [ForeignKey("CityId")]
        public City? City { get; set; }

        public int? LGAId { get; set; }
        
        [ForeignKey("LGAId")]
        public LGA? LGA { get; set; }

        // Route Coordinates (JSON array of lat/lng points)
        [Required]
        public string RouteCoordinates { get; set; } = string.Empty;

        // Start and End Points
        [Required]
        public double StartLatitude { get; set; }

        [Required]
        public double StartLongitude { get; set; }

        [Required]
        public double EndLatitude { get; set; }

        [Required]
        public double EndLongitude { get; set; }

        [StringLength(500)]
        public string? StartAddress { get; set; }

        [StringLength(500)]
        public string? EndAddress { get; set; }

        // Route Information
        [Range(0, 1000)]
        public double DistanceKm { get; set; }

        [Range(0, 1440)]
        public int EstimatedTimeMinutes { get; set; }

        public RouteStatus Status { get; set; } = RouteStatus.Open;

        public bool IsVerified { get; set; }
        public bool IsPrimary { get; set; } // Primary evacuation route

        // Safety Information
        [Range(0, 1000)]
        public double? MinimumElevationMeters { get; set; }

        [Range(0, 1000)]
        public double? MaximumElevationMeters { get; set; }

        public bool HasLighting { get; set; }
        public bool HasShelters { get; set; }
        public bool IsAccessible { get; set; } // Wheelchair/vehicle accessible

        // Linked Shelters (JSON array of shelter IDs)
        public string? LinkedShelterIds { get; set; }

        public string? Warnings { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdated { get; set; }
        public DateTime? LastVerified { get; set; }
    }

    public enum RouteStatus
    {
        Open = 0,
        Congested = 1,
        PartiallyBlocked = 2,
        Flooded = 3,
        Closed = 4,
        Unsafe = 5
    }
}
