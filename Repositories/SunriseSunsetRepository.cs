using Microsoft.EntityFrameworkCore;
using SolarWatch.Data;
using SolarWatch.Models;

namespace SolarWatch.Repositories;

public class SunriseSunsetRepository : ISunriseSunsetRepository
{
    private readonly SolarWatchDbContext _context;

    public SunriseSunsetRepository(SolarWatchDbContext context)
    {
        _context = context;
    }

    public async Task<SunriseSunset?> GetByCityAndDateAsync(int cityId, string date) =>
        await _context.SunriseSunsets
            .FirstOrDefaultAsync(ss => ss.CityId == cityId && ss.Date == date);

    public async Task AddAsync(SunriseSunset sunriseSunset)
    {
        await _context.SunriseSunsets.AddAsync(sunriseSunset);
        await _context.SaveChangesAsync();
    }
}
