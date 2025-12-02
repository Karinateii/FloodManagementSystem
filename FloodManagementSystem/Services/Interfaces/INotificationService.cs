using GlobalDisasterManagement.Models;

namespace GlobalDisasterManagement.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendFloodAlertToCity(int cityId, CityFloodPrediction prediction);
        Task SendFloodAlertToLGA(int lgaId, string message);
        Task BroadcastFloodAlert(string message);
        Task SendModelTrainingUpdate(string status, double? accuracy = null);
        Task SendDisasterAlert(DisasterIncident incident);
        Task SendDisasterAlertToCity(int cityId, DisasterAlert alert);
        Task SendDisasterAlertToRegion(string region, DisasterAlert alert);
    }
}
