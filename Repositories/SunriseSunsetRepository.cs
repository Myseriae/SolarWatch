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

    public async Task<SunriseSunset?> GetByIdAsync(int id) =>
        await _context.SunriseSunsets.FindAsync(id);

    public async Task<SunriseSunset> AddAsync(SunriseSunset sunriseSunset)
    {
        await _context.SunriseSunsets.AddAsync(sunriseSunset);
        await _context.SaveChangesAsync();
        return sunriseSunset;
    }

    public async Task<SunriseSunset> UpdateAsync(SunriseSunset sunriseSunset)
    {
        _context.SunriseSunsets.Update(sunriseSunset);
        await _context.SaveChangesAsync();
        return sunriseSunset;
    }

    public async Task DeleteAsync(int id)
    {
        var entry = await _context.SunriseSunsets.FindAsync(id);
        if (entry is not null)
        {
            _context.SunriseSunsets.Remove(entry);
            await _context.SaveChangesAsync();
        }
    }
}