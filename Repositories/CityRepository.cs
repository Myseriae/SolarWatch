using Microsoft.EntityFrameworkCore;
using SolarWatch.Data;
using SolarWatch.Models;

namespace SolarWatch.Repositories;

public class CityRepository : ICityRepository
{
    private readonly SolarWatchDbContext _context;

    public CityRepository(SolarWatchDbContext context)
    {
        _context = context;
    }

    public async Task<City?> GetByNameAsync(string name) =>
        await _context.Cities.FirstOrDefaultAsync(c => c.Name == name);

    public async Task AddAsync(City city)
    {
        await _context.Cities.AddAsync(city);
        await _context.SaveChangesAsync();
    }
}
