using GlobalDisasterManagement.Models.DTO.Mobile;
using GlobalDisasterManagement.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlobalDisasterManagement.Controllers.API
{
    [Authorize]
    [Route("api/mobile/notifications")]
    [ApiController]
    public class MobileNotificationController : ControllerBase
    {
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ILogger<MobileNotificationController> _logger;

        public MobileNotificationController(
            IPushNotificationService pushNotificationService,
            ILogger<MobileNotificationController> logger)
        {
            _pushNotificationService = pushNotificationService;
            _logger = logger;
        }

        /// <summary>
        /// Send test notification to current user
        /// </summary>
        [HttpPost("test")]
        public async Task<ActionResult<MobileApiResponse<object>>> SendTestNotification(
            [FromBody] TestNotificationRequest request)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized();
                }

                var userDevices = await _pushNotificationService.GetUserDeviceTokensAsync(userId);
                if (!userDevices.Any())
                {
                    return Ok(new MobileApiResponse<object>
                    {
                        Success = false,
                        Message = "No device tokens registered for this user"
                    });
                }

                int successCount = 0;
                int failureCount = 0;

                foreach (var device in userDevices)
                {
                    var sent = await _pushNotificationService.SendToDeviceAsync(
                        device.Token,
                        request.Title ?? "Test Notification",
                        request.Body ?? "This is a test push notification",
                        request.ImageUrl,
                        new Dictionary<string, string> { { "type", "test" } }
                    );

                    if (sent) successCount++;
                    else failureCount++;
                }

                return Ok(new MobileApiResponse<object>
                {
                    Success = true,
                    Message = $"Sent to {successCount} device(s), {failureCount} failed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test notification");
                return StatusCode(500, new MobileApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while sending test notification"
                });
            }
        }

        /// <summary>
        /// Subscribe to a notification topic
        /// </summary>
        [HttpPost("subscribe")]
        public async Task<ActionResult<MobileApiResponse<object>>> SubscribeToTopic(
            [FromBody] TopicSubscriptionRequest request)
        {
            try
            {
                var success = await _pushNotificationService.SubscribeToTopicAsync(
                    request.DeviceToken,
                    request.Topic
                );

                if (success)
                {
                    return Ok(new MobileApiResponse<object>
                    {
                        Success = true,
                        Message = $"Subscribed to topic: {request.Topic}"
                    });
                }

                return Ok(new MobileApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to subscribe to topic"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to topic");
                return StatusCode(500, new MobileApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while subscribing to topic"
                });
            }
        }

        /// <summary>
        /// Unsubscribe from a notification topic
        /// </summary>
        [HttpPost("unsubscribe")]
        public async Task<ActionResult<MobileApiResponse<object>>> UnsubscribeFromTopic(
            [FromBody] TopicSubscriptionRequest request)
        {
            try
            {
                var success = await _pushNotificationService.UnsubscribeFromTopicAsync(
                    request.DeviceToken,
                    request.Topic
                );

                if (success)
                {
                    return Ok(new MobileApiResponse<object>
                    {
                        Success = true,
                        Message = $"Unsubscribed from topic: {request.Topic}"
                    });
                }

                return Ok(new MobileApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to unsubscribe from topic"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing from topic");
                return StatusCode(500, new MobileApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while unsubscribing from topic"
                });
            }
        }

        /// <summary>
        /// Get notification statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<MobileApiResponse<Dictionary<string, int>>>> GetStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var stats = await _pushNotificationService.GetNotificationStatisticsAsync(
                    startDate,
                    endDate
                );

                return Ok(new MobileApiResponse<Dictionary<string, int>>
                {
                    Success = true,
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notification statistics");
                return StatusCode(500, new MobileApiResponse<Dictionary<string, int>>
                {
                    Success = false,
                    Message = "An error occurred while fetching statistics"
                });
            }
        }

        /// <summary>
        /// Get user's registered devices
        /// </summary>
        [HttpGet("devices")]
        public async Task<ActionResult<MobileApiResponse<List<DeviceTokenDto>>>> GetUserDevices()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized();
                }

                var devices = await _pushNotificationService.GetUserDeviceTokensAsync(userId);
                var deviceDtos = devices.Select(d => new DeviceTokenDto
                {
                    Platform = d.Platform.ToString(),
                    DeviceInfo = d.DeviceInfo,
                    IsActive = d.IsActive,
                    LastUsed = d.LastUsedAt ?? d.CreatedAt,
                    RegisteredAt = d.CreatedAt
                }).ToList();

                return Ok(new MobileApiResponse<List<DeviceTokenDto>>
                {
                    Success = true,
                    Data = deviceDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user devices");
                return StatusCode(500, new MobileApiResponse<List<DeviceTokenDto>>
                {
                    Success = false,
                    Message = "An error occurred while fetching devices"
                });
            }
        }
    }

    public class TestNotificationRequest
    {
        public string? Title { get; set; }
        public string? Body { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class TopicSubscriptionRequest
    {
        public string DeviceToken { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
    }

    public class DeviceTokenDto
    {
        public string Platform { get; set; } = string.Empty;
        public string? DeviceInfo { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastUsed { get; set; }
        public DateTime RegisteredAt { get; set; }
    }
}
