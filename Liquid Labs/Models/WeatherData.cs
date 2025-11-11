namespace Liquid_Labs.Models
{
    /// <summary>
    /// Represents weather data for a specific city
    /// </summary>
    public class WeatherData
    {
        /// <summary>
        /// Unique identifier for the record
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the city
        /// </summary>
        public string CityName { get; set; } = string.Empty;

        /// <summary>
        /// Temperature in Celsius
        /// </summary>
        public decimal Temperature { get; set; }

        /// <summary>
        /// Weather condition description (e.g., "Cloudy", "Rainy")
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Humidity percentage (0-100)
        /// </summary>
        public int Humidity { get; set; }

        /// <summary>
        /// Wind speed in meters per second
        /// </summary>
        public decimal WindSpeed { get; set; }

        /// <summary>
        /// Timestamp when data was fetched
        /// </summary>
        public DateTime FetchedAt { get; set; }
    }
}
