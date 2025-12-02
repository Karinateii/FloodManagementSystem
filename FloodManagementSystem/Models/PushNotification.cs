using System.ComponentModel.DataAnnotations;

namespace GlobalDisasterManagement.Models
{
    /// <summary>
    /// Represents a push notification sent through Firebase Cloud Messaging
    /// </summary>
    public class PushNotification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Body { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// JSON object with custom data payload
        /// Example: {"alertId": "123", "severity": "High", "action": "view-details"}
        /// </summary>
        public string? Data { get; set; }

        public DevicePlatform? TargetPlatform { get; set; }

        /// <summary>
        /// Comma-separated device tokens (if sent to specific devices)
        /// </summary>
        public string? DeviceTokens { get; set; }

        /// <summary>
        /// Topic name (if sent to a topic subscription)
        /// </summary>
        [StringLength(100)]
        public string? Topic { get; set; }

        [Required]
        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

        public int SentCount { get; set; } = 0;
        public int FailedCount { get; set; } = 0;

        /// <summary>
        /// Foreign key to DisasterAlert (if notification is for a disaster alert)
        /// </summary>
        public Guid? DisasterAlertId { get; set; }
        public DisasterAlert? DisasterAlert { get; set; }

        /// <summary>
        /// Foreign key to DisasterIncident (if notification is for an incident)
        /// </summary>
        public Guid? DisasterIncidentId { get; set; }
        public DisasterIncident? DisasterIncident { get; set; }

        public MessagePriority Priority { get; set; } = MessagePriority.Normal;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Firebase Cloud Messaging response (JSON)
        /// </summary>
        public string? FcmResponse { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
