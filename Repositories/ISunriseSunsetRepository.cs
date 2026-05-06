using SolarWatch.Models;

namespace SolarWatch.Repositories;

public interface ISunriseSunsetRepository
{
    Task<SunriseSunset?> GetByCityAndDateAsync(int cityId, string date);
    Task<SunriseSunset?> GetByIdAsync(int id);
    Task<SunriseSunset> AddAsync(SunriseSunset sunriseSunset);
    Task<SunriseSunset> UpdateAsync(SunriseSunset sunriseSunset);
    Task DeleteAsync(int id);
}