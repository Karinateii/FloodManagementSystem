using System.ComponentModel.DataAnnotations;

namespace GlobalDisasterManagement.Models
{
    /// <summary>
    /// Represents a USSD session for feature phone users
    /// </summary>
    public class UssdSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string SessionId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public UssdMenuState CurrentState { get; set; } = UssdMenuState.MainMenu;

        public string? UserInput { get; set; }

        public string? ContextData { get; set; } // JSON data for maintaining context

        public int? CityId { get; set; }
        public int? LGAId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// USSD menu navigation states
    /// </summary>
    public enum UssdMenuState
    {
        MainMenu = 0,
        ReportDisaster = 1,
        ViewAlerts = 2,
        FindShelter = 3,
        EmergencyContacts = 4,
        SelectDisasterType = 5,
        EnterLocation = 6,
        EnterDescription = 7,
        ConfirmReport = 8,
        SelectCity = 9,
        SelectLGA = 10,
        ViewShelterList = 11,
        ViewShelterDetails = 12,
        ViewAlertsList = 13,
        ViewAlertDetails = 14,
        RequestHelp = 15,
        CheckIncidentStatus = 16,
        EvacuationInfo = 17,
        SafetyTips = 18,
        LanguageSelection = 19
    }

    /// <summary>
    /// USSD request/response types
    /// </summary>
    public enum UssdRequestType
    {
        Initiation = 1,  // User dials USSD code
        Response = 2     // User responds to menu
    }
}
