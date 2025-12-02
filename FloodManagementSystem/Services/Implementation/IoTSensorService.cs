using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Abstract;
using FloodManagementSystem.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace GlobalDisasterManagement.Services.Implementation
{
    public class IoTSensorService : IIoTSensorService
    {
        private readonly DisasterDbContext _context;
        private readonly ILogger<IoTSensorService> _logger;
        private readonly IStringLocalizer<Resources.SharedResources> _localizer;
        private readonly IHubContext<IoTMonitoringHub> _hubContext;

        public IoTSensorService(
            DisasterDbContext context,
            ILogger<IoTSensorService> logger,
            IStringLocalizer<Resources.SharedResources> localizer,
            IHubContext<IoTMonitoringHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _localizer = localizer;
            _hubContext = hubContext;
        }

        #region Sensor Management

        public async Task<WaterLevelSensor> RegisterWaterLevelSensorAsync(WaterLevelSensor sensor)
        {
            try
            {
                _context.WaterLevelSensors.Add(sensor);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Registered water level sensor {DeviceId} at {Location}", 
                    sensor.DeviceId, sensor.Address);
                return sensor;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering water level sensor {DeviceId}", sensor.DeviceId);
                throw;
            }
        }

        public async Task<RainfallSensor> RegisterRainfallSensorAsync(RainfallSensor sensor)
        {
            try
            {
                _context.RainfallSensors.Add(sensor);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Registered rainfall sensor {DeviceId} at {Location}", 
                    sensor.DeviceId, sensor.Address);
                return sensor;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering rainfall sensor {DeviceId}", sensor.DeviceId);
                throw;
            }
        }

        public async Task<WeatherSensor> RegisterWeatherSensorAsync(WeatherSensor sensor)
        {
            try
            {
                _context.WeatherSensors.Add(sensor);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Registered weather sensor {DeviceId} at {Location}", 
                    sensor.DeviceId, sensor.Address);
                return sensor;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering weather sensor {DeviceId}", sensor.DeviceId);
                throw;
            }
        }

        public async Task<bool> UpdateSensorStatusAsync(Guid sensorId, SensorStatus status)
        {
            try
            {
                var waterSensor = await _context.WaterLevelSensors.FindAsync(sensorId);
                if (waterSensor != null)
                {
                    waterSensor.Status = status;
                    waterSensor.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }

                var rainfallSensor = await _context.RainfallSensors.FindAsync(sensorId);
                if (rainfallSensor != null)
                {
                    rainfallSensor.Status = status;
                    rainfallSensor.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }

                var weatherSensor = await _context.WeatherSensors.FindAsync(sensorId);
                if (weatherSensor != null)
                {
                    weatherSensor.Status = status;
                    weatherSensor.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sensor status for {SensorId}", sensorId);
                return false;
            }
        }

        public async Task<bool> DeactivateSensorAsync(Guid sensorId)
        {
            return await UpdateSensorStatusAsync(sensorId, SensorStatus.Inactive);
        }

        public async Task<IoTSensor?> GetSensorByDeviceIdAsync(string deviceId)
        {
            IoTSensor? sensor = await _context.WaterLevelSensors
                .FirstOrDefaultAsync(s => s.DeviceId == deviceId);

            if (sensor == null)
                sensor = await _context.RainfallSensors
                    .FirstOrDefaultAsync(s => s.DeviceId == deviceId);

            if (sensor == null)
                sensor = await _context.WeatherSensors
                    .FirstOrDefaultAsync(s => s.DeviceId == deviceId);

            return sensor;
        }

        public async Task<List<IoTSensor>> GetActiveSensorsAsync()
        {
            var sensors = new List<IoTSensor>();

            sensors.AddRange(await _context.WaterLevelSensors
                .Where(s => s.Status == SensorStatus.Active)
                .ToListAsync());

            sensors.AddRange(await _context.RainfallSensors
                .Where(s => s.Status == SensorStatus.Active)
                .ToListAsync());

            sensors.AddRange(await _context.WeatherSensors
                .Where(s => s.Status == SensorStatus.Active)
                .ToListAsync());

            return sensors;
        }

        public async Task<List<IoTSensor>> GetSensorsByCityAsync(int cityId)
        {
            var sensors = new List<IoTSensor>();

            sensors.AddRange(await _context.WaterLevelSensors
                .Where(s => s.CityId == cityId)
                .ToListAsync());

            sensors.AddRange(await _context.RainfallSensors
                .Where(s => s.CityId == cityId)
                .ToListAsync());

            sensors.AddRange(await _context.WeatherSensors
                .Where(s => s.CityId == cityId)
                .ToListAsync());

            return sensors;
        }

        public async Task<List<IoTSensor>> GetSensorsByTypeAsync(SensorType type)
        {
            var sensors = new List<IoTSensor>();

            switch (type)
            {
                case SensorType.WaterLevel:
                    sensors.AddRange(await _context.WaterLevelSensors.ToListAsync());
                    break;
                case SensorType.Rainfall:
                    sensors.AddRange(await _context.RainfallSensors.ToListAsync());
                    break;
                case SensorType.Weather:
                    sensors.AddRange(await _context.WeatherSensors.ToListAsync());
                    break;
            }

            return sensors;
        }

        #endregion

        #region Water Level Operations

        public async Task<WaterLevelReading> RecordWaterLevelAsync(string deviceId, double level)
        {
            try
            {
                var sensor = await _context.WaterLevelSensors
                    .FirstOrDefaultAsync(s => s.DeviceId == deviceId);

                if (sensor == null)
                    throw new Exception($"Water level sensor {deviceId} not found");

                // Determine status based on thresholds
                var status = DetermineWaterLevelStatus(level, sensor);

                var reading = new WaterLevelReading
                {
                    SensorId = sensor.Id,
                    Level = level,
                    Timestamp = DateTime.UtcNow,
                    Status = status,
                    IsValid = true
                };

                // Calculate rate of change if we have previous reading
                var lastReading = await _context.WaterLevelReadings
                    .Where(r => r.SensorId == sensor.Id)
                    .OrderByDescending(r => r.Timestamp)
                    .FirstOrDefaultAsync();

                if (lastReading != null)
                {
                    var timeDiff = (reading.Timestamp - lastReading.Timestamp).TotalHours;
                    if (timeDiff > 0)
                    {
                        reading.RateOfChange = (reading.Level - lastReading.Level) / timeDiff;
                    }
                }

                _context.WaterLevelReadings.Add(reading);

                // Update sensor current reading
                sensor.CurrentLevel = level;
                sensor.CurrentReadingTime = DateTime.UtcNow;
                sensor.CurrentStatus = status;
                sensor.LastDataReceivedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Check thresholds and generate alerts if needed
                await CheckWaterLevelThresholdAsync(reading);

                // Broadcast real-time update via SignalR
                await IoTMonitoringHub.BroadcastWaterLevelReading(_hubContext, reading, sensor);

                _logger.LogInformation("Recorded water level {Level}m at sensor {DeviceId}, Status: {Status}", 
                    level, deviceId, status);

                return reading;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording water level for sensor {DeviceId}", deviceId);
                throw;
            }
        }

        private WaterLevelStatus DetermineWaterLevelStatus(double level, WaterLevelSensor sensor)
        {
            if (level >= sensor.CriticalLevel) return WaterLevelStatus.Critical;
            if (level >= sensor.DangerLevel) return WaterLevelStatus.Danger;
            if (level >= sensor.WarningLevel) return WaterLevelStatus.Warning;
            if (level < sensor.NormalLevel) return WaterLevelStatus.BelowNormal;
            return WaterLevelStatus.Normal;
        }

        public async Task<List<WaterLevelReading>> GetWaterLevelHistoryAsync(Guid sensorId, DateTime from, DateTime to)
        {
            return await _context.WaterLevelReadings
                .Where(r => r.SensorId == sensorId && r.Timestamp >= from && r.Timestamp <= to)
                .OrderBy(r => r.Timestamp)
                .ToListAsync();
        }

        public async Task<WaterLevelReading?> GetLatestWaterLevelAsync(Guid sensorId)
        {
            return await _context.WaterLevelReadings
                .Where(r => r.SensorId == sensorId)
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<List<WaterLevelSensor>> GetCriticalWaterLevelSensorsAsync()
        {
            return await _context.WaterLevelSensors
                .Where(s => s.CurrentStatus == WaterLevelStatus.Critical || 
                           s.CurrentStatus == WaterLevelStatus.Danger)
                .Include(s => s.City)
                .Include(s => s.LGA)
                .ToListAsync();
        }

        #endregion

        #region Rainfall Operations

        public async Task<RainfallReading> RecordRainfallAsync(string deviceId, double rainfall, DateTime periodStart, DateTime periodEnd)
        {
            try
            {
                var sensor = await _context.RainfallSensors
                    .FirstOrDefaultAsync(s => s.DeviceId == deviceId);

                if (sensor == null)
                    throw new Exception($"Rainfall sensor {deviceId} not found");

                var intensity = DetermineRainfallIntensity(rainfall, sensor);

                // Calculate cumulative totals
                var hourlyTotal = await CalculateHourlyRainfallAsync(sensor.Id, periodEnd);
                var dailyTotal = await CalculateDailyRainfallAsync(sensor.Id, periodEnd.Date);

                var reading = new RainfallReading
                {
                    SensorId = sensor.Id,
                    Rainfall = rainfall,
                    Timestamp = DateTime.UtcNow,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    Intensity = intensity,
                    HourlyCumulative = hourlyTotal + rainfall,
                    DailyCumulative = dailyTotal + rainfall,
                    IsValid = true
                };

                _context.RainfallReadings.Add(reading);

                // Update sensor current reading
                sensor.CurrentRainfall = rainfall;
                sensor.HourlyRainfall = reading.HourlyCumulative;
                sensor.DailyRainfall = reading.DailyCumulative;
                sensor.CurrentReadingTime = DateTime.UtcNow;
                sensor.CurrentIntensity = intensity;
                sensor.LastDataReceivedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Check thresholds
                await CheckRainfallThresholdAsync(reading);

                // Broadcast real-time update via SignalR
                await IoTMonitoringHub.BroadcastRainfallReading(_hubContext, reading, sensor);

                _logger.LogInformation("Recorded rainfall {Rainfall}mm at sensor {DeviceId}, Intensity: {Intensity}", 
                    rainfall, deviceId, intensity);

                return reading;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording rainfall for sensor {DeviceId}", deviceId);
                throw;
            }
        }

        private RainfallIntensity DetermineRainfallIntensity(double rainfall, RainfallSensor sensor)
        {
            if (rainfall == 0) return RainfallIntensity.None;
            if (rainfall < sensor.LightRainThreshold) return RainfallIntensity.Light;
            if (rainfall < sensor.ModerateRainThreshold) return RainfallIntensity.Moderate;
            if (rainfall < sensor.HeavyRainThreshold) return RainfallIntensity.Heavy;
            if (rainfall < sensor.VeryHeavyRainThreshold) return RainfallIntensity.VeryHeavy;
            return RainfallIntensity.Extreme;
        }

        private async Task<double> CalculateHourlyRainfallAsync(Guid sensorId, DateTime time)
        {
            var hourStart = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0);
            return await _context.RainfallReadings
                .Where(r => r.SensorId == sensorId && r.Timestamp >= hourStart && r.Timestamp < time)
                .SumAsync(r => r.Rainfall);
        }

        private async Task<double> CalculateDailyRainfallAsync(Guid sensorId, DateTime date)
        {
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);
            return await _context.RainfallReadings
                .Where(r => r.SensorId == sensorId && r.Timestamp >= dayStart && r.Timestamp < dayEnd)
                .SumAsync(r => r.Rainfall);
        }

        public async Task<List<RainfallReading>> GetRainfallHistoryAsync(Guid sensorId, DateTime from, DateTime to)
        {
            return await _context.RainfallReadings
                .Where(r => r.SensorId == sensorId && r.Timestamp >= from && r.Timestamp <= to)
                .OrderBy(r => r.Timestamp)
                .ToListAsync();
        }

        public async Task<RainfallReading?> GetLatestRainfallAsync(Guid sensorId)
        {
            return await _context.RainfallReadings
                .Where(r => r.SensorId == sensorId)
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<double> GetDailyRainfallAsync(Guid sensorId, DateTime date)
        {
            return await CalculateDailyRainfallAsync(sensorId, date);
        }

        #endregion

        #region Weather Operations

        public async Task<WeatherReading> RecordWeatherDataAsync(string deviceId, WeatherReading reading)
        {
            try
            {
                var sensor = await _context.WeatherSensors
                    .FirstOrDefaultAsync(s => s.DeviceId == deviceId);

                if (sensor == null)
                    throw new Exception($"Weather sensor {deviceId} not found");

                reading.SensorId = sensor.Id;
                reading.Timestamp = DateTime.UtcNow;

                _context.WeatherReadings.Add(reading);

                // Update sensor current reading
                sensor.CurrentTemperature = reading.Temperature;
                sensor.CurrentHumidity = reading.Humidity;
                sensor.CurrentPressure = reading.Pressure;
                sensor.CurrentWindSpeed = reading.WindSpeed;
                sensor.CurrentWindDirection = reading.WindDirection;
                sensor.CurrentReadingTime = DateTime.UtcNow;
                sensor.LastDataReceivedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Broadcast real-time update via SignalR
                await IoTMonitoringHub.BroadcastWeatherReading(_hubContext, reading, sensor);

                _logger.LogInformation("Recorded weather data at sensor {DeviceId}: Temp={Temp}°C, Humidity={Humidity}%", 
                    deviceId, reading.Temperature, reading.Humidity);

                return reading;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording weather data for sensor {DeviceId}", deviceId);
                throw;
            }
        }

        public async Task<List<WeatherReading>> GetWeatherHistoryAsync(Guid sensorId, DateTime from, DateTime to)
        {
            return await _context.WeatherReadings
                .Where(r => r.SensorId == sensorId && r.Timestamp >= from && r.Timestamp <= to)
                .OrderBy(r => r.Timestamp)
                .ToListAsync();
        }

        public async Task<WeatherReading?> GetLatestWeatherAsync(Guid sensorId)
        {
            return await _context.WeatherReadings
                .Where(r => r.SensorId == sensorId)
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<Dictionary<string, object>> GetWeatherSummaryAsync(int cityId)
        {
            var sensors = await _context.WeatherSensors
                .Where(s => s.CityId == cityId && s.Status == SensorStatus.Active)
                .ToListAsync();

            var avgTemp = sensors.Average(s => s.CurrentTemperature ?? 0);
            var avgHumidity = sensors.Average(s => s.CurrentHumidity ?? 0);
            var avgPressure = sensors.Average(s => s.CurrentPressure ?? 0);

            return new Dictionary<string, object>
            {
                { "avgTemperature", avgTemp },
                { "avgHumidity", avgHumidity },
                { "avgPressure", avgPressure },
                { "sensorCount", sensors.Count }
            };
        }

        #endregion

        #region Alert Detection

        public async Task<List<DisasterAlert>> CheckThresholdsAndGenerateAlertsAsync()
        {
            var alerts = new List<DisasterAlert>();

            // Check water level sensors
            var criticalWaterSensors = await GetCriticalWaterLevelSensorsAsync();
            foreach (var sensor in criticalWaterSensors)
            {
                if (!sensor.AlertsEnabled) continue;

                var alert = new DisasterAlert
                {
                    DisasterType = DisasterType.Flood,
                    Title = $"High Water Level Alert - {sensor.Name}",
                    Message = $"Water level at {sensor.Name} has reached {sensor.CurrentStatus} status: {sensor.CurrentLevel}m",
                    Severity = MapWaterLevelToSeverity(sensor.CurrentStatus ?? WaterLevelStatus.Normal),
                    Status = AlertStatus.Active,
                    IssuedAt = DateTime.UtcNow,
                    AffectedCities = sensor.City?.Name
                };

                _context.DisasterAlerts.Add(alert);
                alerts.Add(alert);
            }

            // Check rainfall sensors
            var rainfallSensors = await _context.RainfallSensors
                .Where(s => s.Status == SensorStatus.Active && s.AlertsEnabled)
                .Where(s => s.CurrentIntensity == RainfallIntensity.VeryHeavy || s.CurrentIntensity == RainfallIntensity.Extreme)
                .Include(s => s.City)
                .ToListAsync();

            foreach (var sensor in rainfallSensors)
            {
                var alert = new DisasterAlert
                {
                    DisasterType = DisasterType.Flood,
                    Title = $"Heavy Rainfall Alert - {sensor.Name}",
                    Message = $"{sensor.CurrentIntensity} rainfall detected at {sensor.Name}: {sensor.DailyRainfall}mm today",
                    Severity = MapRainfallToSeverity(sensor.CurrentIntensity ?? RainfallIntensity.None),
                    Status = AlertStatus.Active,
                    IssuedAt = DateTime.UtcNow,
                    AffectedCities = sensor.City?.Name
                };

                _context.DisasterAlerts.Add(alert);
                alerts.Add(alert);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Generated {Count} alerts from IoT sensor threshold checks", alerts.Count);

            return alerts;
        }

        public async Task<bool> CheckWaterLevelThresholdAsync(WaterLevelReading reading)
        {
            if (reading.Status == WaterLevelStatus.Critical || reading.Status == WaterLevelStatus.Danger)
            {
                reading.AlertTriggered = true;
                reading.AlertTriggeredAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> CheckRainfallThresholdAsync(RainfallReading reading)
        {
            if (reading.Intensity == RainfallIntensity.VeryHeavy || reading.Intensity == RainfallIntensity.Extreme)
            {
                reading.AlertTriggered = true;
                reading.AlertTriggeredAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        private AlertSeverity MapWaterLevelToSeverity(WaterLevelStatus status)
        {
            return status switch
            {
                WaterLevelStatus.Critical => AlertSeverity.Extreme,
                WaterLevelStatus.Danger => AlertSeverity.Emergency,
                WaterLevelStatus.Warning => AlertSeverity.Warning,
                _ => AlertSeverity.Advisory
            };
        }

        private AlertSeverity MapRainfallToSeverity(RainfallIntensity intensity)
        {
            return intensity switch
            {
                RainfallIntensity.Extreme => AlertSeverity.Extreme,
                RainfallIntensity.VeryHeavy => AlertSeverity.Emergency,
                RainfallIntensity.Heavy => AlertSeverity.Warning,
                _ => AlertSeverity.Advisory
            };
        }

        #endregion

        #region Analytics

        public async Task<Dictionary<string, object>> GetSensorStatisticsAsync()
        {
            var waterSensors = await _context.WaterLevelSensors.CountAsync();
            var rainfallSensors = await _context.RainfallSensors.CountAsync();
            var weatherSensors = await _context.WeatherSensors.CountAsync();
            var activeSensors = await _context.WaterLevelSensors.CountAsync(s => s.Status == SensorStatus.Active)
                               + await _context.RainfallSensors.CountAsync(s => s.Status == SensorStatus.Active)
                               + await _context.WeatherSensors.CountAsync(s => s.Status == SensorStatus.Active);
            var offlineSensors = await _context.WaterLevelSensors.CountAsync(s => s.Status == SensorStatus.Offline)
                                + await _context.RainfallSensors.CountAsync(s => s.Status == SensorStatus.Offline)
                                + await _context.WeatherSensors.CountAsync(s => s.Status == SensorStatus.Offline);

            return new Dictionary<string, object>
            {
                { "totalSensors", waterSensors + rainfallSensors + weatherSensors },
                { "waterLevelSensors", waterSensors },
                { "rainfallSensors", rainfallSensors },
                { "weatherSensors", weatherSensors },
                { "activeSensors", activeSensors },
                { "offlineSensors", offlineSensors }
            };
        }

        public async Task<List<object>> GetSensorHealthReportAsync()
        {
            var report = new List<object>();

            // Water level sensors
            var waterSensors = await _context.WaterLevelSensors.ToListAsync();
            foreach (var sensor in waterSensors)
            {
                var lastReading = await GetLatestWaterLevelAsync(sensor.Id);
                report.Add(new
                {
                    sensorId = sensor.Id,
                    deviceId = sensor.DeviceId,
                    name = sensor.Name,
                    type = "WaterLevel",
                    status = sensor.Status.ToString(),
                    batteryLevel = sensor.BatteryLevel,
                    lastCommunication = sensor.LastCommunicationDate,
                    lastReading = lastReading?.Timestamp,
                    isHealthy = sensor.Status == SensorStatus.Active && 
                               (DateTime.UtcNow - sensor.LastCommunicationDate).TotalHours < 24
                });
            }

            // Rainfall sensors
            var rainfallSensors = await _context.RainfallSensors.ToListAsync();
            foreach (var sensor in rainfallSensors)
            {
                var lastReading = await GetLatestRainfallAsync(sensor.Id);
                report.Add(new
                {
                    sensorId = sensor.Id,
                    deviceId = sensor.DeviceId,
                    name = sensor.Name,
                    type = "Rainfall",
                    status = sensor.Status.ToString(),
                    batteryLevel = sensor.BatteryLevel,
                    lastCommunication = sensor.LastCommunicationDate,
                    lastReading = lastReading?.Timestamp,
                    isHealthy = sensor.Status == SensorStatus.Active && 
                               (DateTime.UtcNow - sensor.LastCommunicationDate).TotalHours < 24
                });
            }

            return report;
        }

        public async Task<Dictionary<string, double>> GetAverageReadingsAsync(Guid sensorId, DateTime from, DateTime to)
        {
            var result = new Dictionary<string, double>();

            // Check if it's a water level sensor
            var waterSensor = await _context.WaterLevelSensors.FindAsync(sensorId);
            if (waterSensor != null)
            {
                var readings = await GetWaterLevelHistoryAsync(sensorId, from, to);
                result["avgLevel"] = readings.Any() ? readings.Average(r => r.Level) : 0;
                result["maxLevel"] = readings.Any() ? readings.Max(r => r.Level) : 0;
                result["minLevel"] = readings.Any() ? readings.Min(r => r.Level) : 0;
                return result;
            }

            // Check if it's a rainfall sensor
            var rainfallSensor = await _context.RainfallSensors.FindAsync(sensorId);
            if (rainfallSensor != null)
            {
                var readings = await GetRainfallHistoryAsync(sensorId, from, to);
                result["totalRainfall"] = readings.Sum(r => r.Rainfall);
                result["avgRainfall"] = readings.Any() ? readings.Average(r => r.Rainfall) : 0;
                result["maxRainfall"] = readings.Any() ? readings.Max(r => r.Rainfall) : 0;
                return result;
            }

            // Check if it's a weather sensor
            var weatherSensor = await _context.WeatherSensors.FindAsync(sensorId);
            if (weatherSensor != null)
            {
                var readings = await GetWeatherHistoryAsync(sensorId, from, to);
                result["avgTemperature"] = readings.Any() && readings.Any(r => r.Temperature.HasValue) 
                    ? readings.Where(r => r.Temperature.HasValue).Average(r => r.Temperature!.Value) : 0;
                result["avgHumidity"] = readings.Any() && readings.Any(r => r.Humidity.HasValue)
                    ? readings.Where(r => r.Humidity.HasValue).Average(r => r.Humidity!.Value) : 0;
                result["avgPressure"] = readings.Any() && readings.Any(r => r.Pressure.HasValue)
                    ? readings.Where(r => r.Pressure.HasValue).Average(r => r.Pressure!.Value) : 0;
                return result;
            }

            return result;
        }

        #endregion
    }
}
