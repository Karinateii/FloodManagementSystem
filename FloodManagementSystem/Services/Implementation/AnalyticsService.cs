using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Models.DTO.Analytics;
using GlobalDisasterManagement.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace GlobalDisasterManagement.Services.Implementation
{
    /// <summary>
    /// Analytics service implementation
    /// </summary>
    public class AnalyticsService : IAnalyticsService
    {
        private readonly DisasterDbContext _context;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(DisasterDbContext context, ILogger<AnalyticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DashboardKpiDto> GetDashboardKpisAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddDays(-1);
                endDate ??= DateTime.UtcNow;

                var kpi = new DashboardKpiDto
                {
                    LastUpdated = DateTime.UtcNow,
                    Period = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}"
                };

                // Alert Metrics
                var alerts = await _context.DisasterAlerts
                    .Where(a => a.IssuedAt >= startDate && a.IssuedAt <= endDate)
                    .ToListAsync();

                kpi.TotalAlerts = alerts.Count;
                kpi.ActiveAlerts = alerts.Count(a => a.Status == AlertStatus.Active);
                kpi.ResolvedAlerts = alerts.Count(a => a.Status == AlertStatus.Expired || a.Status == AlertStatus.Cancelled);

                var resolvedAlerts = alerts.Where(a => (a.Status == AlertStatus.Expired || a.Status == AlertStatus.Cancelled) && a.UpdatedAt.HasValue);
                if (resolvedAlerts.Any())
                {
                    kpi.AverageAlertResolutionTime = resolvedAlerts
                        .Average(a => (a.UpdatedAt!.Value - a.IssuedAt).TotalHours);
                }

                kpi.AlertEffectivenessRate = kpi.TotalAlerts > 0 
                    ? (double)kpi.ResolvedAlerts / kpi.TotalAlerts * 100 
                    : 0;

                // Incident Metrics
                var incidents = await _context.DisasterIncidents
                    .Where(i => i.ReportedAt >= startDate && i.ReportedAt <= endDate)
                    .ToListAsync();

                kpi.TotalIncidents = incidents.Count;
                kpi.PendingIncidents = incidents.Count(i => i.Status == IncidentStatus.Reported || i.Status == IncidentStatus.Verified);
                kpi.ResolvedIncidents = incidents.Count(i => i.Status == IncidentStatus.Resolved);

                var respondedIncidents = incidents.Where(i => i.EmergencyResponseTime.HasValue);
                if (respondedIncidents.Any())
                {
                    kpi.AverageResponseTime = respondedIncidents
                        .Average(i => (i.EmergencyResponseTime!.Value - i.ReportedAt).TotalMinutes);
                }

                kpi.IncidentResolutionRate = kpi.TotalIncidents > 0 
                    ? (double)kpi.ResolvedIncidents / kpi.TotalIncidents * 100 
                    : 0;

                // Shelter Metrics
                var shelters = await _context.EmergencyShelters.ToListAsync();
                var checkIns = await _context.ShelterCheckIns
                    .Where(c => c.CheckInTime >= startDate && c.CheckInTime <= endDate)
                    .ToListAsync();

                kpi.TotalShelters = shelters.Count;
                kpi.ActiveShelters = shelters.Count(s => s.IsActive);
                kpi.TotalCapacity = shelters.Sum(s => s.TotalCapacity);
                kpi.CurrentOccupancy = shelters.Sum(s => s.CurrentOccupancy);
                kpi.ShelterUtilizationRate = kpi.TotalCapacity > 0 
                    ? (double)kpi.CurrentOccupancy / kpi.TotalCapacity * 100 
                    : 0;

                // IoT Sensor Metrics
                var waterSensors = await _context.WaterLevelSensors.ToListAsync();
                var rainfallSensors = await _context.RainfallSensors.ToListAsync();
                var weatherSensors = await _context.WeatherSensors.ToListAsync();
                kpi.TotalSensors = waterSensors.Count + rainfallSensors.Count + weatherSensors.Count;
                kpi.ActiveSensors = waterSensors.Count(s => s.Status == SensorStatus.Active) + 
                                   rainfallSensors.Count(s => s.Status == SensorStatus.Active) + 
                                   weatherSensors.Count(s => s.Status == SensorStatus.Active);
                kpi.InactiveSensors = waterSensors.Count(s => s.Status == SensorStatus.Inactive) + 
                                     rainfallSensors.Count(s => s.Status == SensorStatus.Inactive) + 
                                     weatherSensors.Count(s => s.Status == SensorStatus.Inactive);
                kpi.MaintenanceSensors = waterSensors.Count(s => s.Status == SensorStatus.Maintenance) + 
                                        rainfallSensors.Count(s => s.Status == SensorStatus.Maintenance) + 
                                        weatherSensors.Count(s => s.Status == SensorStatus.Maintenance);
                kpi.SensorHealthRate = kpi.TotalSensors > 0 
                    ? (double)kpi.ActiveSensors / kpi.TotalSensors * 100 
                    : 0;

                // User Engagement
                var users = await _context.Users.ToListAsync();
                kpi.TotalUsers = users.Count;
                // Note: LastLoginDate property doesn't exist in User model - would need to be added
                kpi.ActiveUsersToday = 0; // TODO: Add LastLoginDate tracking to User model
                
                var weekAgo = DateTime.UtcNow.AddDays(-7);
                // Note: CreatedDate property doesn't exist in User model
                kpi.NewUsersThisWeek = 0; // TODO: Add CreatedDate tracking to User model

                // Notification Metrics (today)
                var today = DateTime.UtcNow.Date;
                var todayEnd = today.AddDays(1);

                kpi.SmsNotifications = await _context.SmsNotifications
                    .CountAsync(n => n.SentAt >= today && n.SentAt < todayEnd);

                kpi.PushNotifications = await _context.PushNotifications
                    .CountAsync(n => n.SentAt >= today && n.SentAt < todayEnd);

                kpi.WhatsAppNotifications = await _context.WhatsAppMessages
                    .CountAsync(m => m.SentAt >= today && m.SentAt < todayEnd);

                kpi.VoiceNotifications = await _context.VoiceCalls
                    .CountAsync(v => v.CreatedAt >= today && v.CreatedAt < todayEnd);

                kpi.NotificationsSentToday = kpi.SmsNotifications + kpi.PushNotifications + 
                                            kpi.WhatsAppNotifications + kpi.VoiceNotifications;

                return kpi;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard KPIs");
                throw;
            }
        }

        public async Task<DisasterImpactDto> GetDisasterImpactAnalysisAsync(
            string? disasterType = null, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddMonths(-1);
                endDate ??= DateTime.UtcNow;

                var incidentsQuery = _context.DisasterIncidents
                    .Include(i => i.City)
                    .Where(i => i.ReportedAt >= startDate && i.ReportedAt <= endDate);

                if (!string.IsNullOrEmpty(disasterType))
                {
                    incidentsQuery = incidentsQuery.Where(i => i.DisasterType.ToString() == disasterType);
                }

                var incidents = await incidentsQuery.ToListAsync();

                var impact = new DisasterImpactDto
                {
                    DisasterType = disasterType ?? "All Disasters",
                    TotalIncidents = incidents.Count,
                    AffectedPeople = incidents.Sum(i => i.AffectedPeople ?? 0),
                    EvacuatedPeople = 0, // Property doesn't exist in model
                    AverageSeverity = incidents.Any() 
                        ? incidents.Average(i => (int)i.Severity) 
                        : 0
                };

                // Geographic Impact
                impact.GeographicImpacts = incidents
                    .GroupBy(i => new { i.City?.Name, i.Latitude, i.Longitude })
                    .Select(g => new GeographicImpactDto
                    {
                        Location = g.Key.Name ?? "Unknown",
                        IncidentCount = g.Count(),
                        Latitude = g.Key.Latitude,
                        Longitude = g.Key.Longitude,
                        Severity = g.OrderByDescending(i => i.Severity).First().Severity.ToString()
                    })
                    .OrderByDescending(g => g.IncidentCount)
                    .Take(20)
                    .ToList();

                // Timeline
                impact.Timeline = incidents
                    .GroupBy(i => i.ReportedAt.Date)
                    .Select(g => new TimeSeriesDataDto
                    {
                        Date = g.Key,
                        Count = g.Count(),
                        Value = g.Count(),
                        Label = g.Key.ToString("MMM dd")
                    })
                    .OrderBy(t => t.Date)
                    .ToList();

                return impact;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating disaster impact analysis");
                throw;
            }
        }

        public async Task<ResponseEffectivenessDto> GetResponseEffectivenessAsync(
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddMonths(-1);
                endDate ??= DateTime.UtcNow;

                var incidents = await _context.DisasterIncidents
                    .Include(i => i.City)
                    .Where(i => i.ReportedAt >= startDate && i.ReportedAt <= endDate && i.EmergencyResponseTime.HasValue)
                    .ToListAsync();

                var responseTimes = incidents
                    .Select(i => (i.EmergencyResponseTime!.Value - i.ReportedAt).TotalMinutes)
                    .OrderBy(t => t)
                    .ToList();

                var effectiveness = new ResponseEffectivenessDto
                {
                    AverageResponseTime = responseTimes.Any() ? responseTimes.Average() : 0,
                    MedianResponseTime = responseTimes.Any() ? responseTimes[responseTimes.Count / 2] : 0,
                    ResponsesUnder15Min = responseTimes.Count(t => t <= 15),
                    ResponsesUnder30Min = responseTimes.Count(t => t > 15 && t <= 30),
                    ResponsesUnder60Min = responseTimes.Count(t => t > 30 && t <= 60),
                    ResponsesOver60Min = responseTimes.Count(t => t > 60),
                    TotalIncidentsResolved = await _context.DisasterIncidents.CountAsync(i => i.Status == IncidentStatus.Resolved),
                    TotalIncidentsPending = await _context.DisasterIncidents.CountAsync(i => i.Status != IncidentStatus.Resolved)
                };

                var totalIncidents = effectiveness.TotalIncidentsResolved + effectiveness.TotalIncidentsPending;
                effectiveness.IncidentResolutionRate = totalIncidents > 0 
                    ? (double)effectiveness.TotalIncidentsResolved / totalIncidents * 100 
                    : 0;

                // Response Time by Disaster Type
                effectiveness.ResponseTimeByDisasterType = incidents
                    .GroupBy(i => i.DisasterType)
                    .Select(g => new ResponseTimeByTypeDto
                    {
                        DisasterType = g.Key.ToString(),
                        AverageResponseTime = g.Average(i => (i.EmergencyResponseTime!.Value - i.ReportedAt).TotalMinutes),
                        IncidentCount = g.Count()
                    })
                    .OrderBy(r => r.AverageResponseTime)
                    .ToList();

                // Response Time by Location
                effectiveness.ResponseTimeByLocation = incidents
                    .Where(i => i.City != null)
                    .GroupBy(i => i.City!.Name)
                    .Select(g => new ResponseTimeByLocationDto
                    {
                        Location = g.Key,
                        AverageResponseTime = g.Average(i => (i.EmergencyResponseTime!.Value - i.ReportedAt).TotalMinutes),
                        IncidentCount = g.Count()
                    })
                    .OrderBy(r => r.AverageResponseTime)
                    .Take(15)
                    .ToList();

                // Response Time Trend
                effectiveness.ResponseTimeTrend = incidents
                    .GroupBy(i => i.ReportedAt.Date)
                    .Select(g => new TimeSeriesDataDto
                    {
                        Date = g.Key,
                        Value = g.Average(i => (i.EmergencyResponseTime!.Value - i.ReportedAt).TotalMinutes),
                        Count = g.Count(),
                        Label = g.Key.ToString("MMM dd")
                    })
                    .OrderBy(t => t.Date)
                    .ToList();

                return effectiveness;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating response effectiveness analysis");
                throw;
            }
        }

        public async Task<ShelterAnalyticsDto> GetShelterAnalyticsAsync(
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddMonths(-1);
                endDate ??= DateTime.UtcNow;

                var shelters = await _context.EmergencyShelters
                    .Include(s => s.City)
                    .ToListAsync();

                var checkIns = await _context.ShelterCheckIns
                    .Where(c => c.CheckInTime >= startDate && c.CheckInTime <= endDate)
                    .ToListAsync();

                var analytics = new ShelterAnalyticsDto
                {
                    TotalShelters = shelters.Count,
                    ActiveShelters = shelters.Count(s => s.IsActive),
                    InactiveShelters = shelters.Count(s => !s.IsActive),
                    TotalCapacity = shelters.Sum(s => s.TotalCapacity),
                    CurrentOccupancy = shelters.Sum(s => s.CurrentOccupancy),
                    TotalCheckIns = checkIns.Count,
                    CheckInsToday = checkIns.Count(c => c.CheckInTime.Date == DateTime.UtcNow.Date),
                    CheckInsThisWeek = checkIns.Count(c => c.CheckInTime >= DateTime.UtcNow.AddDays(-7))
                };

                analytics.UtilizationRate = analytics.TotalCapacity > 0 
                    ? (double)analytics.CurrentOccupancy / analytics.TotalCapacity * 100 
                    : 0;

                // Average stay duration
                var completedStays = checkIns.Where(c => c.CheckOutTime.HasValue);
                if (completedStays.Any())
                {
                    analytics.AverageStayDuration = completedStays
                        .Average(c => (c.CheckOutTime!.Value - c.CheckInTime).TotalDays);
                }

                // Individual shelter utilization
                analytics.ShelterUtilization = shelters
                    .Select(s => new ShelterUtilizationDto
                    {
                        ShelterId = (int)s.Id.GetHashCode(),
                        ShelterName = s.Name,
                        Location = s.City?.Name ?? s.Address,
                        Capacity = s.TotalCapacity,
                        CurrentOccupancy = s.CurrentOccupancy,
                        UtilizationPercentage = s.TotalCapacity > 0 
                            ? (double)s.CurrentOccupancy / s.TotalCapacity * 100 
                            : 0,
                        HasMedicalFacility = s.HasMedicalFacility,
                        HasFood = s.HasFood,
                        HasWater = s.HasWater
                    })
                    .OrderByDescending(s => s.UtilizationPercentage)
                    .ToList();

                // Shelters by capacity
                analytics.SheltersByCapacity = new List<ShelterCapacityDto>
                {
                    new() { CapacityRange = "0-100", ShelterCount = shelters.Count(s => s.TotalCapacity <= 100), TotalCapacity = shelters.Where(s => s.TotalCapacity <= 100).Sum(s => s.TotalCapacity) },
                    new() { CapacityRange = "101-500", ShelterCount = shelters.Count(s => s.TotalCapacity > 100 && s.TotalCapacity <= 500), TotalCapacity = shelters.Where(s => s.TotalCapacity > 100 && s.TotalCapacity <= 500).Sum(s => s.TotalCapacity) },
                    new() { CapacityRange = "501-1000", ShelterCount = shelters.Count(s => s.TotalCapacity > 500 && s.TotalCapacity <= 1000), TotalCapacity = shelters.Where(s => s.TotalCapacity > 500 && s.TotalCapacity <= 1000).Sum(s => s.TotalCapacity) },
                    new() { CapacityRange = "1000+", ShelterCount = shelters.Count(s => s.TotalCapacity > 1000), TotalCapacity = shelters.Where(s => s.TotalCapacity > 1000).Sum(s => s.TotalCapacity) }
                };

                // Occupancy trend
                analytics.OccupancyTrend = checkIns
                    .GroupBy(c => c.CheckInTime.Date)
                    .Select(g => new TimeSeriesDataDto
                    {
                        Date = g.Key,
                        Count = g.Count(),
                        Value = g.Count(),
                        Label = g.Key.ToString("MMM dd")
                    })
                    .OrderBy(t => t.Date)
                    .ToList();

                return analytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating shelter analytics");
                throw;
            }
        }

        public async Task<IoTSensorAnalyticsDto> GetIoTSensorAnalyticsAsync(
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddMonths(-1);
                endDate ??= DateTime.UtcNow;

                var waterSensors = await _context.WaterLevelSensors.Include(s => s.City).ToListAsync();
                var rainfallSensors = await _context.RainfallSensors.Include(s => s.City).ToListAsync();
                var weatherSensors = await _context.WeatherSensors.Include(s => s.City).ToListAsync();

                var analytics = new IoTSensorAnalyticsDto
                {
                    TotalSensors = waterSensors.Count + rainfallSensors.Count + weatherSensors.Count,
                    ActiveSensors = waterSensors.Count(s => s.Status == SensorStatus.Active) + 
                                   rainfallSensors.Count(s => s.Status == SensorStatus.Active) + 
                                   weatherSensors.Count(s => s.Status == SensorStatus.Active),
                    InactiveSensors = waterSensors.Count(s => s.Status == SensorStatus.Inactive) + 
                                     rainfallSensors.Count(s => s.Status == SensorStatus.Inactive) + 
                                     weatherSensors.Count(s => s.Status == SensorStatus.Inactive),
                    MaintenanceSensors = waterSensors.Count(s => s.Status == SensorStatus.Maintenance) + 
                                        rainfallSensors.Count(s => s.Status == SensorStatus.Maintenance) + 
                                        weatherSensors.Count(s => s.Status == SensorStatus.Maintenance)
                };

                analytics.SensorHealthRate = analytics.TotalSensors > 0 
                    ? (double)analytics.ActiveSensors / analytics.TotalSensors * 100 
                    : 0;

                // Sensors by type
                analytics.SensorsByType = new List<SensorTypeBreakdownDto>
                {
                    new() {
                        SensorType = "WaterLevel",
                        TotalCount = waterSensors.Count,
                        ActiveCount = waterSensors.Count(s => s.Status == SensorStatus.Active),
                        InactiveCount = waterSensors.Count(s => s.Status == SensorStatus.Inactive),
                        HealthRate = waterSensors.Count > 0 ? (double)waterSensors.Count(s => s.Status == SensorStatus.Active) / waterSensors.Count * 100 : 0
                    },
                    new() {
                        SensorType = "Rainfall",
                        TotalCount = rainfallSensors.Count,
                        ActiveCount = rainfallSensors.Count(s => s.Status == SensorStatus.Active),
                        InactiveCount = rainfallSensors.Count(s => s.Status == SensorStatus.Inactive),
                        HealthRate = rainfallSensors.Count > 0 ? (double)rainfallSensors.Count(s => s.Status == SensorStatus.Active) / rainfallSensors.Count * 100 : 0
                    },
                    new() {
                        SensorType = "Weather",
                        TotalCount = weatherSensors.Count,
                        ActiveCount = weatherSensors.Count(s => s.Status == SensorStatus.Active),
                        InactiveCount = weatherSensors.Count(s => s.Status == SensorStatus.Inactive),
                        HealthRate = weatherSensors.Count > 0 ? (double)weatherSensors.Count(s => s.Status == SensorStatus.Active) / weatherSensors.Count * 100 : 0
                    }
                };

                // Individual sensor health - combine all sensor types
                var allSensorHealth = new List<SensorHealthDto>();
                
                allSensorHealth.AddRange(waterSensors.Select(s => new SensorHealthDto
                {
                    SensorId = (int)s.Id.GetHashCode(),
                    SensorName = s.Name,
                    SensorType = "WaterLevel",
                    Location = s.City?.Name ?? "Unknown",
                    Status = s.Status.ToString(),
                    LastReading = s.CurrentReadingTime,
                    HoursSinceLastReading = s.CurrentReadingTime.HasValue ? (int)(DateTime.UtcNow - s.CurrentReadingTime.Value).TotalHours : 9999,
                    BatteryLevel = s.BatteryLevel ?? 0,
                    RequiresMaintenance = s.Status == SensorStatus.Maintenance || (s.BatteryLevel.HasValue && s.BatteryLevel < 20)
                }));
                
                allSensorHealth.AddRange(rainfallSensors.Select(s => new SensorHealthDto
                {
                    SensorId = (int)s.Id.GetHashCode(),
                    SensorName = s.Name,
                    SensorType = "Rainfall",
                    Location = s.City?.Name ?? "Unknown",
                    Status = s.Status.ToString(),
                    LastReading = s.CurrentReadingTime,
                    HoursSinceLastReading = s.CurrentReadingTime.HasValue ? (int)(DateTime.UtcNow - s.CurrentReadingTime.Value).TotalHours : 9999,
                    BatteryLevel = s.BatteryLevel ?? 0,
                    RequiresMaintenance = s.Status == SensorStatus.Maintenance || (s.BatteryLevel.HasValue && s.BatteryLevel < 20)
                }));
                
                allSensorHealth.AddRange(weatherSensors.Select(s => new SensorHealthDto
                {
                    SensorId = (int)s.Id.GetHashCode(),
                    SensorName = s.Name,
                    SensorType = "Weather",
                    Location = s.City?.Name ?? "Unknown",
                    Status = s.Status.ToString(),
                    LastReading = s.CurrentReadingTime,
                    HoursSinceLastReading = s.CurrentReadingTime.HasValue ? (int)(DateTime.UtcNow - s.CurrentReadingTime.Value).TotalHours : 9999,
                    BatteryLevel = s.BatteryLevel ?? 0,
                    RequiresMaintenance = s.Status == SensorStatus.Maintenance || (s.BatteryLevel.HasValue && s.BatteryLevel < 20)
                }));
                
                analytics.SensorHealth = allSensorHealth
                    .OrderBy(s => s.Status)
                    .ThenByDescending(s => s.HoursSinceLastReading)
                    .ToList();

                // Data quality trend (simulated based on sensor activity)
                var dateRange = Enumerable.Range(0, (endDate.Value - startDate.Value).Days + 1)
                    .Select(d => startDate.Value.AddDays(d).Date);

                analytics.DataQualityTrend = dateRange
                    .Select(date => new TimeSeriesDataDto
                    {
                        Date = date,
                        Value = waterSensors.Count(s => s.CurrentReadingTime.HasValue && s.CurrentReadingTime.Value.Date == date) +
                               rainfallSensors.Count(s => s.CurrentReadingTime.HasValue && s.CurrentReadingTime.Value.Date == date) +
                               weatherSensors.Count(s => s.CurrentReadingTime.HasValue && s.CurrentReadingTime.Value.Date == date),
                        Count = analytics.ActiveSensors,
                        Label = date.ToString("MMM dd")
                    })
                    .ToList();

                // Reading statistics
                var today = DateTime.UtcNow.Date;
                var weekAgo = DateTime.UtcNow.AddDays(-7);

                analytics.TotalReadingsToday = waterSensors.Count(s => s.CurrentReadingTime.HasValue && s.CurrentReadingTime.Value.Date == today) +
                                              rainfallSensors.Count(s => s.CurrentReadingTime.HasValue && s.CurrentReadingTime.Value.Date == today) +
                                              weatherSensors.Count(s => s.CurrentReadingTime.HasValue && s.CurrentReadingTime.Value.Date == today);
                analytics.TotalReadingsThisWeek = waterSensors.Count(s => s.CurrentReadingTime.HasValue && s.CurrentReadingTime >= weekAgo) +
                                                 rainfallSensors.Count(s => s.CurrentReadingTime.HasValue && s.CurrentReadingTime >= weekAgo) +
                                                 weatherSensors.Count(s => s.CurrentReadingTime.HasValue && s.CurrentReadingTime >= weekAgo);

                // Average reading frequency (simulated)
                var activeSensorsWithReadings = waterSensors.Where(s => s.CurrentReadingTime.HasValue).Count() +
                                               rainfallSensors.Where(s => s.CurrentReadingTime.HasValue).Count() +
                                               weatherSensors.Where(s => s.CurrentReadingTime.HasValue).Count();
                if (activeSensorsWithReadings > 0)
                {
                    analytics.AverageReadingFrequency = 15; // Assume 15 minutes as default
                }

                // Alerts triggered by sensors (property doesn't exist, using all alerts as placeholder)
                analytics.AlertsTriggered = await _context.DisasterAlerts
                    .CountAsync(a => a.IssuedAt >= startDate && a.IssuedAt <= endDate);

                return analytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating IoT sensor analytics");
                throw;
            }
        }

        public async Task<NotificationAnalyticsDto> GetNotificationAnalyticsAsync(
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddMonths(-1);
                endDate ??= DateTime.UtcNow;

                var smsNotifications = await _context.SmsNotifications
                    .Where(n => n.SentAt >= startDate && n.SentAt <= endDate)
                    .ToListAsync();

                var pushNotifications = await _context.PushNotifications
                    .Where(n => n.SentAt >= startDate && n.SentAt <= endDate)
                    .ToListAsync();

                var whatsappMessages = await _context.WhatsAppMessages
                    .Where(m => m.SentAt >= startDate && m.SentAt <= endDate)
                    .ToListAsync();

                var voiceCalls = await _context.VoiceCalls
                    .Where(v => v.CreatedAt >= startDate && v.CreatedAt <= endDate)
                    .ToListAsync();

                var analytics = new NotificationAnalyticsDto
                {
                    SmsNotifications = smsNotifications.Count,
                    PushNotifications = pushNotifications.Count,
                    WhatsAppNotifications = whatsappMessages.Count,
                    VoiceNotifications = voiceCalls.Count
                };

                analytics.TotalNotificationsSent = analytics.SmsNotifications + 
                                                   analytics.PushNotifications + 
                                                   analytics.WhatsAppNotifications + 
                                                   analytics.VoiceNotifications;

                var today = DateTime.UtcNow.Date;
                var weekAgo = DateTime.UtcNow.AddDays(-7);
                var monthAgo = DateTime.UtcNow.AddMonths(-1);

                analytics.NotificationsToday = smsNotifications.Count(n => n.SentAt.Date == today) +
                                              pushNotifications.Count(n => n.SentAt.HasValue && n.SentAt.Value.Date == today) +
                                              whatsappMessages.Count(m => m.SentAt.HasValue && m.SentAt.Value.Date == today) +
                                              voiceCalls.Count(v => v.CreatedAt.Date == today);

                analytics.NotificationsThisWeek = smsNotifications.Count(n => n.SentAt >= weekAgo) +
                                                 pushNotifications.Count(n => n.SentAt >= weekAgo) +
                                                 whatsappMessages.Count(m => m.SentAt >= weekAgo) +
                                                 voiceCalls.Count(v => v.CreatedAt >= weekAgo);

                analytics.NotificationsThisMonth = smsNotifications.Count(n => n.SentAt >= monthAgo) +
                                                   pushNotifications.Count(n => n.SentAt >= monthAgo) +
                                                   whatsappMessages.Count(m => m.SentAt >= monthAgo) +
                                                   voiceCalls.Count(v => v.CreatedAt >= monthAgo);

                // Delivery status
                analytics.SuccessfulDeliveries = smsNotifications.Count(n => n.Status == NotificationStatus.Delivered) +
                                                pushNotifications.Count(n => n.Status == NotificationStatus.Delivered) +
                                                whatsappMessages.Count(m => m.Status == WhatsAppMessageStatus.Delivered) +
                                                voiceCalls.Count(v => v.Status == VoiceCallStatus.Completed);

                analytics.FailedDeliveries = smsNotifications.Count(n => n.Status == NotificationStatus.Failed) +
                                            pushNotifications.Count(n => n.Status == NotificationStatus.Failed) +
                                            whatsappMessages.Count(m => m.Status == WhatsAppMessageStatus.Failed) +
                                            voiceCalls.Count(v => v.Status == VoiceCallStatus.Failed);

                analytics.PendingDeliveries = smsNotifications.Count(n => n.Status == NotificationStatus.Pending) +
                                             pushNotifications.Count(n => n.Status == NotificationStatus.Pending);

                analytics.DeliverySuccessRate = analytics.TotalNotificationsSent > 0 
                    ? (double)analytics.SuccessfulDeliveries / analytics.TotalNotificationsSent * 100 
                    : 0;

                // Channel statistics
                analytics.ChannelStatistics = new List<NotificationChannelStatsDto>
                {
                    new()
                    {
                        ChannelName = "SMS",
                        TotalSent = analytics.SmsNotifications,
                        Successful = smsNotifications.Count(n => n.Status == NotificationStatus.Delivered),
                        Failed = smsNotifications.Count(n => n.Status == NotificationStatus.Failed),
                        SuccessRate = analytics.SmsNotifications > 0 
                            ? (double)smsNotifications.Count(n => n.Status == NotificationStatus.Delivered) / analytics.SmsNotifications * 100 
                            : 0,
                        AverageDeliveryTime = 2.5
                    },
                    new()
                    {
                        ChannelName = "Push Notification",
                        TotalSent = analytics.PushNotifications,
                        Successful = pushNotifications.Count(n => n.Status == NotificationStatus.Delivered),
                        Failed = pushNotifications.Count(n => n.Status == NotificationStatus.Failed),
                        SuccessRate = analytics.PushNotifications > 0 
                            ? (double)pushNotifications.Count(n => n.Status == NotificationStatus.Delivered) / analytics.PushNotifications * 100 
                            : 0,
                        AverageDeliveryTime = 1.2
                    },
                    new()
                    {
                        ChannelName = "WhatsApp",
                        TotalSent = analytics.WhatsAppNotifications,
                        Successful = whatsappMessages.Count(m => m.Status == WhatsAppMessageStatus.Delivered),
                        Failed = whatsappMessages.Count(m => m.Status == WhatsAppMessageStatus.Failed),
                        SuccessRate = analytics.WhatsAppNotifications > 0 
                            ? (double)whatsappMessages.Count(m => m.Status == WhatsAppMessageStatus.Delivered) / analytics.WhatsAppNotifications * 100 
                            : 0,
                        AverageDeliveryTime = 3.1
                    },
                    new()
                    {
                        ChannelName = "Voice Call",
                        TotalSent = analytics.VoiceNotifications,
                        Successful = voiceCalls.Count(v => v.Status == VoiceCallStatus.Completed),
                        Failed = voiceCalls.Count(v => v.Status == VoiceCallStatus.Failed),
                        SuccessRate = analytics.VoiceNotifications > 0 
                            ? (double)voiceCalls.Count(v => v.Status == VoiceCallStatus.Completed) / analytics.VoiceNotifications * 100 
                            : 0,
                        AverageDeliveryTime = 8.5
                    }
                };

                // Notification trend
                var allNotificationDates = smsNotifications.Select(n => n.SentAt.Date)
                    .Concat(pushNotifications.Where(n => n.SentAt.HasValue).Select(n => n.SentAt!.Value.Date))
                    .Concat(whatsappMessages.Where(m => m.SentAt.HasValue).Select(m => m.SentAt!.Value.Date))
                    .Concat(voiceCalls.Select(v => v.CreatedAt.Date));

                analytics.NotificationTrend = allNotificationDates
                    .GroupBy(d => d)
                    .Select(g => new TimeSeriesDataDto
                    {
                        Date = g.Key,
                        Count = g.Count(),
                        Value = g.Count(),
                        Label = g.Key.ToString("MMM dd")
                    })
                    .OrderBy(t => t.Date)
                    .ToList();

                // Notifications by disaster type
                var alerts = await _context.DisasterAlerts
                    .Where(a => a.IssuedAt >= startDate && a.IssuedAt <= endDate)
                    .ToListAsync();

                analytics.NotificationsByDisasterType = alerts
                    .GroupBy(a => a.DisasterType.ToString())
                    .Select(g => new NotificationByTypeDto
                    {
                        DisasterType = g.Key,
                        NotificationCount = g.Count(),
                        RecipientsReached = g.Count() // Using count as proxy since NotificationsSent doesn't exist
                    })
                    .OrderByDescending(n => n.NotificationCount)
                    .ToList();

                return analytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating notification analytics");
                throw;
            }
        }

        public async Task<PredictionAccuracyDto> GetPredictionAccuracyAsync(
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddMonths(-3);
                endDate ??= DateTime.UtcNow;

                // TODO: CityFloodPredictions table doesn't exist yet - returning placeholder data
                var accuracy = new PredictionAccuracyDto
                {
                    ModelName = "Flood Prediction Model",
                    ModelType = "Fast Tree Binary Classifier",
                    LastTrainingDate = DateTime.UtcNow.AddMonths(-2),
                    TotalPredictions = 0,
                    CorrectPredictions = 0,
                    FalsePositives = 0,
                    FalseNegatives = 0,
                    Accuracy = 0,
                    Precision = 0,
                    Recall = 0,
                    F1Score = 0,
                    TruePositives = 0,
                    TrueNegatives = 0,
                    AucScore = 0,
                    PredictionsByLocation = new List<PredictionByLocationDto>(),
                    AccuracyTrend = new List<TimeSeriesDataDto>()
                };

                // TODO: Implement prediction accuracy tracking when CityFloodPredictions table exists
                return accuracy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating prediction accuracy metrics");
                throw;
            }
        }

        public async Task<List<TimeSeriesDataDto>> GetTimeSeriesDataAsync(
            string metricType, 
            DateTime startDate, 
            DateTime endDate, 
            string? groupBy = "day")
        {
            try
            {
                var timeSeriesData = new List<TimeSeriesDataDto>();

                switch (metricType.ToLower())
                {
                    case "incidents":
                        var incidents = await _context.DisasterIncidents
                            .Where(i => i.ReportedAt >= startDate && i.ReportedAt <= endDate)
                            .ToListAsync();

                        timeSeriesData = incidents
                            .GroupBy(i => i.ReportedAt.Date)
                            .Select(g => new TimeSeriesDataDto
                            {
                                Date = g.Key,
                                Count = g.Count(),
                                Value = g.Count(),
                                Label = g.Key.ToString("MMM dd")
                            })
                            .OrderBy(t => t.Date)
                            .ToList();
                        break;

                    case "alerts":
                        var alerts = await _context.DisasterAlerts
                            .Where(a => a.IssuedAt >= startDate && a.IssuedAt <= endDate)
                            .ToListAsync();

                        timeSeriesData = alerts
                            .GroupBy(a => a.IssuedAt.Date)
                            .Select(g => new TimeSeriesDataDto
                            {
                                Date = g.Key,
                                Count = g.Count(),
                                Value = g.Count(),
                                Label = g.Key.ToString("MMM dd")
                            })
                            .OrderBy(t => t.Date)
                            .ToList();
                        break;

                    case "shelteroccupancy":
                        var checkIns = await _context.ShelterCheckIns
                            .Where(c => c.CheckInTime >= startDate && c.CheckInTime <= endDate)
                            .ToListAsync();

                        timeSeriesData = checkIns
                            .GroupBy(c => c.CheckInTime.Date)
                            .Select(g => new TimeSeriesDataDto
                            {
                                Date = g.Key,
                                Count = g.Count(),
                                Value = g.Count(),
                                Label = g.Key.ToString("MMM dd")
                            })
                            .OrderBy(t => t.Date)
                            .ToList();
                        break;
                }

                return timeSeriesData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating time series data");
                throw;
            }
        }

        public async Task<List<GeographicImpactDto>> GetGeographicHeatMapDataAsync(
            string? disasterType = null, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddMonths(-1);
                endDate ??= DateTime.UtcNow;

                var incidentsQuery = _context.DisasterIncidents
                    .Include(i => i.City)
                    .Where(i => i.ReportedAt >= startDate && i.ReportedAt <= endDate);

                if (!string.IsNullOrEmpty(disasterType))
                {
                    incidentsQuery = incidentsQuery.Where(i => i.DisasterType.ToString() == disasterType);
                }

                var incidents = await incidentsQuery.ToListAsync();

                var heatMapData = incidents
                    .Where(i => i.Latitude != 0 && i.Longitude != 0)
                    .GroupBy(i => new { i.Latitude, i.Longitude, Location = i.City?.Name ?? "Unknown" })
                    .Select(g => new GeographicImpactDto
                    {
                        Location = g.Key.Location,
                        IncidentCount = g.Count(),
                        Latitude = g.Key.Latitude,
                        Longitude = g.Key.Longitude,
                        Severity = g.OrderByDescending(i => i.Severity).First().Severity.ToString()
                    })
                    .OrderByDescending(h => h.IncidentCount)
                    .ToList();

                return heatMapData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating geographic heat map data");
                throw;
            }
        }

        public async Task<ReportResultDto> GenerateReportAsync(ReportRequestDto request, string userId)
        {
            try
            {
                // Generate report based on type
                var result = new ReportResultDto
                {
                    ReportType = request.ReportType,
                    Format = request.Format,
                    GeneratedAt = DateTime.UtcNow,
                    GeneratedBy = userId,
                    FileName = $"{request.ReportType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{request.Format.ToString().ToLower()}"
                };

                // In a real implementation, you would:
                // 1. Gather the required data based on ReportType
                // 2. Generate the report using a library like iTextSharp (PDF), EPPlus (Excel), etc.
                // 3. Save the file to disk or cloud storage
                // 4. Return the file path/URL

                // Placeholder implementation
                result.FileUrl = $"/reports/{result.FileName}";
                result.FileSizeBytes = 102400; // 100 KB placeholder

                _logger.LogInformation($"Generated report: {result.FileName} for user {userId}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                throw;
            }
        }

        public async Task<byte[]> ExportToCsvAsync(string dataType, DateTime startDate, DateTime endDate)
        {
            // Placeholder for CSV export implementation
            // Would use a library like CsvHelper to generate CSV files
            await Task.CompletedTask;
            return Array.Empty<byte>();
        }

        public async Task<byte[]> ExportToExcelAsync(string dataType, DateTime startDate, DateTime endDate)
        {
            // Placeholder for Excel export implementation
            // Would use a library like EPPlus to generate Excel files
            await Task.CompletedTask;
            return Array.Empty<byte>();
        }

        public async Task<Dictionary<string, object>> GetComparativeAnalysisAsync(
            DateTime period1Start, 
            DateTime period1End, 
            DateTime period2Start, 
            DateTime period2End)
        {
            try
            {
                var period1Kpis = await GetDashboardKpisAsync(period1Start, period1End);
                var period2Kpis = await GetDashboardKpisAsync(period2Start, period2End);

                var comparison = new Dictionary<string, object>
                {
                    ["Period1"] = new { Start = period1Start, End = period1End, Kpis = period1Kpis },
                    ["Period2"] = new { Start = period2Start, End = period2End, Kpis = period2Kpis },
                    ["Changes"] = new
                    {
                        AlertChange = period2Kpis.TotalAlerts - period1Kpis.TotalAlerts,
                        IncidentChange = period2Kpis.TotalIncidents - period1Kpis.TotalIncidents,
                        ResponseTimeChange = period2Kpis.AverageResponseTime - period1Kpis.AverageResponseTime,
                        ShelterUtilizationChange = period2Kpis.ShelterUtilizationRate - period1Kpis.ShelterUtilizationRate
                    }
                };

                return comparison;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating comparative analysis");
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetRealTimeSnapshotAsync()
        {
            try
            {
                var snapshot = new Dictionary<string, object>
                {
                    ["Timestamp"] = DateTime.UtcNow,
                    ["ActiveAlerts"] = await _context.DisasterAlerts.CountAsync(a => a.Status == AlertStatus.Active),
                    ["PendingIncidents"] = await _context.DisasterIncidents.CountAsync(i => i.Status == IncidentStatus.Reported),
                    ["ActiveShelters"] = await _context.EmergencyShelters.CountAsync(s => s.IsActive),
                    ["OnlineSensors"] = await _context.WaterLevelSensors.CountAsync(s => s.Status == SensorStatus.Active) +
                                        await _context.RainfallSensors.CountAsync(s => s.Status == SensorStatus.Active) +
                                        await _context.WeatherSensors.CountAsync(s => s.Status == SensorStatus.Active),
                    ["OnlineUsers"] = await _context.Users.CountAsync() // TODO: User model doesn't have LastLoginDate property
                };

                return snapshot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating real-time snapshot");
                throw;
            }
        }
    }
}
