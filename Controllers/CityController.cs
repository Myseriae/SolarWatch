using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarWatch.Contracts;
using SolarWatch.Models;
using SolarWatch.Services.Authentication;
using SolarWatch.Repositories;

namespace SolarWatch.Controllers;

[ApiController]
[Route("api/cities")]
public class CityController : ControllerBase
{
    private readonly ICityRepository _cityRepository;
    private readonly ILogger<CityController> _logger;

    public CityController(ICityRepository cityRepository, ILogger<CityController> logger)
    {
        _cityRepository = cityRepository;
        _logger         = logger;
    }

    // -------------------------------------------------------------------------
    // Read endpoints — available to both User and Admin
    // -------------------------------------------------------------------------

    [HttpGet]
    [Authorize(Roles = $"{Roles.User}, {Roles.Admin}")]
    [ProducesResponseType(typeof(IEnumerable<CityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<CityResponse>>> GetAll()
    {
        var cities = await _cityRepository.GetAllAsync();
        return Ok(cities.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{Roles.User}, {Roles.Admin}")]
    [ProducesResponseType(typeof(CityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CityResponse>> GetById(int id)
    {
        var city = await _cityRepository.GetByIdAsync(id);
        if (city is null) return NotFound($"City with ID {id} not found.");
        return Ok(ToResponse(city));
    }

    // -------------------------------------------------------------------------
    // Write endpoints — Admin only
    // -------------------------------------------------------------------------

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(CityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CityResponse>> Create([FromBody] CityRequest request)
    {
        var city = new City
        {
            Name    = request.Name,
            State   = request.State,
            Country = request.Country,
            Lat     = request.Lat,
            Lon     = request.Lon
        };

        var created = await _cityRepository.AddAsync(city);
        _logger.LogInformation("Admin created city ID {Id}: {Name}", created.Id, created.Name);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToResponse(created));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(CityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CityResponse>> Update(int id, [FromBody] CityRequest request)
    {
        var existing = await _cityRepository.GetByIdAsync(id);
        if (existing is null) return NotFound($"City with ID {id} not found.");

        existing.Name    = request.Name;
        existing.State   = request.State;
        existing.Country = request.Country;
        existing.Lat     = request.Lat;
        existing.Lon     = request.Lon;

        var updated = await _cityRepository.UpdateAsync(existing);
        _logger.LogInformation("Admin updated city ID {Id}.", id);
        return Ok(ToResponse(updated));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _cityRepository.GetByIdAsync(id);
        if (existing is null) return NotFound($"City with ID {id} not found.");

        await _cityRepository.DeleteAsync(id);
        _logger.LogInformation("Admin deleted city ID {Id}.", id);
        return NoContent();
    }

    private static CityResponse ToResponse(City city)
        => new(city.Id, city.Name, city.State, city.Country, city.Lat, city.Lon);
}