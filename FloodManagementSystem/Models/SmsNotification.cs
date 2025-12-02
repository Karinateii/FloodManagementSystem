using System.ComponentModel.DataAnnotations;

namespace GlobalDisasterManagement.Models
{
    public class SmsNotification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }

        public MessagePriority Priority { get; set; }

        public NotificationStatus Status { get; set; }

        public string? TwilioMessageSid { get; set; }

        public Guid? DisasterIncidentId { get; set; }
        public DisasterIncident? DisasterIncident { get; set; }

        public Guid? DisasterAlertId { get; set; }
        public DisasterAlert? DisasterAlert { get; set; }

        public string? UserId { get; set; }
        public User? User { get; set; }

        public DateTime SentAt { get; set; }

        public DateTime? DeliveredAt { get; set; }

        public string? ErrorMessage { get; set; }

        public int RetryCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum NotificationType
    {
        IncidentAlert,
        ShelterUpdate,
        EvacuationOrder,
        WeatherWarning,
        AllClear,
        ResourceAvailable,
        SystemAnnouncement,
        IncidentVerification,
        StatusUpdate
    }

    public enum MessagePriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4,
        Critical = 5
    }

    public enum NotificationStatus
    {
        Pending,
        Sent,
        Delivered,
        Failed,
        Queued
    }
}
