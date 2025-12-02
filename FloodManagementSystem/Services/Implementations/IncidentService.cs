using Microsoft.EntityFrameworkCore;
using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Interfaces;
using System.Text.Json;

namespace GlobalDisasterManagement.Services.Implementations
{
    public class IncidentService : IIncidentService
    {
        private readonly DisasterDbContext _context;
        private readonly ILogger<IncidentService> _logger;
        private readonly INotificationService _notificationService;

        public IncidentService(
            DisasterDbContext context,
            ILogger<IncidentService> logger,
            INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<DisasterIncident> CreateIncidentAsync(DisasterIncident incident)
        {
            try
            {
                incident.Id = Guid.NewGuid();
                incident.ReportedAt = DateTime.UtcNow;
                incident.LastUpdated = DateTime.UtcNow;

                _context.DisasterIncidents.Add(incident);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Incident created: {IncidentId} - Type: {Type}, Severity: {Severity}",
                    incident.Id, incident.DisasterType, incident.Severity);

                // Send notifications for critical incidents
                if (incident.Severity >= IncidentSeverity.High)
                {
                    await _notificationService.SendDisasterAlert(incident);
                }

                return incident;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating incident");
                throw;
            }
        }

        public async Task<DisasterIncident?> GetIncidentByIdAsync(Guid id)
        {
            return await _context.DisasterIncidents
                .Include(i => i.City)
                .Include(i => i.LGA)
                .Include(i => i.Reporter)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<DisasterIncident>> GetActiveIncidentsAsync()
        {
            return await _context.DisasterIncidents
                .Include(i => i.City)
                .Include(i => i.LGA)
                .Where(i => i.Status != IncidentStatus.Resolved && 
                           i.Status != IncidentStatus.Closed &&
                           i.Status != IncidentStatus.False)
                .OrderByDescending(i => i.Severity)
                .ThenByDescending(i => i.ReportedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DisasterIncident>> GetIncidentsByDisasterTypeAsync(DisasterType disasterType)
        {
            return await _context.DisasterIncidents
                .Include(i => i.City)
                .Include(i => i.LGA)
                .Where(i => i.DisasterType == disasterType)
                .OrderByDescending(i => i.ReportedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DisasterIncident>> GetIncidentsByCityAsync(int cityId)
        {
            return await _context.DisasterIncidents
                .Include(i => i.City)
                .Include(i => i.LGA)
                .Where(i => i.CityId == cityId)
                .OrderByDescending(i => i.ReportedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DisasterIncident>> GetIncidentsByLGAAsync(int lgaId)
        {
            return await _context.DisasterIncidents
                .Include(i => i.City)
                .Include(i => i.LGA)
                .Where(i => i.LGAId == lgaId)
                .OrderByDescending(i => i.ReportedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DisasterIncident>> GetIncidentsBySeverityAsync(IncidentSeverity severity)
        {
            return await _context.DisasterIncidents
                .Include(i => i.City)
                .Include(i => i.LGA)
                .Where(i => i.Severity == severity)
                .OrderByDescending(i => i.ReportedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DisasterIncident>> GetRecentIncidentsAsync(int count = 20)
        {
            return await _context.DisasterIncidents
                .Include(i => i.City)
                .Include(i => i.LGA)
                .OrderByDescending(i => i.ReportedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<DisasterIncident> UpdateIncidentAsync(DisasterIncident incident)
        {
            try
            {
                incident.LastUpdated = DateTime.UtcNow;
                _context.DisasterIncidents.Update(incident);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Incident updated: {IncidentId}", incident.Id);
                return incident;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating incident {IncidentId}", incident.Id);
                throw;
            }
        }

        public async Task<bool> UpdateIncidentStatusAsync(Guid id, IncidentStatus status, string? notes = null)
        {
            try
            {
                var incident = await _context.DisasterIncidents.FindAsync(id);
                if (incident == null) return false;

                incident.Status = status;
                incident.LastUpdated = DateTime.UtcNow;

                if (status == IncidentStatus.Resolved)
                {
                    incident.ResolvedAt = DateTime.UtcNow;
                }

                if (!string.IsNullOrEmpty(notes))
                {
                    incident.ResponseNotes = notes;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Incident {IncidentId} status updated to {Status}", id, status);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating incident status for {IncidentId}", id);
                return false;
            }
        }

        public async Task<bool> VerifyIncidentAsync(Guid id, string verifiedBy)
        {
            try
            {
                var incident = await _context.DisasterIncidents.FindAsync(id);
                if (incident == null) return false;

                incident.IsVerified = true;
                incident.VerifiedAt = DateTime.UtcNow;
                incident.VerifiedBy = verifiedBy;
                incident.Status = IncidentStatus.Verified;
                incident.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Incident {IncidentId} verified by {VerifiedBy}", id, verifiedBy);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying incident {IncidentId}", id);
                return false;
            }
        }

        public async Task<bool> AssignRespondersAsync(Guid id, List<string> responderIds)
        {
            try
            {
                var incident = await _context.DisasterIncidents.FindAsync(id);
                if (incident == null) return false;

                incident.AssignedResponders = JsonSerializer.Serialize(responderIds);
                incident.EmergencyServicesNotified = true;
                incident.EmergencyResponseTime = DateTime.UtcNow;
                incident.Status = IncidentStatus.EmergencyDispatched;
                incident.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Responders assigned to incident {IncidentId}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning responders to incident {IncidentId}", id);
                return false;
            }
        }

        public async Task<IEnumerable<DisasterIncident>> GetIncidentsByLocationAsync(double latitude, double longitude, double radiusKm)
        {
            // Haversine formula for distance calculation
            var incidents = await _context.DisasterIncidents
                .Include(i => i.City)
                .Include(i => i.LGA)
                .ToListAsync();

            return incidents.Where(i =>
            {
                var distance = CalculateDistance(latitude, longitude, i.Latitude, i.Longitude);
                return distance <= radiusKm;
            }).OrderBy(i => CalculateDistance(latitude, longitude, i.Latitude, i.Longitude));
        }

        public async Task<int> GetActiveIncidentCountAsync()
        {
            return await _context.DisasterIncidents
                .CountAsync(i => i.Status != IncidentStatus.Resolved &&
                               i.Status != IncidentStatus.Closed &&
                               i.Status != IncidentStatus.False);
        }

        public async Task<Dictionary<IncidentSeverity, int>> GetIncidentStatisticsBySeverityAsync()
        {
            return await _context.DisasterIncidents
                .GroupBy(i => i.Severity)
                .Select(g => new { Severity = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Severity, x => x.Count);
        }

        public async Task<Dictionary<DisasterType, int>> GetIncidentStatisticsByTypeAsync()
        {
            return await _context.DisasterIncidents
                .GroupBy(i => i.DisasterType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in km
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
    }
}
