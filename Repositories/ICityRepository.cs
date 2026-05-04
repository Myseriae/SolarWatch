using SolarWatch.Models;

namespace SolarWatch.Repositories;

public interface ICityRepository
{
    Task<City?> GetByNameAsync(string name);
    Task AddAsync(City city);
}
