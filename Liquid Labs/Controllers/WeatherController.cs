using Liquid_Labs.Models;
using Liquid_Labs.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Liquid_Labs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly ILogger<WeatherController> _logger;

        public WeatherController(IWeatherService weatherService, ILogger<WeatherController> logger)
        {
            _weatherService = weatherService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all weather records from the database
        /// </summary>
        /// <returns>List of all weather records</returns>
        /// <response code="200">Returns the list of weather records</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<WeatherData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<WeatherData>>> GetAllWeatherData()
        {
            try
            {
                var weatherData = await _weatherService.GetAllWeatherDataAsync();
                return Ok(weatherData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all weather data");
                return StatusCode(500, new { error = "An error occurred while retrieving weather data" });
            }
        }
        /// <summary>
        /// Gets weather data for a specific city (with caching)
        /// Checks database first, fetches from API if not found or expired
        /// </summary>
        /// <param name="cityName">Name of the city</param>
        /// <returns>Weather data for the specified city</returns>
        /// <response code="200">Returns the weather data</response>
        /// <response code="404">If the city was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("{cityName}")]
        [ProducesResponseType(typeof(WeatherData), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<WeatherData>> GetWeatherByCity(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                return BadRequest(new { error = "City name is required" });
            }

            try
            {
                var weatherData = await _weatherService.GetWeatherByCityAsync(cityName);

                if (weatherData == null)
                {
                    return NotFound(new { error = $"Weather data not found for city: {cityName}" });
                }

                return Ok(weatherData);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Configuration error");
                return StatusCode(500, new { error = "Service configuration error" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching weather for city: {CityName}", cityName);
                return StatusCode(500, new { error = "An error occurred while retrieving weather data" });
            }
        }
        /// <summary>
        /// Gets a specific weather record by its ID
        /// </summary>
        /// <param name="id">The ID of the weather record</param>
        /// <returns>Weather data for the specified ID</returns>
        /// <response code="200">Returns the weather data</response>
        /// <response code="404">If the record was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("id/{id}")]
        [ProducesResponseType(typeof(WeatherData), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<WeatherData>> GetWeatherById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { error = "Invalid ID" });
            }

            try
            {
                var weatherData = await _weatherService.GetWeatherByIdAsync(id);

                if (weatherData == null)
                {
                    return NotFound(new { error = $"Weather data not found for ID: {id}" });
                }

                return Ok(weatherData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching weather by ID: {Id}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving weather data" });
            }
        }
    }
}
