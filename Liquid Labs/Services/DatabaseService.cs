using Liquid_Labs.Models;
using Microsoft.Data.SqlClient;

namespace Liquid_Labs.Services
{
    /// <summary>
    /// Handles all database operations using raw SQL queries
    /// </summary>
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("Connection string not found");
            _logger = logger;
        }
        /// <summary>
        /// Retrieves a weather record by its unique ID
        /// </summary>
        public async Task<WeatherData?> GetWeatherByIdAsync(int id)
        {
            const string query = @"
                SELECT Id, CityName, Temperature, Description, Humidity, WindSpeed, FetchedAt
                FROM WeatherData
                WHERE Id = @Id";

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return MapReaderToWeatherData(reader);
                }

                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error while fetching weather by ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a weather record by city name
        /// </summary>
        public async Task<WeatherData?> GetWeatherByCityAsync(string cityName)
        {
            const string query = @"
                SELECT Id, CityName, Temperature, Description, Humidity, WindSpeed, FetchedAt
                FROM WeatherData
                WHERE CityName = @CityName";

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CityName", cityName);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return MapReaderToWeatherData(reader);
                }

                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error while fetching weather for city: {CityName}", cityName);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all weather records from the database
        /// </summary>
        public async Task<List<WeatherData>> GetAllWeatherDataAsync()
        {
            const string query = @"
                SELECT Id, CityName, Temperature, Description, Humidity, WindSpeed, FetchedAt
                FROM WeatherData
                ORDER BY FetchedAt DESC";

            var weatherDataList = new List<WeatherData>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    weatherDataList.Add(MapReaderToWeatherData(reader));
                }

                return weatherDataList;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error while fetching all weather data");
                throw;
            }
        }

        /// <summary>
        /// Inserts new weather data or updates existing record for a city
        /// Uses MERGE statement for upsert operation
        /// </summary>
        public async Task<int> InsertOrUpdateWeatherDataAsync(WeatherData weatherData)
        {
            const string query = @"
                MERGE WeatherData AS target
                USING (SELECT @CityName AS CityName) AS source
                ON target.CityName = source.CityName
                WHEN MATCHED THEN
                    UPDATE SET 
                        Temperature = @Temperature,
                        Description = @Description,
                        Humidity = @Humidity,
                        WindSpeed = @WindSpeed,
                        FetchedAt = @FetchedAt
                WHEN NOT MATCHED THEN
                    INSERT (CityName, Temperature, Description, Humidity, WindSpeed, FetchedAt)
                    VALUES (@CityName, @Temperature, @Description, @Humidity, @WindSpeed, @FetchedAt);
                
                SELECT Id FROM WeatherData WHERE CityName = @CityName;";

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CityName", weatherData.CityName);
                command.Parameters.AddWithValue("@Temperature", weatherData.Temperature);
                command.Parameters.AddWithValue("@Description", weatherData.Description);
                command.Parameters.AddWithValue("@Humidity", weatherData.Humidity);
                command.Parameters.AddWithValue("@WindSpeed", weatherData.WindSpeed);
                command.Parameters.AddWithValue("@FetchedAt", weatherData.FetchedAt);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error while inserting/updating weather data for: {CityName}",
                    weatherData.CityName);
                throw;
            }
        }

        /// <summary>
        /// Maps a SqlDataReader row to a WeatherData object
        /// </summary>
        private static WeatherData MapReaderToWeatherData(SqlDataReader reader)
        {
            return new WeatherData
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                CityName = reader.GetString(reader.GetOrdinal("CityName")),
                Temperature = reader.GetDecimal(reader.GetOrdinal("Temperature")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Humidity = reader.GetInt32(reader.GetOrdinal("Humidity")),
                WindSpeed = reader.GetDecimal(reader.GetOrdinal("WindSpeed")),
                FetchedAt = reader.GetDateTime(reader.GetOrdinal("FetchedAt"))
            };
        }
    }
}
