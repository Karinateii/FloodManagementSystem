using System.ComponentModel.DataAnnotations;

namespace GlobalDisasterManagement.Models
{
    /// <summary>
    /// Represents a device token for push notifications
    /// </summary>
    public class DeviceToken
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(500)]
        public string Token { get; set; } = string.Empty;

        public string? UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public DevicePlatform Platform { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// JSON array of topic names the device is subscribed to
        /// Example: ["disasters", "weather", "lagos-alerts"]
        /// </summary>
        public string? TopicSubscriptions { get; set; }

        /// <summary>
        /// JSON object with device information (OS version, app version, device model, etc.)
        /// </summary>
        public string? DeviceInfo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUsedAt { get; set; }
    }

    /// <summary>
    /// Device platform types
    /// </summary>
    public enum DevicePlatform
    {
        Android = 1,
        iOS = 2,
        Web = 3
    }
}
