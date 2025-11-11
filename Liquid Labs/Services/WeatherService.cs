using Liquid_Labs.Models;
using System.Text.Json;

namespace Liquid_Labs.Services
{
    /// <summary>
    /// Orchestrates weather data retrieval from cache or external API
    /// </summary>
    public class WeatherService : IWeatherService
    {
        private readonly IDatabaseService _databaseService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WeatherService> _logger;
        private const int CacheExpirationMinutes = 30;

        public WeatherService(
            IDatabaseService databaseService,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<WeatherService> logger)
        {
            _databaseService = databaseService;
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }
        /// <summary>
        /// Retrieves weather data by ID from database
        /// </summary>
        public async Task<WeatherData?> GetWeatherByIdAsync(int id)
        {
            try
            {
                return await _databaseService.GetWeatherByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving weather data by ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Retrieves weather data by city name with caching logic
        /// Checks database first, fetches from API if not found or expired
        /// </summary>
        public async Task<WeatherData?> GetWeatherByCityAsync(string cityName)
        {
            try
            {
                // Check if data exists in database and is fresh
                var cachedData = await _databaseService.GetWeatherByCityAsync(cityName);

                if (cachedData != null && IsCacheValid(cachedData.FetchedAt))
                {
                    _logger.LogInformation("Returning cached data for city: {CityName}", cityName);
                    return cachedData;
                }

                // Fetch from external API
                _logger.LogInformation("Fetching fresh data from API for city: {CityName}", cityName);
                var weatherData = await FetchWeatherFromApiAsync(cityName);

                if (weatherData == null)
                {
                    return null;
                }

                // Store in database
                weatherData.Id = await _databaseService.InsertOrUpdateWeatherDataAsync(weatherData);

                return weatherData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving weather data for city: {CityName}", cityName);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all weather records from database
        /// </summary>
        public async Task<List<WeatherData>> GetAllWeatherDataAsync()
        {
            try
            {
                return await _databaseService.GetAllWeatherDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all weather data");
                throw;
            }
        }
        /// <summary>
        /// Fetches weather data from OpenWeatherMap API
        /// </summary>
        private async Task<WeatherData?> FetchWeatherFromApiAsync(string cityName)
        {
            var apiKey = _configuration["OPENWEATHER_API_KEY"]
                ?? Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY");

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException(
                    "OpenWeatherMap API key not found. Set OPENWEATHER_API_KEY environment variable.");
            }

            var url = $"https://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={apiKey}&units=metric";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning("City not found: {CityName}", cityName);
                        return null;
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API request failed with status {StatusCode}: {Error}",
                        response.StatusCode, errorContent);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return ParseWeatherApiResponse(content, cityName);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching weather data for: {CityName}", cityName);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error for weather data: {CityName}", cityName);
                throw;
            }
        }
        /// <summary>
        /// Parses the JSON response from OpenWeatherMap API
        /// </summary>
        private WeatherData ParseWeatherApiResponse(string jsonContent, string cityName)
        {
            using var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;

            var main = root.GetProperty("main");
            var weather = root.GetProperty("weather")[0];
            var wind = root.GetProperty("wind");

            return new WeatherData
            {
                CityName = cityName,
                Temperature = main.GetProperty("temp").GetDecimal(),
                Description = weather.GetProperty("description").GetString() ?? "Unknown",
                Humidity = main.GetProperty("humidity").GetInt32(),
                WindSpeed = wind.GetProperty("speed").GetDecimal(),
                FetchedAt = DateTime.UtcNow
            };
        }
        /// <summary>
        /// Checks if cached data is still valid based on expiration time
        /// </summary>
        private bool IsCacheValid(DateTime fetchedAt)
        {
            var expirationTime = fetchedAt.AddMinutes(CacheExpirationMinutes);
            return DateTime.UtcNow < expirationTime;
        }
    }

}
