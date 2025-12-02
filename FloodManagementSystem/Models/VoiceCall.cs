using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalDisasterManagement.Models
{
    /// <summary>
    /// Represents a voice call for disaster alerts and IVR system
    /// </summary>
    public class VoiceCall
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(20)]
        public string ToPhoneNumber { get; set; } = string.Empty;

        [StringLength(20)]
        public string? FromPhoneNumber { get; set; }

        [Required]
        public VoiceCallDirection Direction { get; set; }

        [Required]
        public VoiceCallStatus Status { get; set; } = VoiceCallStatus.Initiated;

        [Required]
        public VoiceCallType CallType { get; set; }

        // Twilio call identifier
        [StringLength(100)]
        public string? CallSid { get; set; }

        // Call duration in seconds
        public int? Duration { get; set; }

        // Recording URL if call was recorded
        [StringLength(500)]
        public string? RecordingUrl { get; set; }

        // Recording duration in seconds
        public int? RecordingDuration { get; set; }

        // IVR menu state for interactive calls
        public IVRMenuState? MenuState { get; set; }

        // User's DTMF input during IVR
        [StringLength(50)]
        public string? UserInput { get; set; }

        // Error information
        [StringLength(10)]
        public string? ErrorCode { get; set; }

        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RingingAt { get; set; }
        public DateTime? AnsweredAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? FailedAt { get; set; }

        // Retry logic for failed calls
        public int RetryCount { get; set; } = 0;
        public DateTime? NextRetryAt { get; set; }

        // Foreign keys for context
        public Guid? DisasterAlertId { get; set; }
        public Guid? DisasterIncidentId { get; set; }
        public string? UserId { get; set; }

        // Navigation properties
        [ForeignKey("DisasterAlertId")]
        public DisasterAlert? DisasterAlert { get; set; }

        [ForeignKey("DisasterIncidentId")]
        public DisasterIncident? DisasterIncident { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        // Additional metadata (JSON)
        public string? Metadata { get; set; }
    }

    /// <summary>
    /// Direction of voice call
    /// </summary>
    public enum VoiceCallDirection
    {
        Outbound = 1,  // System calling user
        Inbound = 2    // User calling system
    }

    /// <summary>
    /// Status of voice call
    /// </summary>
    public enum VoiceCallStatus
    {
        Initiated = 1,    // Call created but not yet sent
        Queued = 2,       // Call queued by Twilio
        Ringing = 3,      // Phone is ringing
        InProgress = 4,   // Call answered and in progress
        Completed = 5,    // Call completed successfully
        Failed = 6,       // Call failed
        Busy = 7,         // Recipient was busy
        NoAnswer = 8,     // No one answered
        Canceled = 9      // Call was canceled
    }

    /// <summary>
    /// Type of voice call
    /// </summary>
    public enum VoiceCallType
    {
        DisasterAlert = 1,              // Automated disaster alert
        IncidentConfirmation = 2,       // Incident report confirmation
        ShelterInformation = 3,         // Emergency shelter details
        EvacuationInstructions = 4,     // Evacuation route guidance
        IVRMenu = 5,                    // Interactive voice response menu
        EmergencyBroadcast = 6,         // Mass emergency notification
        WellnessCheck = 7,              // Check on vulnerable individuals
        VolunteerDispatch = 8           // Volunteer coordination
    }

    /// <summary>
    /// IVR menu navigation states
    /// </summary>
    public enum IVRMenuState
    {
        MainMenu = 0,                   // Welcome menu
        DisasterAlerts = 1,             // Listen to active alerts
        ReportIncident = 2,             // Report new incident
        FindShelter = 3,                // Find nearest shelter
        EvacuationRoutes = 4,           // Get evacuation instructions
        EmergencyContacts = 5,          // Emergency contact numbers
        SelectDisasterType = 6,         // Choose disaster type
        RecordLocation = 7,             // Record location via voice
        RecordDescription = 8,          // Describe incident
        ConfirmReport = 9,              // Confirm incident submission
        SelectLanguage = 10,            // Choose language
        ShelterCapacity = 11,           // Check shelter availability
        MedicalEmergency = 12,          // Medical emergency hotline
        RequestCallback = 13,           // Request human callback
        ProvideFeedback = 14            // System feedback
    }
}
