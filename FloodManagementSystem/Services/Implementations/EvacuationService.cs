using Microsoft.EntityFrameworkCore;
using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Interfaces;

namespace GlobalDisasterManagement.Services.Implementations
{
    public class EvacuationService : IEvacuationService
    {
        private readonly DisasterDbContext _context;
        private readonly ILogger<EvacuationService> _logger;

        public EvacuationService(DisasterDbContext context, ILogger<EvacuationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<EvacuationRoute> CreateRouteAsync(EvacuationRoute route)
        {
            try
            {
                route.Id = Guid.NewGuid();
                route.CreatedAt = DateTime.UtcNow;

                _context.EvacuationRoutes.Add(route);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Evacuation route created: {RouteId} - {RouteName}", route.Id, route.Name);
                return route;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating evacuation route");
                throw;
            }
        }

        public async Task<EvacuationRoute?> GetRouteByIdAsync(Guid id)
        {
            return await _context.EvacuationRoutes
                .Include(r => r.City)
                .Include(r => r.LGA)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<EvacuationRoute>> GetActiveRoutesAsync()
        {
            return await _context.EvacuationRoutes
                .Include(r => r.City)
                .Include(r => r.LGA)
                .Where(r => r.Status == RouteStatus.Open || r.Status == RouteStatus.Congested)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<EvacuationRoute>> GetRoutesByCityAsync(int cityId)
        {
            return await _context.EvacuationRoutes
                .Include(r => r.City)
                .Include(r => r.LGA)
                .Where(r => r.CityId == cityId)
                .OrderBy(r => r.IsPrimary ? 0 : 1)
                .ThenBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<EvacuationRoute>> GetRoutesFromLocationAsync(double latitude, double longitude)
        {
            var routes = await _context.EvacuationRoutes
                .Include(r => r.City)
                .Include(r => r.LGA)
                .Where(r => r.Status == RouteStatus.Open || r.Status == RouteStatus.Congested)
                .ToListAsync();

            return routes
                .Where(r =>
                {
                    var distance = CalculateDistance(latitude, longitude, r.StartLatitude, r.StartLongitude);
                    return distance <= 10; // Within 10km of start point
                })
                .OrderBy(r => CalculateDistance(latitude, longitude, r.StartLatitude, r.StartLongitude));
        }

        public async Task<EvacuationRoute?> GetOptimalRouteAsync(double startLat, double startLng, double endLat, double endLng)
        {
            var routes = await _context.EvacuationRoutes
                .Include(r => r.City)
                .Include(r => r.LGA)
                .Where(r => r.Status == RouteStatus.Open)
                .ToListAsync();

            EvacuationRoute? bestRoute = null;
            double minDistance = double.MaxValue;

            foreach (var route in routes)
            {
                var startDistance = CalculateDistance(startLat, startLng, route.StartLatitude, route.StartLongitude);
                var endDistance = CalculateDistance(endLat, endLng, route.EndLatitude, route.EndLongitude);
                var totalDistance = startDistance + endDistance;

                if (totalDistance < minDistance && startDistance < 5) // Within 5km of route start
                {
                    minDistance = totalDistance;
                    bestRoute = route;
                }
            }

            return bestRoute;
        }

        public async Task<EvacuationRoute> UpdateRouteAsync(EvacuationRoute route)
        {
            try
            {
                route.LastUpdated = DateTime.UtcNow;
                _context.EvacuationRoutes.Update(route);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Evacuation route updated: {RouteId}", route.Id);
                return route;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating evacuation route {RouteId}", route.Id);
                throw;
            }
        }

        public async Task<bool> UpdateRouteStatusAsync(Guid id, RouteStatus status)
        {
            try
            {
                var route = await _context.EvacuationRoutes.FindAsync(id);
                if (route == null) return false;

                route.Status = status;
                route.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Evacuation route {RouteId} status updated to {Status}", id, status);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating route status for {RouteId}", id);
                return false;
            }
        }

        public async Task<IEnumerable<EvacuationRoute>> GetSafeRoutesAsync()
        {
            return await _context.EvacuationRoutes
                .Include(r => r.City)
                .Include(r => r.LGA)
                .Where(r => r.Status == RouteStatus.Open && r.IsVerified)
                .OrderBy(r => r.IsPrimary ? 0 : 1)
                .ThenBy(r => r.EstimatedTimeMinutes)
                .ToListAsync();
        }

        public async Task<EmergencyContact> CreateEmergencyContactAsync(EmergencyContact contact)
        {
            try
            {
                contact.Id = Guid.NewGuid();
                contact.CreatedAt = DateTime.UtcNow;

                _context.EmergencyContacts.Add(contact);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Emergency contact created: {ContactId} - {OrgName}", contact.Id, contact.OrganizationName);
                return contact;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating emergency contact");
                throw;
            }
        }

        public async Task<IEnumerable<EmergencyContact>> GetEmergencyContactsAsync(string? countryCode = null)
        {
            var query = _context.EmergencyContacts
                .Where(c => c.IsActive);

            if (!string.IsNullOrEmpty(countryCode))
            {
                query = query.Where(c => c.CountryCode == countryCode);
            }

            return await query
                .OrderBy(c => c.Priority)
                .ThenBy(c => c.OrganizationName)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmergencyContact>> GetEmergencyContactsByTypeAsync(EmergencyServiceType type)
        {
            return await _context.EmergencyContacts
                .Where(c => c.ServiceType == type && c.IsActive)
                .OrderBy(c => c.Priority)
                .ThenBy(c => c.OrganizationName)
                .ToListAsync();
        }

        public async Task<EmergencyContact?> GetPrimaryEmergencyContactAsync(EmergencyServiceType type)
        {
            return await _context.EmergencyContacts
                .Where(c => c.ServiceType == type && c.IsActive)
                .OrderBy(c => c.Priority)
                .FirstOrDefaultAsync();
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
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
