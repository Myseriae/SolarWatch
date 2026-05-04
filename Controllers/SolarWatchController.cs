using Microsoft.AspNetCore.Mvc;
using SolarWatch.Models;
using SolarWatch.Services;

namespace SolarWatch.Controllers;

[ApiController]
[Route("api/solarwatch")]
public class SolarWatchController : ControllerBase
{
    private readonly ISolarWatchService _solarWatchService;
    private readonly ILogger<SolarWatchController> _logger;

    public SolarWatchController(ISolarWatchService solarWatchService, ILogger<SolarWatchController> logger)
    {
        _solarWatchService = solarWatchService;
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
            var response = await _solarWatchService.GetSunriseSunsetAsync(city, resolvedDate);
            return Ok(response);
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
