using GlobalDisasterManagement.Models;

namespace GlobalDisasterManagement.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendFloodAlertAsync(CityFloodPrediction prediction, List<string> recipients);
        Task<bool> SendWelcomeEmailAsync(string email, string userName);
        Task<bool> SendBulkFloodAlertsAsync(List<CityFloodPrediction> predictions);
    }
}
