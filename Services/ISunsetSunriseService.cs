using SolarWatch.Models;

namespace SolarWatch.Services;

public interface ISunriseSunsetService
{
    Task<SunriseSunsetData> GetSunriseSunsetAsync(double lat, double lon, string date);
}