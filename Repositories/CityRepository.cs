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

    public async Task<City> AddAsync(City city)
    {
        await _context.Cities.AddAsync(city);
        await _context.SaveChangesAsync();
        return city;
    }
    public async Task<IEnumerable<City>> GetAllAsync() =>
        await _context.Cities.ToListAsync();

    public async Task<City?> GetByIdAsync(int id) =>
        await _context.Cities.FindAsync(id);

    public async Task<City> UpdateAsync(City city)
    {
        _context.Cities.Update(city);
        await _context.SaveChangesAsync();
        return city;
    }

    public async Task DeleteAsync(int id)
    {
        var city = await _context.Cities.FindAsync(id);
        if (city is not null)
        {
            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();
        }
    }
}
