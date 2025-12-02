using Microsoft.AspNetCore.SignalR;
using GlobalDisasterManagement.Hubs;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Interfaces;

namespace GlobalDisasterManagement.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<DisasterAlertHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IHubContext<DisasterAlertHub> hubContext,
            ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendFloodAlertToCity(int cityId, CityFloodPrediction prediction)
        {
            try
            {
                var alert = new
                {
                    Type = "FloodAlert",
                    CityId = cityId,
                    City = prediction.City,
                    Month = prediction.Month,
                    Year = prediction.Year,
                    IsHighRisk = prediction.Prediction == true.ToString(),
                    Message = $"Flood risk alert for {prediction.City} in {prediction.Month} {prediction.Year}",
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group($"City_{cityId}")
                    .SendAsync("ReceiveFloodAlert", alert);

                _logger.LogInformation(
                    "Sent flood alert to city group {CityId}: {City}", 
                    cityId, 
                    prediction.City);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending flood alert to city {CityId}", cityId);
            }
        }

        public async Task SendFloodAlertToLGA(int lgaId, string message)
        {
            try
            {
                var alert = new
                {
                    Type = "LGAAlert",
                    LGAId = lgaId,
                    Message = message,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group($"LGA_{lgaId}")
                    .SendAsync("ReceiveLGAAlert", alert);

                _logger.LogInformation("Sent LGA alert to group {LgaId}", lgaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending LGA alert to {LgaId}", lgaId);
            }
        }

        public async Task BroadcastFloodAlert(string message)
        {
            try
            {
                var alert = new
                {
                    Type = "Broadcast",
                    Message = message,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("ReceiveBroadcast", alert);
                _logger.LogInformation("Broadcast flood alert sent to all clients");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting flood alert");
            }
        }

        public async Task SendModelTrainingUpdate(string status, double? accuracy = null)
        {
            try
            {
                var update = new
                {
                    Type = "ModelTraining",
                    Status = status,
                    Accuracy = accuracy,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("ReceiveModelUpdate", update);
                _logger.LogInformation("Model training update sent: {Status}", status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending model training update");
            }
        }

        public async Task SendDisasterAlert(DisasterIncident incident)
        {
            try
            {
                var alert = new
                {
                    Type = "DisasterIncident",
                    IncidentId = incident.Id,
                    DisasterType = incident.DisasterType.ToString(),
                    Severity = incident.Severity.ToString(),
                    Title = incident.Title,
                    Location = incident.Address,
                    Latitude = incident.Latitude,
                    Longitude = incident.Longitude,
                    Message = $"{incident.Severity} {incident.DisasterType} reported: {incident.Title}",
                    Timestamp = incident.ReportedAt
                };

                // Send to specific groups
                if (incident.CityId.HasValue)
                {
                    await _hubContext.Clients.Group($"City_{incident.CityId}")
                        .SendAsync("ReceiveDisasterAlert", alert);
                }

                if (incident.LGAId.HasValue)
                {
                    await _hubContext.Clients.Group($"LGA_{incident.LGAId}")
                        .SendAsync("ReceiveDisasterAlert", alert);
                }

                // Broadcast critical/catastrophic incidents
                if (incident.Severity >= IncidentSeverity.Critical)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveEmergencyAlert", alert);
                }

                _logger.LogInformation(
                    "Sent disaster alert: {DisasterType} - {Severity} - {Title}",
                    incident.DisasterType,
                    incident.Severity,
                    incident.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending disaster alert for incident {IncidentId}", incident.Id);
            }
        }

        public async Task SendDisasterAlertToCity(int cityId, DisasterAlert alert)
        {
            try
            {
                var notification = new
                {
                    Type = "DisasterAlert",
                    AlertId = alert.Id,
                    DisasterType = alert.DisasterType.ToString(),
                    Severity = alert.Severity.ToString(),
                    Title = alert.Title,
                    Message = alert.Message,
                    RecommendedActions = alert.RecommendedActions,
                    Timestamp = alert.IssuedAt
                };

                await _hubContext.Clients.Group($"City_{cityId}")
                    .SendAsync("ReceiveDisasterAlert", notification);

                _logger.LogInformation(
                    "Sent disaster alert to city {CityId}: {AlertTitle}",
                    cityId,
                    alert.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending disaster alert to city {CityId}", cityId);
            }
        }

        public async Task SendDisasterAlertToRegion(string region, DisasterAlert alert)
        {
            try
            {
                var notification = new
                {
                    Type = "RegionalDisasterAlert",
                    AlertId = alert.Id,
                    DisasterType = alert.DisasterType.ToString(),
                    Severity = alert.Severity.ToString(),
                    Title = alert.Title,
                    Message = alert.Message,
                    Region = region,
                    RecommendedActions = alert.RecommendedActions,
                    Timestamp = alert.IssuedAt
                };

                await _hubContext.Clients.Group($"Region_{region}")
                    .SendAsync("ReceiveRegionalAlert", notification);

                _logger.LogInformation(
                    "Sent regional disaster alert to {Region}: {AlertTitle}",
                    region,
                    alert.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending regional disaster alert to {Region}", region);
            }
        }
    }
}
