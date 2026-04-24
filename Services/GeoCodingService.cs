using System.Text.Json;
using SolarWatch.Models;

namespace SolarWatch.Services;

public class GeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GeocodingService> _logger;

    public GeocodingService(HttpClient httpClient, IConfiguration configuration, ILogger<GeocodingService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenWeather:ApiKey"]
                  ?? throw new InvalidOperationException("OpenWeather API key is not configured");
        _logger = logger;
    }

    public async Task<GeocodingResult> GetCoordinatesAsync(string city)
    {
        var url = $"http://api.openweathermap.org/geo/1.0/direct?q={city}&limit=1&appid={_apiKey}";
        _logger.LogInformation("Calling geocoding API for city: {City}", city);

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var results = JsonSerializer.Deserialize<GeocodingResult[]>(content);

        if (results == null || results.Length == 0)
            throw new ArgumentException($"City not found: {city}");

        return results[0];
    }
}