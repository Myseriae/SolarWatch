using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SolarWatch.Controllers;
using SolarWatch.Models;
using SolarWatch.Services;

namespace SolarWatchTest;

[TestFixture]
public class SolarWatchControllerTests
{
    private Mock<IGeocodingService> _geocodingServiceMock;
    private Mock<ISunriseSunsetService> _sunriseSunsetServiceMock;
    private Mock<ILogger<SolarWatchController>> _loggerMock;
    private SolarWatchController _controller;

    [SetUp]
    public void SetUp()
    {
        _geocodingServiceMock = new Mock<IGeocodingService>();
        _sunriseSunsetServiceMock = new Mock<ISunriseSunsetService>();
        _loggerMock = new Mock<ILogger<SolarWatchController>>();
        _controller = new SolarWatchController(
            _geocodingServiceMock.Object,
            _sunriseSunsetServiceMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task Get_ReturnsBadRequest_WhenCityIsEmpty()
    {
        var result = await _controller.Get("");
        Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
    }

    [Test]
    public async Task Get_ReturnsNotFound_WhenCityDoesNotExist()
    {
        _geocodingServiceMock
            .Setup(x => x.GetCoordinatesAsync(It.IsAny<string>()))
            .ThrowsAsync(new ArgumentException("City not found"));

        var result = await _controller.Get("NonExistentCity123");
        Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
    }

    [Test]
    public async Task Get_ReturnsOk_WithCorrectData_WhenCityExists()
    {
        var coordinates = new GeocodingResult("Budapest", 47.5, 19.0);
        var sunriseSunset = new SunriseSunsetData("2024-06-01T04:00:00+00:00", "2024-06-01T19:00:00+00:00");

        _geocodingServiceMock
            .Setup(x => x.GetCoordinatesAsync("Budapest"))
            .ReturnsAsync(coordinates);
        _sunriseSunsetServiceMock
            .Setup(x => x.GetSunriseSunsetAsync(47.5, 19.0, It.IsAny<string>()))
            .ReturnsAsync(sunriseSunset);

        var result = await _controller.Get("Budapest");

        Assert.IsInstanceOf<OkObjectResult>(result.Result);
        var response = (SolarWatchResponse)((OkObjectResult)result.Result!).Value!;
        Assert.That(response.City, Is.EqualTo("Budapest"));
        Assert.That(response.Sunrise, Is.EqualTo(sunriseSunset.Sunrise));
    }

    [Test]
    public async Task Get_Returns500_WhenUnexpectedErrorOccurs()
    {
        _geocodingServiceMock
            .Setup(x => x.GetCoordinatesAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Unexpected"));

        var result = await _controller.Get("Budapest");
        Assert.IsInstanceOf<ObjectResult>(result.Result);
        Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(500));
    }
}