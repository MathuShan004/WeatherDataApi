using Liquid_Labs.Models;

namespace Liquid_Labs.Services
{
    /// <summary>
    /// Interface for database operations
    /// </summary>
    public interface IDatabaseService
    {
        Task<WeatherData?> GetWeatherByIdAsync(int id);
        Task<WeatherData?> GetWeatherByCityAsync(string cityName);
        Task<List<WeatherData>> GetAllWeatherDataAsync();
        Task<int> InsertOrUpdateWeatherDataAsync(WeatherData weatherData);
        
    }
}
