using Microsoft.AspNetCore.SignalR;
using GlobalDisasterManagement.Models;
using System.Collections.Concurrent;

namespace FloodManagementSystem.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time IoT sensor monitoring and data broadcasting
    /// </summary>
    public class IoTMonitoringHub : Hub
    {
        private static readonly ConcurrentDictionary<string, HashSet<string>> _citySubscriptions = new();
        private static readonly ConcurrentDictionary<string, HashSet<string>> _sensorSubscriptions = new();
        private readonly ILogger<IoTMonitoringHub> _logger;

        public IoTMonitoringHub(ILogger<IoTMonitoringHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Called when a client connects to the hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            _logger.LogInformation("Client connected: {ConnectionId}", connectionId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a client disconnects from the hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            
            // Remove from all subscriptions
            foreach (var cityId in _citySubscriptions.Keys)
            {
                if (_citySubscriptions.TryGetValue(cityId, out var connections))
                {
                    connections.Remove(connectionId);
                }
            }

            foreach (var sensorId in _sensorSubscriptions.Keys)
            {
                if (_sensorSubscriptions.TryGetValue(sensorId, out var connections))
                {
                    connections.Remove(connectionId);
                }
            }

            _logger.LogInformation("Client disconnected: {ConnectionId}", connectionId);
            await base.OnDisconnectedAsync(exception);
        }

        #region Subscription Management

        /// <summary>
        /// Subscribe to real-time updates for a specific city
        /// </summary>
        public async Task SubscribeToCity(int cityId)
        {
            var connectionId = Context.ConnectionId;
            var groupName = $"city_{cityId}";

            await Groups.AddToGroupAsync(connectionId, groupName);

            _citySubscriptions.AddOrUpdate(
                groupName,
                new HashSet<string> { connectionId },
                (key, existing) => { existing.Add(connectionId); return existing; }
            );

            _logger.LogInformation("Client {ConnectionId} subscribed to city {CityId}", connectionId, cityId);
            await Clients.Caller.SendAsync("SubscriptionConfirmed", "city", cityId);
        }

        /// <summary>
        /// Unsubscribe from city updates
        /// </summary>
        public async Task UnsubscribeFromCity(int cityId)
        {
            var connectionId = Context.ConnectionId;
            var groupName = $"city_{cityId}";

            await Groups.RemoveFromGroupAsync(connectionId, groupName);

            if (_citySubscriptions.TryGetValue(groupName, out var connections))
            {
                connections.Remove(connectionId);
            }

            _logger.LogInformation("Client {ConnectionId} unsubscribed from city {CityId}", connectionId, cityId);
        }

        /// <summary>
        /// Subscribe to real-time updates for a specific sensor
        /// </summary>
        public async Task SubscribeToSensor(string sensorId)
        {
            var connectionId = Context.ConnectionId;
            var groupName = $"sensor_{sensorId}";

            await Groups.AddToGroupAsync(connectionId, groupName);

            _sensorSubscriptions.AddOrUpdate(
                groupName,
                new HashSet<string> { connectionId },
                (key, existing) => { existing.Add(connectionId); return existing; }
            );

            _logger.LogInformation("Client {ConnectionId} subscribed to sensor {SensorId}", connectionId, sensorId);
            await Clients.Caller.SendAsync("SubscriptionConfirmed", "sensor", sensorId);
        }

        /// <summary>
        /// Unsubscribe from sensor updates
        /// </summary>
        public async Task UnsubscribeFromSensor(string sensorId)
        {
            var connectionId = Context.ConnectionId;
            var groupName = $"sensor_{sensorId}";

            await Groups.RemoveFromGroupAsync(connectionId, groupName);

            if (_sensorSubscriptions.TryGetValue(groupName, out var connections))
            {
                connections.Remove(connectionId);
            }

            _logger.LogInformation("Client {ConnectionId} unsubscribed from sensor {SensorId}", connectionId, sensorId);
        }

        /// <summary>
        /// Subscribe to all sensor updates (admin/monitoring dashboard)
        /// </summary>
        public async Task SubscribeToAllSensors()
        {
            var connectionId = Context.ConnectionId;
            await Groups.AddToGroupAsync(connectionId, "all_sensors");
            _logger.LogInformation("Client {ConnectionId} subscribed to all sensors", connectionId);
            await Clients.Caller.SendAsync("SubscriptionConfirmed", "all_sensors", "all");
        }

        /// <summary>
        /// Unsubscribe from all sensor updates
        /// </summary>
        public async Task UnsubscribeFromAllSensors()
        {
            var connectionId = Context.ConnectionId;
            await Groups.RemoveFromGroupAsync(connectionId, "all_sensors");
            _logger.LogInformation("Client {ConnectionId} unsubscribed from all sensors", connectionId);
        }

        /// <summary>
        /// Subscribe to updates for a specific sensor type
        /// </summary>
        public async Task SubscribeToSensorType(int sensorType)
        {
            var connectionId = Context.ConnectionId;
            var groupName = $"sensor_type_{sensorType}";
            await Groups.AddToGroupAsync(connectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} subscribed to sensor type {SensorType}", connectionId, sensorType);
            await Clients.Caller.SendAsync("SubscriptionConfirmed", "sensor_type", sensorType);
        }

        /// <summary>
        /// Unsubscribe from sensor type updates
        /// </summary>
        public async Task UnsubscribeFromSensorType(int sensorType)
        {
            var connectionId = Context.ConnectionId;
            var groupName = $"sensor_type_{sensorType}";
            await Groups.RemoveFromGroupAsync(connectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} unsubscribed from sensor type {SensorType}", connectionId, sensorType);
        }

        #endregion

        #region Ping/Heartbeat

        /// <summary>
        /// Heartbeat method to keep connection alive
        /// </summary>
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
        }

        #endregion

        #region Static Broadcast Methods (Called by IoTSensorService)

        /// <summary>
        /// Broadcast water level reading to subscribed clients
        /// </summary>
        public static async Task BroadcastWaterLevelReading(
            IHubContext<IoTMonitoringHub> hubContext,
            WaterLevelReading reading,
            WaterLevelSensor sensor)
        {
            var data = new
            {
                SensorId = sensor.Id,
                DeviceId = sensor.DeviceId,
                SensorName = sensor.Name,
                Location = new { sensor.Latitude, sensor.Longitude },
                CityId = sensor.CityId,
                WaterBody = sensor.WaterBodyName,
                Reading = new
                {
                    reading.Level,
                    Status = reading.Status.ToString(),
                    reading.RateOfChange,
                    reading.AlertTriggered,
                    reading.Timestamp
                },
                Thresholds = new
                {
                    sensor.NormalLevel,
                    sensor.WarningLevel,
                    sensor.DangerLevel,
                    sensor.CriticalLevel
                }
            };

            // Broadcast to sensor-specific subscribers
            await hubContext.Clients.Group($"sensor_{sensor.Id}")
                .SendAsync("WaterLevelUpdate", data);

            // Broadcast to city subscribers
            if (sensor.CityId.HasValue)
            {
                await hubContext.Clients.Group($"city_{sensor.CityId.Value}")
                    .SendAsync("WaterLevelUpdate", data);
            }

            // Broadcast to all sensors group
            await hubContext.Clients.Group("all_sensors")
                .SendAsync("WaterLevelUpdate", data);
        }

        /// <summary>
        /// Broadcast rainfall reading to subscribed clients
        /// </summary>
        public static async Task BroadcastRainfallReading(
            IHubContext<IoTMonitoringHub> hubContext,
            RainfallReading reading,
            RainfallSensor sensor)
        {
            var data = new
            {
                SensorId = sensor.Id,
                DeviceId = sensor.DeviceId,
                SensorName = sensor.Name,
                Location = new { sensor.Latitude, sensor.Longitude },
                CityId = sensor.CityId,
                Reading = new
                {
                    reading.Rainfall,
                    Intensity = reading.Intensity.ToString(),
                    reading.HourlyCumulative,
                    reading.DailyCumulative,
                    reading.MonthlyCumulative,
                    reading.AlertTriggered,
                    reading.Timestamp
                },
                Thresholds = new
                {
                    sensor.LightRainThreshold,
                    sensor.ModerateRainThreshold,
                    sensor.HeavyRainThreshold,
                    sensor.VeryHeavyRainThreshold
                }
            };

            // Broadcast to sensor-specific subscribers
            await hubContext.Clients.Group($"sensor_{sensor.Id}")
                .SendAsync("RainfallUpdate", data);

            // Broadcast to city subscribers
            if (sensor.CityId.HasValue)
            {
                await hubContext.Clients.Group($"city_{sensor.CityId.Value}")
                    .SendAsync("RainfallUpdate", data);
            }

            // Broadcast to all sensors group
            await hubContext.Clients.Group("all_sensors")
                .SendAsync("RainfallUpdate", data);
        }

        /// <summary>
        /// Broadcast weather reading to subscribed clients
        /// </summary>
        public static async Task BroadcastWeatherReading(
            IHubContext<IoTMonitoringHub> hubContext,
            WeatherReading reading,
            WeatherSensor sensor)
        {
            var data = new
            {
                SensorId = sensor.Id,
                DeviceId = sensor.DeviceId,
                SensorName = sensor.Name,
                Location = new { sensor.Latitude, sensor.Longitude },
                CityId = sensor.CityId,
                Reading = new
                {
                    reading.Temperature,
                    reading.FeelsLike,
                    reading.Humidity,
                    reading.Pressure,
                    reading.SeaLevelPressure,
                    reading.WindSpeed,
                    reading.WindDirection,
                    reading.WindGust,
                    reading.Visibility,
                    reading.CloudCover,
                    reading.UVIndex,
                    reading.SolarRadiation,
                    Condition = reading.Condition?.ToString(),
                    reading.Timestamp
                }
            };

            // Broadcast to sensor-specific subscribers
            await hubContext.Clients.Group($"sensor_{sensor.Id}")
                .SendAsync("WeatherUpdate", data);

            // Broadcast to city subscribers
            if (sensor.CityId.HasValue)
            {
                await hubContext.Clients.Group($"city_{sensor.CityId.Value}")
                    .SendAsync("WeatherUpdate", data);
            }

            // Broadcast to all sensors group
            await hubContext.Clients.Group("all_sensors")
                .SendAsync("WeatherUpdate", data);
        }

        /// <summary>
        /// Broadcast sensor status change to subscribed clients
        /// </summary>
        public static async Task BroadcastSensorStatusChange(
            IHubContext<IoTMonitoringHub> hubContext,
            int sensorId,
            string deviceId,
            SensorStatus oldStatus,
            SensorStatus newStatus)
        {
            var data = new
            {
                SensorId = sensorId,
                DeviceId = deviceId,
                OldStatus = oldStatus.ToString(),
                NewStatus = newStatus.ToString(),
                Timestamp = DateTime.UtcNow
            };

            // Broadcast to sensor-specific subscribers
            await hubContext.Clients.Group($"sensor_{sensorId}")
                .SendAsync("SensorStatusChanged", data);

            // Broadcast to all sensors group
            await hubContext.Clients.Group("all_sensors")
                .SendAsync("SensorStatusChanged", data);
        }

        /// <summary>
        /// Broadcast alert notification to subscribed clients
        /// </summary>
        public static async Task BroadcastAlert(
            IHubContext<IoTMonitoringHub> hubContext,
            DisasterAlert alert,
            int? cityId = null)
        {
            var data = new
            {
                alert.Id,
                alert.Title,
                alert.Message,
                Severity = alert.Severity.ToString(),
                DisasterType = alert.DisasterType.ToString(),
                Status = alert.Status.ToString(),
                alert.AffectedCities,
                alert.AffectedLGAs,
                alert.IssuedAt,
                alert.ExpiresAt
            };

            // Broadcast to city subscribers
            if (cityId.HasValue)
            {
                await hubContext.Clients.Group($"city_{cityId.Value}")
                    .SendAsync("DisasterAlert", data);
            }

            // Broadcast to all sensors group (monitoring dashboard)
            await hubContext.Clients.Group("all_sensors")
                .SendAsync("DisasterAlert", data);

            // Also broadcast to the main disaster alert hub if it exists
            await hubContext.Clients.All.SendAsync("AlertCreated", data);
        }

        /// <summary>
        /// Broadcast sensor health status to monitoring clients
        /// </summary>
        public static async Task BroadcastSensorHealth(
            IHubContext<IoTMonitoringHub> hubContext,
            object healthData)
        {
            await hubContext.Clients.Group("all_sensors")
                .SendAsync("SensorHealthUpdate", healthData);
        }

        #endregion
    }
}
