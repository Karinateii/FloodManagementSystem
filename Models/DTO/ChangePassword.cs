using System.ComponentModel.DataAnnotations;
using System.Security.Policy;

namespace NewLagosFloodDetectionSystem.Models.DTO
{
    public class ChangePassword
    {
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set;}
    }
}
