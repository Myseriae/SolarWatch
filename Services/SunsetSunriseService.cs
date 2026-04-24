using System.Text.Json;
using SolarWatch.Models;

namespace SolarWatch.Services;

public class SunriseSunsetService : ISunriseSunsetService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SunriseSunsetService> _logger;

    public SunriseSunsetService(HttpClient httpClient, ILogger<SunriseSunsetService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SunriseSunsetData> GetSunriseSunsetAsync(double lat, double lon, string date)
    {
        var url = $"https://api.sunrise-sunset.org/json?lat={lat}&lng={lon}&date={date}&formatted=0";
        _logger.LogInformation("Calling sunrise-sunset API for lat: {Lat}, lon: {Lon}, date: {Date}", lat, lon, date);

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SunriseSunsetApiResponse>(content);

        if (result?.Status != "OK")
            throw new InvalidOperationException("Sunrise-sunset API returned an error status");

        return result.Results;
    }
}