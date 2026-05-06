using SolarWatch.Models;

namespace SolarWatch.Repositories;

public interface ICityRepository
{
    Task<City?> GetByNameAsync(string name);
    Task<City> AddAsync(City city);
    Task<IEnumerable<City>> GetAllAsync();
    Task<City?> GetByIdAsync(int id);
    Task<City> UpdateAsync(City city);
    Task DeleteAsync(int id);
}
