using Microsoft.AspNetCore.Mvc;
using SolarWatch.Models;
using SolarWatch.Services;

namespace SolarWatch.Controllers;

[ApiController]
[Route("api/solarwatch")]
public class SolarWatchController : ControllerBase
{
    private readonly IGeocodingService _geocodingService;
    private readonly ISunriseSunsetService _sunriseSunsetService;
    private readonly ILogger<SolarWatchController> _logger;

    public SolarWatchController(
        IGeocodingService geocodingService,
        ISunriseSunsetService sunriseSunsetService,
        ILogger<SolarWatchController> logger)
    {
        _geocodingService = geocodingService;
        _sunriseSunsetService = sunriseSunsetService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<SolarWatchResponse>> Get(
        [FromQuery] string city,
        [FromQuery] string? date = null)
    {
        if (string.IsNullOrWhiteSpace(city))
            return BadRequest("City parameter is required");

        var resolvedDate = date ?? DateTime.Today.ToString("yyyy-MM-dd");

        try
        {
            var coordinates = await _geocodingService.GetCoordinatesAsync(city);
            var sunriseSunset = await _sunriseSunsetService.GetSunriseSunsetAsync(
                coordinates.Lat, coordinates.Lon, resolvedDate);

            return Ok(new SolarWatchResponse(
                coordinates.Name,
                resolvedDate,
                sunriseSunset.Sunrise,
                sunriseSunset.Sunset));
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
}