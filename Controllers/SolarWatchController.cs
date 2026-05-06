using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarWatch.Models;
using SolarWatch.Repositories;
using SolarWatch.Services.Authentication;
using SolarWatch.Services;

namespace SolarWatch.Controllers;

[ApiController]
[Route("api/sunrise-sunset")]
public class SolarWatchController : ControllerBase
{
    private readonly IGeocodingService _geocodingService;
    private readonly ISunriseSunsetService _sunriseSunsetService;
    private readonly ILogger<SolarWatchController> _logger;
    private readonly ICityRepository _cityRepository;
    private readonly ISunriseSunsetRepository _sunriseSunsetRepository;

    public SolarWatchController(
        IGeocodingService geocodingService,
        ISunriseSunsetService sunriseSunsetService,
        ILogger<SolarWatchController> logger,
        ICityRepository cityRepository,
        ISunriseSunsetRepository sunriseSunsetRepository)
    {
        _geocodingService        = geocodingService;
        _sunriseSunsetService    = sunriseSunsetService;
        _logger                  = logger;
        _cityRepository          = cityRepository;
        _sunriseSunsetRepository = sunriseSunsetRepository;
    }


    [HttpGet]
    [Authorize(Roles = $"{Roles.User}, {Roles.Admin}")]
    public async Task<ActionResult<SolarWatchResponse>> Get(
        [FromQuery] string city,
        [FromQuery] string? date = null)
    {
        if (string.IsNullOrWhiteSpace(city))
            return BadRequest("City parameter is required");

        var resolvedDate = date ?? DateTime.Today.ToString("yyyy-MM-dd");

        try
        {
            var cityEntity = await _cityRepository.GetByNameAsync(city);

            if (cityEntity is null)
            {
                var result = await _geocodingService.GetCoordinatesAsync(city);
                cityEntity = new City
                {
                    Name    = result.Name,
                    Lat     = result.Lat,
                    Lon     = result.Lon,
                    State   = result.State,
                    Country = result.Country
                };
                await _cityRepository.AddAsync(cityEntity);
            }

            var sunriseSunset = await _sunriseSunsetRepository.GetByCityAndDateAsync(cityEntity.Id, resolvedDate);

            if (sunriseSunset is null)
            {
                var sunData = await _sunriseSunsetService.GetSunriseSunsetAsync(
                    cityEntity.Lat, cityEntity.Lon, resolvedDate);

                sunriseSunset = new SunriseSunset
                {
                    CityId  = cityEntity.Id,
                    Date    = resolvedDate,
                    Sunrise = sunData.Sunrise,
                    Sunset  = sunData.Sunset
                };
                await _sunriseSunsetRepository.AddAsync(sunriseSunset);
            }

            return Ok(new SolarWatchResponse(
                cityEntity.Name, resolvedDate, sunriseSunset.Sunrise, sunriseSunset.Sunset));
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning(e, "City not found: {City}", city);
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error for city: {City}", city);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{Roles.User}, {Roles.Admin}")]
    public async Task<ActionResult<SunriseSunset>> GetById(int id)
    {
        var entry = await _sunriseSunsetRepository.GetByIdAsync(id);
        if (entry is null) return NotFound($"Record with ID {id} not found.");
        return Ok(entry);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<SunriseSunset>> Create([FromBody] SunriseSunset entry)
    {
        var created = await _sunriseSunsetRepository.AddAsync(entry);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<SunriseSunset>> Update(int id, [FromBody] SunriseSunset entry)
    {
        var existing = await _sunriseSunsetRepository.GetByIdAsync(id);
        if (existing is null) return NotFound($"Record with ID {id} not found.");

        existing.CityId  = entry.CityId;
        existing.Date    = entry.Date;
        existing.Sunrise = entry.Sunrise;
        existing.Sunset  = entry.Sunset;

        var updated = await _sunriseSunsetRepository.UpdateAsync(existing);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _sunriseSunsetRepository.GetByIdAsync(id);
        if (existing is null) return NotFound($"Record with ID {id} not found.");
        await _sunriseSunsetRepository.DeleteAsync(id);
        _logger.LogInformation("Admin deleted SunriseSunset ID {Id}.", id);
        return NoContent();
    }
}