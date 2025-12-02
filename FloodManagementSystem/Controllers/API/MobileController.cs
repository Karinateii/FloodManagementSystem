using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Models.DTO.Mobile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlobalDisasterManagement.Controllers.API
{
    [Authorize]
    [Route("api/mobile")]
    [ApiController]
    public class MobileController : ControllerBase
    {
        private readonly DisasterDbContext _context;
        private readonly ILogger<MobileController> _logger;
        private readonly IWebHostEnvironment _environment;

        public MobileController(
            DisasterDbContext context,
            ILogger<MobileController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Get dashboard summary for mobile
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<MobileApiResponse<MobileDashboardDto>>> GetDashboard()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return Unauthorized();
                }

                var activeAlerts = await _context.DisasterAlerts
                    .Where(a => a.Status == AlertStatus.Active)
                    .OrderByDescending(a => a.Severity)
                    .Take(5)
                    .Select(a => new MobileAlertDto
                    {
                        Id = (int)a.Id.GetHashCode(), // Convert Guid to int for mobile
                        Title = a.Title,
                        Message = a.Message,
                        Severity = a.Severity,
                        DisasterType = a.DisasterType,
                        CreatedAt = a.IssuedAt,
                        ExpiresAt = a.ExpiresAt
                    })
                    .ToListAsync();

                var recentIncidents = await _context.DisasterIncidents
                    .Where(i => i.Status != IncidentStatus.Resolved)
                    .CountAsync();

                var nearbyShelters = await _context.EmergencyShelters
                    .CountAsync();

                var dashboard = new MobileDashboardDto
                {
                    ActiveAlertsCount = activeAlerts.Count,
                    RecentIncidentsCount = recentIncidents,
                    NearbySheltersCount = nearbyShelters,
                    RecentAlerts = activeAlerts,
                    UserCityName = user.CityName,
                    UserLGAName = user.LGAName
                };

                return Ok(new MobileApiResponse<MobileDashboardDto> { Success = true, Data = dashboard });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard");
                return StatusCode(500, new MobileApiResponse<MobileDashboardDto>
                {
                    Success = false,
                    Message = "An error occurred while fetching dashboard"
                });
            }
        }

        /// <summary>
        /// Get paginated disaster alerts
        /// </summary>
        [HttpGet("alerts")]
        public async Task<ActionResult<MobileApiResponse<PaginatedAlertsResponse>>> GetAlerts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] AlertSeverity? severity = null,
            [FromQuery] DisasterType? disasterType = null)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return Unauthorized();
                }

                var query = _context.DisasterAlerts
                    .Where(a => a.Status == AlertStatus.Active);

                if (severity.HasValue)
                {
                    query = query.Where(a => a.Severity == severity.Value);
                }

                if (disasterType.HasValue)
                {
                    query = query.Where(a => a.DisasterType == disasterType.Value);
                }

                var totalCount = await query.CountAsync();
                var alerts = await query
                    .OrderByDescending(a => a.IssuedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new MobileAlertDto
                    {
                        Id = (int)a.Id.GetHashCode(),
                        Title = a.Title,
                        Message = a.Message,
                        Severity = a.Severity,
                        DisasterType = a.DisasterType,
                        CityName = null, // AffectedCities is JSON string, handle separately if needed
                        CreatedAt = a.IssuedAt,
                        ExpiresAt = a.ExpiresAt
                    })
                    .ToListAsync();

                var response = new PaginatedAlertsResponse
                {
                    Alerts = alerts,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                return Ok(new MobileApiResponse<PaginatedAlertsResponse> { Success = true, Data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching alerts");
                return StatusCode(500, new MobileApiResponse<PaginatedAlertsResponse>
                {
                    Success = false,
                    Message = "An error occurred while fetching alerts"
                });
            }
        }

        /// <summary>
        /// Get alert by ID
        /// </summary>
        [HttpGet("alerts/{id}")]
        public async Task<ActionResult<MobileApiResponse<MobileAlertDto>>> GetAlert(int id)
        {
            try
            {
                var alertGuid = _context.DisasterAlerts.FirstOrDefault()?.Id ?? Guid.Empty;
                var alert = await _context.DisasterAlerts
                    .FirstOrDefaultAsync(a => a.Id.GetHashCode() == id);

                if (alert == null)
                {
                    return NotFound(new MobileApiResponse<MobileAlertDto>
                    {
                        Success = false,
                        Message = "Alert not found"
                    });
                }

                var alertDto = new MobileAlertDto
                {
                    Id = (int)alert.Id.GetHashCode(),
                    Title = alert.Title,
                    Message = alert.Message,
                    Severity = alert.Severity,
                    DisasterType = alert.DisasterType,
                    CityName = null,
                    CreatedAt = alert.IssuedAt,
                    ExpiresAt = alert.ExpiresAt
                };

                return Ok(new MobileApiResponse<MobileAlertDto> { Success = true, Data = alertDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching alert {AlertId}", id);
                return StatusCode(500, new MobileApiResponse<MobileAlertDto>
                {
                    Success = false,
                    Message = "An error occurred while fetching alert"
                });
            }
        }

        /// <summary>
        /// Report a new incident
        /// </summary>
        [HttpPost("incidents/report")]
        public async Task<ActionResult<MobileApiResponse<int>>> ReportIncident([FromForm] MobileIncidentRequest request)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var incident = new DisasterIncident
                {
                    Title = request.Title,
                    Description = request.Description,
                    DisasterType = request.DisasterType,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Address = request.Location ?? string.Empty,
                    Severity = (IncidentSeverity)(int)request.Severity,
                    Status = IncidentStatus.Reported,
                    ReporterId = userId!,
                    ReportedAt = DateTime.UtcNow
                };

                // Handle photo upload
                if (request.Photo != null && request.Photo.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "incidents");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{request.Photo.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.Photo.CopyToAsync(stream);
                    }

                    incident.PhotoUrls = System.Text.Json.JsonSerializer.Serialize(new[] { $"/uploads/incidents/{uniqueFileName}" });
                }

                _context.DisasterIncidents.Add(incident);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Incident {IncidentId} reported by user {UserId}", incident.Id, userId);

                return Ok(new MobileApiResponse<int>
                {
                    Success = true,
                    Data = incident.Id.GetHashCode(),
                    Message = "Incident reported successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting incident");
                return StatusCode(500, new MobileApiResponse<int>
                {
                    Success = false,
                    Message = "An error occurred while reporting incident"
                });
            }
        }

        /// <summary>
        /// Get nearby shelters
        /// </summary>
        [HttpGet("shelters/nearby")]
        public async Task<ActionResult<MobileApiResponse<List<MobileShelterDto>>>> GetNearbyShelters(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] int radius = 50)
        {
            try
            {
                var shelters = await _context.EmergencyShelters
                    .Where(s => s.IsActive)
                    .ToListAsync();

                var nearbyShelters = shelters
                    .Select(s => new MobileShelterDto
                    {
                        Id = s.Id.GetHashCode(),
                        Name = s.Name,
                        Address = s.Address,
                        Latitude = s.Latitude,
                        Longitude = s.Longitude,
                        Capacity = s.TotalCapacity,
                        CurrentOccupancy = s.CurrentOccupancy,
                        Facilities = BuildFacilitiesString(s),
                        ContactPhone = s.ContactPhone,
                        Distance = CalculateDistance(latitude, longitude, s.Latitude, s.Longitude)
                    })
                    .Where(s => s.Distance <= radius)
                    .OrderBy(s => s.Distance)
                    .ToList();

                return Ok(new MobileApiResponse<List<MobileShelterDto>>
                {
                    Success = true,
                    Data = nearbyShelters
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching nearby shelters");
                return StatusCode(500, new MobileApiResponse<List<MobileShelterDto>>
                {
                    Success = false,
                    Message = "An error occurred while fetching shelters"
                });
            }
        }

        /// <summary>
        /// Get IoT sensor data
        /// </summary>
        [HttpGet("sensors")]
        public async Task<ActionResult<MobileApiResponse<List<MobileSensorDto>>>> GetSensors(
            [FromQuery] int? cityId = null,
            [FromQuery] string? sensorType = null)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var user = await _context.Users.FindAsync(userId);

                var query = _context.WaterLevelSensors.Cast<IoTSensor>().Where(s => s.Status == SensorStatus.Active);

                if (cityId.HasValue)
                {
                    query = query.Where(s => s.CityId == cityId.Value);
                }
                else if (user?.CityId > 0)
                {
                    query = query.Where(s => s.CityId == user.CityId);
                }

                if (!string.IsNullOrEmpty(sensorType))
                {
                    if (Enum.TryParse<SensorType>(sensorType, true, out var sensorTypeEnum))
                    {
                        query = query.Where(s => s.SensorType == sensorTypeEnum);
                    }
                }

                var sensors = await query
                    .OrderBy(s => s.Name)
                    .Select(s => new MobileSensorDto
                    {
                        Id = s.Id.GetHashCode(),
                        Name = s.Name,
                        SensorType = s.SensorType.ToString(),
                        Status = s.Status.ToString(),
                        Latitude = s.Latitude,
                        Longitude = s.Longitude,
                        LastReading = s.LastDataReceivedDate
                    })
                    .ToListAsync();

                return Ok(new MobileApiResponse<List<MobileSensorDto>>
                {
                    Success = true,
                    Data = sensors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sensors");
                return StatusCode(500, new MobileApiResponse<List<MobileSensorDto>>
                {
                    Success = false,
                    Message = "An error occurred while fetching sensors"
                });
            }
        }

        /// <summary>
        /// Sync offline data
        /// </summary>
        [HttpPost("sync")]
        public async Task<ActionResult<MobileApiResponse<object>>> SyncOfflineData([FromBody] List<OfflineIncidentData> offlineIncidents)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                // Process offline incidents
                foreach (var incidentData in offlineIncidents)
                {
                    var incident = new DisasterIncident
                    {
                        Title = incidentData.Title,
                        Description = incidentData.Description,
                        DisasterType = incidentData.DisasterType,
                        Latitude = incidentData.Latitude,
                        Longitude = incidentData.Longitude,
                        Address = incidentData.Location ?? string.Empty,
                        Severity = (IncidentSeverity)(int)incidentData.Severity,
                        Status = IncidentStatus.Reported,
                        ReporterId = userId!,
                        ReportedAt = incidentData.Timestamp
                    };

                    _context.DisasterIncidents.Add(incident);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Synced {Count} offline incidents for user {UserId}",
                    offlineIncidents.Count, userId);

                return Ok(new MobileApiResponse<object>
                {
                    Success = true,
                    Message = $"Synced {offlineIncidents.Count} incidents successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing offline data");
                return StatusCode(500, new MobileApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while syncing data"
                });
            }
        }

        /// <summary>
        /// Calculate distance between two coordinates using Haversine formula
        /// </summary>
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in kilometers

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        /// <summary>
        /// Build facilities string from EmergencyShelter properties
        /// </summary>
        private string BuildFacilitiesString(EmergencyShelter shelter)
        {
            var facilities = new List<string>();
            if (shelter.HasMedicalFacility) facilities.Add("Medical");
            if (shelter.HasFood) facilities.Add("Food");
            if (shelter.HasWater) facilities.Add("Water");
            if (shelter.HasPower) facilities.Add("Power");
            if (shelter.HasSanitation) facilities.Add("Sanitation");
            if (shelter.HasSecurity) facilities.Add("Security");
            if (shelter.IsAccessible) facilities.Add("Accessible");
            return string.Join(", ", facilities);
        }
    }
}
