using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalDisasterManagement.Models
{
    /// <summary>
    /// Resource tracking for disaster response (supplies, equipment, personnel)
    /// </summary>
    public class DisasterResource
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public ResourceType Type { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Unit { get; set; } = string.Empty; // kg, liters, units, people, etc.

        [Required]
        [Range(0, double.MaxValue)]
        public double Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public double? MinimumThreshold { get; set; }

        public ResourceStatus Status { get; set; } = ResourceStatus.Available;

        // Location
        [StringLength(500)]
        public string? StorageLocation { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public int? CityId { get; set; }
        
        [ForeignKey("CityId")]
        public City? City { get; set; }

        // Management
        [StringLength(200)]
        public string? ManagedBy { get; set; } // Organization/Person

        [StringLength(20)]
        public string? ContactPhone { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool RequiresRefrigeration { get; set; }
        public bool IsPerishable { get; set; }

        // Allocation
        public Guid? AllocatedToShelterId { get; set; }
        public Guid? AllocatedToIncidentId { get; set; }

        public DateTime? AllocationDate { get; set; }

        // Tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdated { get; set; }
        public DateTime? LastInventoryCheck { get; set; }

        public string? Notes { get; set; }

        // Financial
        [Range(0, double.MaxValue)]
        public decimal? EstimatedValue { get; set; }

        [StringLength(3)]
        public string? CurrencyCode { get; set; } = "USD";
    }

    public enum ResourceType
    {
        Food = 1,
        Water = 2,
        MedicalSupplies = 3,
        Clothing = 4,
        Shelter = 5,
        Blankets = 6,
        HygieneKits = 7,
        FirstAid = 8,
        Medicine = 9,
        Fuel = 10,
        Generator = 11,
        Vehicle = 12,
        Boat = 13,
        CommunicationEquipment = 14,
        RescueEquipment = 15,
        Personnel = 16,
        Volunteers = 17,
        MedicalStaff = 18,
        SecurityPersonnel = 19,
        Engineers = 20,
        Money = 21,
        BuildingMaterials = 22,
        Tools = 23,
        Other = 99
    }

    public enum ResourceStatus
    {
        Available = 1,
        Allocated = 2,
        InTransit = 3,
        Deployed = 4,
        LowStock = 5,
        OutOfStock = 6,
        Expired = 7,
        Damaged = 8
    }
}
