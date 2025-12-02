using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalDisasterManagement.Models
{
    /// <summary>
    /// Represents a WhatsApp message sent through the system
    /// </summary>
    public class WhatsAppMessage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(20)]
        public string ToPhoneNumber { get; set; } = string.Empty;

        [StringLength(20)]
        public string? FromPhoneNumber { get; set; }

        [Required]
        public string MessageBody { get; set; } = string.Empty;

        public WhatsAppMessageType MessageType { get; set; } = WhatsAppMessageType.Text;

        public WhatsAppMessageDirection Direction { get; set; } = WhatsAppMessageDirection.Outbound;

        public WhatsAppMessageStatus Status { get; set; } = WhatsAppMessageStatus.Pending;

        // Media
        public string? MediaUrl { get; set; }
        public string? MediaContentType { get; set; }

        // Template Message
        public string? TemplateName { get; set; }
        public string? TemplateParameters { get; set; } // JSON array

        // Twilio Response
        public string? MessageSid { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }

        // Timestamps
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? FailedAt { get; set; }

        // Retry Logic
        public int RetryCount { get; set; }
        public DateTime? NextRetryAt { get; set; }

        // Context
        public Guid? DisasterIncidentId { get; set; }
        
        [ForeignKey("DisasterIncidentId")]
        public DisasterIncident? DisasterIncident { get; set; }

        public Guid? DisasterAlertId { get; set; }
        
        [ForeignKey("DisasterAlertId")]
        public DisasterAlert? DisasterAlert { get; set; }

        // User who sent (for inbound messages)
        public string? UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User? User { get; set; }

        // Conversation tracking
        public string? ConversationId { get; set; }
        public Guid? InReplyToMessageId { get; set; }

        // Metadata
        public string? Metadata { get; set; } // JSON for additional data
    }

    public enum WhatsAppMessageType
    {
        Text = 0,
        Image = 1,
        Video = 2,
        Audio = 3,
        Document = 4,
        Location = 5,
        Template = 6,
        Interactive = 7
    }

    public enum WhatsAppMessageDirection
    {
        Outbound = 0,
        Inbound = 1
    }

    public enum WhatsAppMessageStatus
    {
        Pending = 0,
        Queued = 1,
        Sent = 2,
        Delivered = 3,
        Read = 4,
        Failed = 5,
        Undelivered = 6
    }
}
