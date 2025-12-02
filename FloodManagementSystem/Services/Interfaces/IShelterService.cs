using GlobalDisasterManagement.Models;

namespace GlobalDisasterManagement.Services.Interfaces
{
    public interface IShelterService
    {
        Task<EmergencyShelter> CreateShelterAsync(EmergencyShelter shelter);
        Task<EmergencyShelter?> GetShelterByIdAsync(Guid id);
        Task<IEnumerable<EmergencyShelter>> GetActiveSheltersAsync();
        Task<IEnumerable<EmergencyShelter>> GetSheltersByCityAsync(int cityId);
        Task<IEnumerable<EmergencyShelter>> GetSheltersWithAvailabilityAsync();
        Task<IEnumerable<EmergencyShelter>> GetNearbySheltersAsync(double latitude, double longitude, double radiusKm);
        Task<EmergencyShelter> UpdateShelterAsync(EmergencyShelter shelter);
        Task<bool> UpdateShelterCapacityAsync(Guid shelterId, int currentOccupancy);
        Task<ShelterCheckIn> CheckInToShelterAsync(ShelterCheckIn checkIn);
        Task<bool> CheckOutFromShelterAsync(Guid checkInId);
        Task<IEnumerable<ShelterCheckIn>> GetShelterOccupantsAsync(Guid shelterId);
        Task<int> GetTotalShelterCapacityAsync();
        Task<int> GetTotalOccupancyAsync();
        Task<Dictionary<Guid, int>> GetShelterOccupancyStatsAsync();
    }
}
