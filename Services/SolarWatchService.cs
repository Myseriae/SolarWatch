using SolarWatch.Models;
using SolarWatch.Repositories;

namespace SolarWatch.Services;

public class SolarWatchService : ISolarWatchService
{
    private readonly IGeocodingService _geocodingService;
    private readonly ISunriseSunsetService _sunriseSunsetService;
    private readonly ICityRepository _cityRepository;
    private readonly ISunriseSunsetRepository _sunriseSunsetRepository;
    private readonly ILogger<SolarWatchService> _logger;

    public SolarWatchService(
        IGeocodingService geocodingService,
        ISunriseSunsetService sunriseSunsetService,
        ICityRepository cityRepository,
        ISunriseSunsetRepository sunriseSunsetRepository,
        ILogger<SolarWatchService> logger)
    {
        _geocodingService = geocodingService;
        _sunriseSunsetService = sunriseSunsetService;
        _cityRepository = cityRepository;
        _sunriseSunsetRepository = sunriseSunsetRepository;
        _logger = logger;
    }

    public async Task<SolarWatchResponse> GetSunriseSunsetAsync(string city, string date)
    {
        var cityEntity = await GetOrFetchCityAsync(city);
        var sunriseSunset = await GetOrFetchSunriseSunsetAsync(cityEntity, date);

        return new SolarWatchResponse(cityEntity.Name, date, sunriseSunset.Sunrise, sunriseSunset.Sunset);
    }

    private async Task<City> GetOrFetchCityAsync(string cityName)
    {
        var city = await _cityRepository.GetByNameAsync(cityName);
        if (city != null)
            return city;

        _logger.LogInformation("City '{City}' not in database, fetching from geocoding API", cityName);
        var result = await _geocodingService.GetCoordinatesAsync(cityName);

        city = new City
        {
            Name = result.Name,
            Lat = result.Lat,
            Lon = result.Lon,
            State = result.State,
            Country = result.Country
        };
        await _cityRepository.AddAsync(city);
        return city;
    }

    private async Task<SunriseSunset> GetOrFetchSunriseSunsetAsync(City city, string date)
    {
        var entry = await _sunriseSunsetRepository.GetByCityAndDateAsync(city.Id, date);
        if (entry != null)
            return entry;

        _logger.LogInformation(
            "No sunrise/sunset for '{City}' on {Date}, fetching from API", city.Name, date);
        var data = await _sunriseSunsetService.GetSunriseSunsetAsync(city.Lat, city.Lon, date);

        entry = new SunriseSunset
        {
            CityId = city.Id,
            Date = date,
            Sunrise = data.Sunrise,
            Sunset = data.Sunset
        };
        await _sunriseSunsetRepository.AddAsync(entry);
        return entry;
    }
}
