# WeatherDataApi
Liquid Labs/
├── Controllers/
│   └── WeatherController.cs       # API endpoints
├── Models/
│   └── WeatherData.cs             # Data model
├── Services/
│   ├── IWeatherService.cs         # Service interface
│   ├── WeatherService.cs          # Business logic
│   ├── IDatabaseService.cs        # Database interface
│   └── DatabaseService.cs         # SQL operations
├── Scripts/
│   └── InitializeDatabase.sql     # Database setup
├── appsettings.json               # Configuration
└── Program.cs                     # Application entry point

## Database Schema

### WeatherData Table
```sql
CREATE TABLE WeatherData (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CityName NVARCHAR(100) NOT NULL,
    Temperature DECIMAL(5,2) NOT NULL,
    Description NVARCHAR(200) NOT NULL,
    Humidity INT NOT NULL,
    WindSpeed DECIMAL(5,2) NOT NULL,
    FetchedAt DATETIME2 NOT NULL,
    CONSTRAINT UQ_CityName UNIQUE(CityName)
);

**Windows (Command Prompt):**
```cmd
set OPENWEATHER_API_KEY=your_api_key_here
set ConnectionStrings__DefaultConnection=Server=localhost;Database=WeatherDB;Trusted_Connection=True;TrustServerCertificate=True;
```

## Technology Stack
- **ASP.NET Core 8.0**: Latest LTS version with improved performance and built-in features
- **MS SQL Server**: For data persistence
- **Microsoft.Data.SqlClient**: ADO.NET provider for SQL Server operations (no ORM as per requirements)
- **System.Text.Json**: Built-in JSON serialization (lightweight, no external dependencies)
- **HttpClient**: For external API calls with built-in connection pooling

### API Key Issues
- Verify environment variable is set correctly
- Restart terminal/IDE after setting environment variables
- Free tier has rate limits (60 calls/minute)

### Using curl
```bash
# Get all records
curl https://localhost:5001/api/weather

# Get weather for London (will fetch and cache)
curl https://localhost:5001/api/weather/London

# Get by ID
curl https://localhost:5001/api/weather/id/1
```
