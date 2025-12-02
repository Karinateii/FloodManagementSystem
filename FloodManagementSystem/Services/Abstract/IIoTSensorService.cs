using GlobalDisasterManagement.Models;

namespace GlobalDisasterManagement.Services.Abstract
{
    /// <summary>
    /// Service for managing IoT sensors and real-time data ingestion
    /// </summary>
    public interface IIoTSensorService
    {
        // Sensor Management
        Task<WaterLevelSensor> RegisterWaterLevelSensorAsync(WaterLevelSensor sensor);
        Task<RainfallSensor> RegisterRainfallSensorAsync(RainfallSensor sensor);
        Task<WeatherSensor> RegisterWeatherSensorAsync(WeatherSensor sensor);
        Task<bool> UpdateSensorStatusAsync(Guid sensorId, SensorStatus status);
        Task<bool> DeactivateSensorAsync(Guid sensorId);
        Task<IoTSensor?> GetSensorByDeviceIdAsync(string deviceId);
        Task<List<IoTSensor>> GetActiveSensorsAsync();
        Task<List<IoTSensor>> GetSensorsByCityAsync(int cityId);
        Task<List<IoTSensor>> GetSensorsByTypeAsync(SensorType type);

        // Water Level Sensor Operations
        Task<WaterLevelReading> RecordWaterLevelAsync(string deviceId, double level);
        Task<List<WaterLevelReading>> GetWaterLevelHistoryAsync(Guid sensorId, DateTime from, DateTime to);
        Task<WaterLevelReading?> GetLatestWaterLevelAsync(Guid sensorId);
        Task<List<WaterLevelSensor>> GetCriticalWaterLevelSensorsAsync();

        // Rainfall Sensor Operations
        Task<RainfallReading> RecordRainfallAsync(string deviceId, double rainfall, DateTime periodStart, DateTime periodEnd);
        Task<List<RainfallReading>> GetRainfallHistoryAsync(Guid sensorId, DateTime from, DateTime to);
        Task<RainfallReading?> GetLatestRainfallAsync(Guid sensorId);
        Task<double> GetDailyRainfallAsync(Guid sensorId, DateTime date);

        // Weather Sensor Operations
        Task<WeatherReading> RecordWeatherDataAsync(string deviceId, WeatherReading reading);
        Task<List<WeatherReading>> GetWeatherHistoryAsync(Guid sensorId, DateTime from, DateTime to);
        Task<WeatherReading?> GetLatestWeatherAsync(Guid sensorId);
        Task<Dictionary<string, object>> GetWeatherSummaryAsync(int cityId);

        // Alert Detection
        Task<List<DisasterAlert>> CheckThresholdsAndGenerateAlertsAsync();
        Task<bool> CheckWaterLevelThresholdAsync(WaterLevelReading reading);
        Task<bool> CheckRainfallThresholdAsync(RainfallReading reading);

        // Analytics
        Task<Dictionary<string, object>> GetSensorStatisticsAsync();
        Task<List<object>> GetSensorHealthReportAsync();
        Task<Dictionary<string, double>> GetAverageReadingsAsync(Guid sensorId, DateTime from, DateTime to);
    }
}
