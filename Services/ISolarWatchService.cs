using SolarWatch.Models;

namespace SolarWatch.Services;

public interface ISolarWatchService
{
    Task<SolarWatchResponse> GetSunriseSunsetAsync(string city, string date);
}
