using SolarWatch.Models;

namespace SolarWatch.Services;

public interface IGeocodingService
{
    Task<GeocodingResult> GetCoordinatesAsync(string city);
}