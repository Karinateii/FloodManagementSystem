using GlobalDisasterManagement.Models;

namespace GlobalDisasterManagement.Services.Interfaces
{
    public interface IFloodPredictionService
    {
        Task<(bool Success, string Message, double Accuracy)> TrainModelAsync(Guid fileId);
        Task<(bool Success, string Message, int PredictionCount)> GeneratePredictionsAsync(
            Guid fileId, int cityId, Guid modelId, string year);
        Task<IEnumerable<CityFloodPrediction>> GetPredictionsForCityAsync(int cityId);
        Task<IEnumerable<CityFloodPrediction>> GetPredictionsForUserAsync(string userId);
        Task<Dictionary<string, int>> GetPredictionStatisticsAsync();
    }
}
