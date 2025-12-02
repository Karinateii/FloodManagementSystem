using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalDisasterManagement.Models
{
    public class ShelterCheckIn
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ShelterId { get; set; }
        
        [ForeignKey("ShelterId")]
        public EmergencyShelter? Shelter { get; set; }

        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Range(0, 150)]
        public int Age { get; set; }

        public Gender Gender { get; set; }

        [Range(1, 50)]
        public int FamilyMembers { get; set; } = 1;

        public bool HasMedicalNeeds { get; set; }
        public string? MedicalConditions { get; set; }

        public bool NeedsSpecialAssistance { get; set; }
        public string? SpecialRequirements { get; set; }

        // Check-in/out
        [Required]
        public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

        public DateTime? CheckOutTime { get; set; }

        public bool IsCheckedOut { get; set; }

        // Emergency Contact
        [StringLength(200)]
        public string? EmergencyContactName { get; set; }

        [StringLength(20)]
        public string? EmergencyContactPhone { get; set; }

        // Origin Information
        [StringLength(500)]
        public string? EvacuatedFromAddress { get; set; }

        public string? Notes { get; set; }
    }

    public enum Gender
    {
        NotSpecified = 0,
        Male = 1,
        Female = 2,
        Other = 3
    }
}
