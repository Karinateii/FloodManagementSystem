using System.ComponentModel.DataAnnotations;

namespace GlobalDisasterManagement.Models
{
    public class SmsTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string TemplateText { get; set; } = string.Empty;

        public NotificationType Type { get; set; }

        [MaxLength(10)]
        public string LanguageCode { get; set; } = "en";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
