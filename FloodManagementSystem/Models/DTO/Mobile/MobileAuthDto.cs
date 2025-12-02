using System.ComponentModel.DataAnnotations;

namespace GlobalDisasterManagement.Models.DTO.Mobile
{
    /// <summary>
    /// Mobile authentication request
    /// </summary>
    public class MobileLoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string? DeviceToken { get; set; }
        public DevicePlatform? Platform { get; set; }
        public string? DeviceInfo { get; set; }
    }

    /// <summary>
    /// Mobile authentication response
    /// </summary>
    public class MobileAuthResponse
    {
        public bool Success { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public MobileUserDto? User { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// Mobile registration request
    /// </summary>
    public class MobileRegisterRequest
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        public int? CityId { get; set; }
        public int? LGAId { get; set; }

        public string? DeviceToken { get; set; }
        public DevicePlatform? Platform { get; set; }
    }

    /// <summary>
    /// Lightweight user DTO for mobile
    /// </summary>
    public class MobileUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? CityName { get; set; }
        public string? LGAName { get; set; }
        public bool EmailConfirmed { get; set; }
    }

    /// <summary>
    /// Token refresh request
    /// </summary>
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Device token update request
    /// </summary>
    public class UpdateDeviceTokenRequest
    {
        [Required]
        public string DeviceToken { get; set; } = string.Empty;

        [Required]
        public DevicePlatform Platform { get; set; }

        public string? DeviceInfo { get; set; }
    }
}
