using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Abstract;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Text.Json;

namespace GlobalDisasterManagement.Services.Implementation
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly DisasterDbContext _context;
        private readonly ILogger<PushNotificationService> _logger;
        private readonly IStringLocalizer<Resources.SharedResources> _localizer;
        private readonly FirebaseMessaging _messaging;

        public PushNotificationService(
            DisasterDbContext context,
            ILogger<PushNotificationService> logger,
            IConfiguration configuration,
            IStringLocalizer<Resources.SharedResources> localizer)
        {
            _context = context;
            _logger = logger;
            _localizer = localizer;

            // Initialize Firebase Admin SDK
            try
            {
                var firebaseCredentialPath = configuration["Firebase:CredentialPath"];
                if (!string.IsNullOrEmpty(firebaseCredentialPath) && File.Exists(firebaseCredentialPath))
                {
                    if (FirebaseApp.DefaultInstance == null)
                    {
                        FirebaseApp.Create(new AppOptions
                        {
                            Credential = GoogleCredential.FromFile(firebaseCredentialPath)
                        });
                    }
                    _messaging = FirebaseMessaging.DefaultInstance;
                    _logger.LogInformation("Firebase Admin SDK initialized successfully");
                }
                else
                {
                    _logger.LogWarning("Firebase credentials not found. Push notifications will not work.");
                    _messaging = null!;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase Admin SDK");
                _messaging = null!;
            }
        }

        public async Task<DeviceToken> RegisterDeviceAsync(string token, string? userId, DevicePlatform platform, string? deviceInfo = null)
        {
            try
            {
                // Check if token already exists
                var existingToken = await _context.DeviceTokens
                    .FirstOrDefaultAsync(dt => dt.Token == token);

                if (existingToken != null)
                {
                    // Update existing token
                    existingToken.UserId = userId;
                    existingToken.Platform = platform;
                    existingToken.DeviceInfo = deviceInfo;
                    existingToken.IsActive = true;
                    existingToken.UpdatedAt = DateTime.UtcNow;
                    existingToken.LastUsedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Updated device token for user {UserId}", userId);
                    return existingToken;
                }

                // Create new token
                var deviceToken = new DeviceToken
                {
                    Token = token,
                    UserId = userId,
                    Platform = platform,
                    DeviceInfo = deviceInfo,
                    IsActive = true,
                    LastUsedAt = DateTime.UtcNow
                };

                _context.DeviceTokens.Add(deviceToken);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Registered new device token for user {UserId}, platform {Platform}", userId, platform);
                return deviceToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering device token");
                throw;
            }
        }

        public async Task UpdateDeviceLastUsedAsync(string token)
        {
            try
            {
                var deviceToken = await _context.DeviceTokens
                    .FirstOrDefaultAsync(dt => dt.Token == token);

                if (deviceToken != null)
                {
                    deviceToken.LastUsedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating device last used");
            }
        }

        public async Task DeactivateDeviceAsync(string token)
        {
            try
            {
                var deviceToken = await _context.DeviceTokens
                    .FirstOrDefaultAsync(dt => dt.Token == token);

                if (deviceToken != null)
                {
                    deviceToken.IsActive = false;
                    deviceToken.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Deactivated device token");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating device token");
                throw;
            }
        }

        public async Task<bool> SendToDeviceAsync(string deviceToken, string title, string body, string? imageUrl = null, Dictionary<string, string>? data = null, MessagePriority priority = MessagePriority.Normal)
        {
            if (_messaging == null)
            {
                _logger.LogWarning("Firebase not initialized. Cannot send notification.");
                return false;
            }

            try
            {
                var message = BuildMessage(deviceToken, title, body, imageUrl, data, priority);

                var response = await _messaging.SendAsync(message);

                // Save to database
                var notification = new PushNotification
                {
                    Title = title,
                    Body = body,
                    ImageUrl = imageUrl,
                    Data = data != null ? JsonSerializer.Serialize(data) : null,
                    DeviceTokens = deviceToken,
                    Status = NotificationStatus.Sent,
                    SentCount = 1,
                    Priority = priority,
                    SentAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    FcmResponse = response
                };

                _context.PushNotifications.Add(notification);
                await _context.SaveChangesAsync();
                await UpdateDeviceLastUsedAsync(deviceToken);

                _logger.LogInformation("Push notification sent successfully. FCM response: {Response}", response);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification to device");

                // Save failed notification
                var notification = new PushNotification
                {
                    Title = title,
                    Body = body,
                    ImageUrl = imageUrl,
                    Data = data != null ? JsonSerializer.Serialize(data) : null,
                    DeviceTokens = deviceToken,
                    Status = NotificationStatus.Failed,
                    FailedCount = 1,
                    Priority = priority,
                    ErrorMessage = ex.Message
                };

                _context.PushNotifications.Add(notification);
                await _context.SaveChangesAsync();

                return false;
            }
        }

        public async Task<(int successCount, int failureCount)> SendToMultipleDevicesAsync(List<string> deviceTokens, string title, string body, string? imageUrl = null, Dictionary<string, string>? data = null, MessagePriority priority = MessagePriority.Normal)
        {
            if (_messaging == null)
            {
                _logger.LogWarning("Firebase not initialized. Cannot send notifications.");
                return (0, deviceTokens.Count);
            }

            int successCount = 0;
            int failureCount = 0;

            try
            {
                var message = new MulticastMessage
                {
                    Tokens = deviceTokens,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body,
                        ImageUrl = imageUrl
                    },
                    Data = data,
                    Android = new AndroidConfig
                    {
                        Priority = priority == MessagePriority.Urgent ? Priority.High : Priority.Normal,
                        Notification = new AndroidNotification
                        {
                            Sound = "default"
                        }
                    },
                    Apns = new ApnsConfig
                    {
                        Headers = new Dictionary<string, string>
                        {
                            { "apns-priority", priority == MessagePriority.Urgent ? "10" : "5" }
                        },
                        Aps = new Aps
                        {
                            Sound = "default",
                            Badge = 1
                        }
                    }
                };

                var response = await _messaging.SendMulticastAsync(message);
                successCount = response.SuccessCount;
                failureCount = response.FailureCount;

                // Save to database
                var notification = new PushNotification
                {
                    Title = title,
                    Body = body,
                    ImageUrl = imageUrl,
                    Data = data != null ? JsonSerializer.Serialize(data) : null,
                    DeviceTokens = string.Join(",", deviceTokens),
                    Status = failureCount > 0 ? NotificationStatus.Failed : NotificationStatus.Sent,
                    SentCount = successCount,
                    FailedCount = failureCount,
                    Priority = priority,
                    SentAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    FcmResponse = JsonSerializer.Serialize(new { SuccessCount = successCount, FailureCount = failureCount })
                };

                _context.PushNotifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Multicast notification sent. Success: {Success}, Failure: {Failure}", successCount, failureCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending multicast notification");
                failureCount = deviceTokens.Count;
            }

            return (successCount, failureCount);
        }

        public async Task<bool> SendToTopicAsync(string topic, string title, string body, string? imageUrl = null, Dictionary<string, string>? data = null, MessagePriority priority = MessagePriority.Normal)
        {
            if (_messaging == null)
            {
                _logger.LogWarning("Firebase not initialized. Cannot send notification.");
                return false;
            }

            try
            {
                var message = new Message
                {
                    Topic = topic,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body,
                        ImageUrl = imageUrl
                    },
                    Data = data,
                    Android = new AndroidConfig
                    {
                        Priority = priority == MessagePriority.Urgent ? Priority.High : Priority.Normal,
                        Notification = new AndroidNotification
                        {
                            ChannelId = "disaster_alerts",
                            Sound = "default"
                        }
                    },
                    Apns = new ApnsConfig
                    {
                        Headers = new Dictionary<string, string>
                        {
                            { "apns-priority", priority == MessagePriority.Urgent ? "10" : "5" }
                        },
                        Aps = new Aps
                        {
                            Sound = "default",
                            Badge = 1
                        }
                    }
                };

                var response = await _messaging.SendAsync(message);

                // Save to database
                var notification = new PushNotification
                {
                    Title = title,
                    Body = body,
                    ImageUrl = imageUrl,
                    Data = data != null ? JsonSerializer.Serialize(data) : null,
                    Topic = topic,
                    Status = NotificationStatus.Sent,
                    SentCount = 1,
                    Priority = priority,
                    SentAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    FcmResponse = response
                };

                _context.PushNotifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Topic notification sent to {Topic}. Response: {Response}", topic, response);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending topic notification");
                return false;
            }
        }

        public async Task<(int successCount, int failureCount)> SendDisasterAlertNotificationAsync(DisasterAlert alert)
        {
            try
            {
                var title = $"🚨 {alert.Title}";
                var body = $"{alert.Message}\n\nSeverity: {alert.Severity}\nType: {alert.DisasterType}";
                
                var data = new Dictionary<string, string>
                {
                    { "alertId", alert.Id.ToString() },
                    { "severity", alert.Severity.ToString() },
                    { "disasterType", alert.DisasterType.ToString() },
                    { "issuedAt", alert.IssuedAt.ToString("O") },
                    { "action", "view-alert" }
                };

                var priority = alert.Severity switch
                {
                    AlertSeverity.Extreme => MessagePriority.Urgent,
                    AlertSeverity.Emergency => MessagePriority.Urgent,
                    AlertSeverity.Warning => MessagePriority.High,
                    _ => MessagePriority.Normal
                };

                // Send to topic based on disaster type
                var topic = $"{alert.DisasterType.ToString().ToLower()}-alerts";
                var topicSent = await SendToTopicAsync(topic, title, body, null, data, priority);

                // Also send to general disasters topic
                var generalTopicSent = await SendToTopicAsync("disasters", title, body, null, data, priority);

                return (topicSent && generalTopicSent ? 2 : (topicSent || generalTopicSent ? 1 : 0), 
                        topicSent && generalTopicSent ? 0 : (topicSent || generalTopicSent ? 1 : 2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending disaster alert notification");
                return (0, 1);
            }
        }

        public async Task<(int successCount, int failureCount)> SendDisasterAlertNotificationAsync(DisasterAlert alert, string languageCode)
        {
            try
            {
                var culture = new CultureInfo(languageCode);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;

                var localizedType = _localizer[alert.DisasterType.ToString()].Value;
                var localizedSeverity = _localizer[alert.Severity.ToString()].Value;
                
                var title = string.Format(_localizer["PushNotificationTitle"].Value, localizedType);
                var body = string.Format(_localizer["PushNotificationBody"].Value, 
                    localizedType, 
                    alert.AffectedCities ?? "affected area",
                    localizedSeverity);
                
                var data = new Dictionary<string, string>
                {
                    { "alertId", alert.Id.ToString() },
                    { "severity", alert.Severity.ToString() },
                    { "disasterType", alert.DisasterType.ToString() },
                    { "issuedAt", alert.IssuedAt.ToString("O") },
                    { "action", "view-alert" },
                    { "language", languageCode }
                };

                var priority = alert.Severity switch
                {
                    AlertSeverity.Extreme => MessagePriority.Urgent,
                    AlertSeverity.Emergency => MessagePriority.Urgent,
                    AlertSeverity.Warning => MessagePriority.High,
                    _ => MessagePriority.Normal
                };

                // Send to topic based on disaster type and language
                var topic = $"{alert.DisasterType.ToString().ToLower()}-{languageCode}-alerts";
                var topicSent = await SendToTopicAsync(topic, title, body, null, data, priority);

                // Also send to general disasters topic for the language
                var generalTopicSent = await SendToTopicAsync($"disasters-{languageCode}", title, body, null, data, priority);

                return (topicSent && generalTopicSent ? 2 : (topicSent || generalTopicSent ? 1 : 0), 
                        topicSent && generalTopicSent ? 0 : (topicSent || generalTopicSent ? 1 : 2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending localized disaster alert notification");
                return (0, 1);
            }
        }

        public async Task<int> SendBulkDisasterAlertsAsync(DisasterAlert alert, List<string> userIds)
        {
            try
            {
                var deviceTokens = await _context.DeviceTokens
                    .Where(dt => userIds.Contains(dt.UserId!) && dt.IsActive)
                    .Select(dt => dt.Token)
                    .ToListAsync();

                if (!deviceTokens.Any())
                {
                    _logger.LogWarning("No active device tokens found for the specified users");
                    return 0;
                }

                var title = $"🚨 {alert.Title}";
                var body = $"{alert.Message}\n\nSeverity: {alert.Severity}";
                var data = new Dictionary<string, string>
                {
                    { "alertId", alert.Id.ToString() },
                    { "severity", alert.Severity.ToString() },
                    { "disasterType", alert.DisasterType.ToString() },
                    { "action", "view-alert" }
                };

                var priority = alert.Severity switch
                {
                    AlertSeverity.Extreme => MessagePriority.Urgent,
                    AlertSeverity.Emergency => MessagePriority.Urgent,
                    AlertSeverity.Warning => MessagePriority.High,
                    _ => MessagePriority.Normal
                };

                var (successCount, _) = await SendToMultipleDevicesAsync(deviceTokens, title, body, null, data, priority);

                return successCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk disaster alerts");
                return 0;
            }
        }

        public async Task<bool> SendIncidentConfirmationNotificationAsync(DisasterIncident incident, string userId)
        {
            try
            {
                var deviceTokens = await _context.DeviceTokens
                    .Where(dt => dt.UserId == userId && dt.IsActive)
                    .Select(dt => dt.Token)
                    .ToListAsync();

                if (!deviceTokens.Any())
                {
                    return false;
                }

                var title = "✅ Incident Report Confirmed";
                var body = $"Your report of {incident.DisasterType} at {incident.Address} has been confirmed. Report ID: {incident.Id}";
                var data = new Dictionary<string, string>
                {
                    { "incidentId", incident.Id.ToString() },
                    { "disasterType", incident.DisasterType.ToString() },
                    { "action", "view-incident" }
                };

                var (successCount, _) = await SendToMultipleDevicesAsync(deviceTokens, title, body, null, data, MessagePriority.Normal);

                return successCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending incident confirmation notification");
                return false;
            }
        }

        public async Task<bool> SendShelterInfoNotificationAsync(EmergencyShelter shelter, string userId)
        {
            try
            {
                var deviceTokens = await _context.DeviceTokens
                    .Where(dt => dt.UserId == userId && dt.IsActive)
                    .Select(dt => dt.Token)
                    .ToListAsync();

                if (!deviceTokens.Any())
                {
                    return false;
                }

                var title = $"🏠 Emergency Shelter: {shelter.Name}";
                var body = $"{shelter.Address}\n\nCapacity: {shelter.TotalCapacity} (Available: {shelter.AvailableSpaces})\nContact: {shelter.ContactPhone}";
                var data = new Dictionary<string, string>
                {
                    { "shelterId", shelter.Id.ToString() },
                    { "latitude", shelter.Latitude.ToString() },
                    { "longitude", shelter.Longitude.ToString() },
                    { "action", "view-shelter" }
                };

                var (successCount, _) = await SendToMultipleDevicesAsync(deviceTokens, title, body, null, data, MessagePriority.High);

                return successCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending shelter info notification");
                return false;
            }
        }

        public async Task<bool> SubscribeToTopicAsync(string deviceToken, string topic)
        {
            if (_messaging == null)
            {
                return false;
            }

            try
            {
                await _messaging.SubscribeToTopicAsync(new List<string> { deviceToken }, topic);

                // Update device token subscriptions
                var device = await _context.DeviceTokens.FirstOrDefaultAsync(dt => dt.Token == deviceToken);
                if (device != null)
                {
                    var subscriptions = string.IsNullOrEmpty(device.TopicSubscriptions)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(device.TopicSubscriptions) ?? new List<string>();

                    if (!subscriptions.Contains(topic))
                    {
                        subscriptions.Add(topic);
                        device.TopicSubscriptions = JsonSerializer.Serialize(subscriptions);
                        await _context.SaveChangesAsync();
                    }
                }

                _logger.LogInformation("Device subscribed to topic {Topic}", topic);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing device to topic");
                return false;
            }
        }

        public async Task<(int successCount, int failureCount)> SubscribeToTopicAsync(List<string> deviceTokens, string topic)
        {
            if (_messaging == null)
            {
                return (0, deviceTokens.Count);
            }

            try
            {
                var response = await _messaging.SubscribeToTopicAsync(deviceTokens, topic);
                _logger.LogInformation("Subscribed {Success} devices to topic {Topic}", response.SuccessCount, topic);
                return (response.SuccessCount, response.FailureCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing devices to topic");
                return (0, deviceTokens.Count);
            }
        }

        public async Task<bool> UnsubscribeFromTopicAsync(string deviceToken, string topic)
        {
            if (_messaging == null)
            {
                return false;
            }

            try
            {
                await _messaging.UnsubscribeFromTopicAsync(new List<string> { deviceToken }, topic);

                // Update device token subscriptions
                var device = await _context.DeviceTokens.FirstOrDefaultAsync(dt => dt.Token == deviceToken);
                if (device != null && !string.IsNullOrEmpty(device.TopicSubscriptions))
                {
                    var subscriptions = JsonSerializer.Deserialize<List<string>>(device.TopicSubscriptions) ?? new List<string>();
                    subscriptions.Remove(topic);
                    device.TopicSubscriptions = JsonSerializer.Serialize(subscriptions);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Device unsubscribed from topic {Topic}", topic);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing device from topic");
                return false;
            }
        }

        public async Task<(int successCount, int failureCount)> UnsubscribeFromTopicAsync(List<string> deviceTokens, string topic)
        {
            if (_messaging == null)
            {
                return (0, deviceTokens.Count);
            }

            try
            {
                var response = await _messaging.UnsubscribeFromTopicAsync(deviceTokens, topic);
                _logger.LogInformation("Unsubscribed {Success} devices from topic {Topic}", response.SuccessCount, topic);
                return (response.SuccessCount, response.FailureCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing devices from topic");
                return (0, deviceTokens.Count);
            }
        }

        public async Task<Dictionary<string, int>> GetNotificationStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            var notifications = await _context.PushNotifications
                .Where(n => n.CreatedAt >= startDate && n.CreatedAt <= endDate)
                .ToListAsync();

            return new Dictionary<string, int>
            {
                { "Total", notifications.Count },
                { "Sent", notifications.Count(n => n.Status == NotificationStatus.Sent) },
                { "Failed", notifications.Count(n => n.Status == NotificationStatus.Failed) },
                { "Pending", notifications.Count(n => n.Status == NotificationStatus.Pending) },
                { "TotalSent", notifications.Sum(n => n.SentCount) },
                { "TotalFailed", notifications.Sum(n => n.FailedCount) }
            };
        }

        public async Task<List<DeviceToken>> GetUserDeviceTokensAsync(string userId)
        {
            return await _context.DeviceTokens
                .Where(dt => dt.UserId == userId && dt.IsActive)
                .OrderByDescending(dt => dt.LastUsedAt)
                .ToListAsync();
        }

        public async Task<List<DeviceToken>> GetTopicSubscribersAsync(string topic)
        {
            return await _context.DeviceTokens
                .Where(dt => dt.IsActive && dt.TopicSubscriptions != null && dt.TopicSubscriptions.Contains(topic))
                .ToListAsync();
        }

        private Message BuildMessage(string token, string title, string body, string? imageUrl, Dictionary<string, string>? data, MessagePriority priority)
        {
            return new Message
            {
                Token = token,
                Notification = new Notification
                {
                    Title = title,
                    Body = body,
                    ImageUrl = imageUrl
                },
                Data = data,
                Android = new AndroidConfig
                {
                    Priority = priority == MessagePriority.Urgent ? Priority.High : Priority.Normal,
                    Notification = new AndroidNotification
                    {
                        ChannelId = "disaster_alerts",
                        Sound = "default"
                    }
                },
                Apns = new ApnsConfig
                {
                    Headers = new Dictionary<string, string>
                    {
                        { "apns-priority", priority == MessagePriority.Urgent ? "10" : "5" }
                    },
                    Aps = new Aps
                    {
                        Sound = "default",
                        Badge = 1
                    }
                }
            };
        }
    }
}
