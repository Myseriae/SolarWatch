using SolarWatch.Models;

namespace SolarWatch.Repositories;

public interface ISunriseSunsetRepository
{
    Task<SunriseSunset?> GetByCityAndDateAsync(int cityId, string date);
    Task AddAsync(SunriseSunset sunriseSunset);
}
