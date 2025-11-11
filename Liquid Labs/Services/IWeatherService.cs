using Liquid_Labs.Models;

namespace Liquid_Labs.Services
{
    /// <summary>
    /// Interface for weather service operations
    /// </summary>
    public interface IWeatherService
    {
        Task<WeatherData?> GetWeatherByIdAsync(int id);
        Task<WeatherData?> GetWeatherByCityAsync(string cityName);
        Task<List<WeatherData>> GetAllWeatherDataAsync();
    }
}
