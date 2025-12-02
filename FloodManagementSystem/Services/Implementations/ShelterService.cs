using Microsoft.EntityFrameworkCore;
using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Interfaces;

namespace GlobalDisasterManagement.Services.Implementations
{
    public class ShelterService : IShelterService
    {
        private readonly DisasterDbContext _context;
        private readonly ILogger<ShelterService> _logger;

        public ShelterService(DisasterDbContext context, ILogger<ShelterService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<EmergencyShelter> CreateShelterAsync(EmergencyShelter shelter)
        {
            try
            {
                shelter.Id = Guid.NewGuid();
                shelter.CreatedAt = DateTime.UtcNow;
                
                _context.EmergencyShelters.Add(shelter);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Shelter created: {ShelterId} - {ShelterName}", shelter.Id, shelter.Name);
                return shelter;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shelter");
                throw;
            }
        }

        public async Task<EmergencyShelter?> GetShelterByIdAsync(Guid id)
        {
            return await _context.EmergencyShelters
                .Include(s => s.City)
                .Include(s => s.LGA)
                .Include(s => s.CheckIns)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<EmergencyShelter>> GetActiveSheltersAsync()
        {
            return await _context.EmergencyShelters
                .Include(s => s.City)
                .Include(s => s.LGA)
                .Where(s => s.IsActive && s.IsOperational)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmergencyShelter>> GetSheltersByCityAsync(int cityId)
        {
            return await _context.EmergencyShelters
                .Include(s => s.City)
                .Include(s => s.LGA)
                .Where(s => s.CityId == cityId && s.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmergencyShelter>> GetSheltersWithAvailabilityAsync()
        {
            return await _context.EmergencyShelters
                .Include(s => s.City)
                .Include(s => s.LGA)
                .Where(s => s.IsActive && s.IsOperational && s.CurrentOccupancy < s.TotalCapacity)
                .OrderByDescending(s => s.TotalCapacity - s.CurrentOccupancy)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmergencyShelter>> GetNearbySheltersAsync(double latitude, double longitude, double radiusKm)
        {
            var shelters = await _context.EmergencyShelters
                .Include(s => s.City)
                .Include(s => s.LGA)
                .Where(s => s.IsActive && s.IsOperational)
                .ToListAsync();

            return shelters.Where(s =>
            {
                var distance = CalculateDistance(latitude, longitude, s.Latitude, s.Longitude);
                return distance <= radiusKm;
            }).OrderBy(s => CalculateDistance(latitude, longitude, s.Latitude, s.Longitude));
        }

        public async Task<EmergencyShelter> UpdateShelterAsync(EmergencyShelter shelter)
        {
            try
            {
                shelter.LastUpdated = DateTime.UtcNow;
                _context.EmergencyShelters.Update(shelter);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Shelter updated: {ShelterId}", shelter.Id);
                return shelter;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shelter {ShelterId}", shelter.Id);
                throw;
            }
        }

        public async Task<bool> UpdateShelterCapacityAsync(Guid shelterId, int currentOccupancy)
        {
            try
            {
                var shelter = await _context.EmergencyShelters.FindAsync(shelterId);
                if (shelter == null) return false;

                shelter.CurrentOccupancy = currentOccupancy;
                shelter.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Shelter {ShelterId} occupancy updated to {Occupancy}", shelterId, currentOccupancy);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shelter capacity for {ShelterId}", shelterId);
                return false;
            }
        }

        public async Task<ShelterCheckIn> CheckInToShelterAsync(ShelterCheckIn checkIn)
        {
            try
            {
                checkIn.Id = Guid.NewGuid();
                checkIn.CheckInTime = DateTime.UtcNow;
                checkIn.IsCheckedOut = false;

                _context.ShelterCheckIns.Add(checkIn);

                // Update shelter occupancy
                var shelter = await _context.EmergencyShelters.FindAsync(checkIn.ShelterId);
                if (shelter != null)
                {
                    shelter.CurrentOccupancy += checkIn.FamilyMembers;
                    shelter.LastUpdated = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Check-in created: {CheckInId} for shelter {ShelterId}", checkIn.Id, checkIn.ShelterId);

                return checkIn;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in to shelter {ShelterId}", checkIn.ShelterId);
                throw;
            }
        }

        public async Task<bool> CheckOutFromShelterAsync(Guid checkInId)
        {
            try
            {
                var checkIn = await _context.ShelterCheckIns
                    .Include(c => c.Shelter)
                    .FirstOrDefaultAsync(c => c.Id == checkInId);

                if (checkIn == null || checkIn.IsCheckedOut) return false;

                checkIn.CheckOutTime = DateTime.UtcNow;
                checkIn.IsCheckedOut = true;

                // Update shelter occupancy
                if (checkIn.Shelter != null)
                {
                    checkIn.Shelter.CurrentOccupancy -= checkIn.FamilyMembers;
                    if (checkIn.Shelter.CurrentOccupancy < 0) checkIn.Shelter.CurrentOccupancy = 0;
                    checkIn.Shelter.LastUpdated = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Check-out processed for {CheckInId}", checkInId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking out from shelter {CheckInId}", checkInId);
                return false;
            }
        }

        public async Task<IEnumerable<ShelterCheckIn>> GetShelterOccupantsAsync(Guid shelterId)
        {
            return await _context.ShelterCheckIns
                .Where(c => c.ShelterId == shelterId && !c.IsCheckedOut)
                .OrderBy(c => c.CheckInTime)
                .ToListAsync();
        }

        public async Task<int> GetTotalShelterCapacityAsync()
        {
            return await _context.EmergencyShelters
                .Where(s => s.IsActive && s.IsOperational)
                .SumAsync(s => s.TotalCapacity);
        }

        public async Task<int> GetTotalOccupancyAsync()
        {
            return await _context.EmergencyShelters
                .Where(s => s.IsActive && s.IsOperational)
                .SumAsync(s => s.CurrentOccupancy);
        }

        public async Task<Dictionary<Guid, int>> GetShelterOccupancyStatsAsync()
        {
            return await _context.EmergencyShelters
                .Where(s => s.IsActive)
                .ToDictionaryAsync(s => s.Id, s => s.CurrentOccupancy);
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
