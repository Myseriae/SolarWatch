using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SolarWatch.Controllers;
using SolarWatch.Models;
using SolarWatch.Repositories;
using SolarWatch.Services;

namespace SolarWatchTest;

[TestFixture]
public class SolarWatchControllerTests
{
    private Mock<ISolarWatchService> _solarWatchServiceMock;
    private Mock<ILogger<SolarWatchController>> _loggerMock;
    private SolarWatchController _controller;

    [SetUp]
    public void SetUp()
    {
        _solarWatchServiceMock = new Mock<ISolarWatchService>();
        _loggerMock = new Mock<ILogger<SolarWatchController>>();
        _controller = new SolarWatchController(_solarWatchServiceMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task Get_ReturnsBadRequest_WhenCityIsEmpty()
    {
        var result = await _controller.Get("");
        Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
    }

    [Test]
    public async Task Get_ReturnsNotFound_WhenServiceThrowsArgumentException()
    {
        _solarWatchServiceMock
            .Setup(x => x.GetSunriseSunsetAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new ArgumentException("City not found"));

        var result = await _controller.Get("NonExistentCity123");
        Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
    }

    [Test]
    public async Task Get_ReturnsOk_WithCorrectData_WhenServiceSucceeds()
    {
        var response = new SolarWatchResponse("Budapest", "2024-06-01",
            "2024-06-01T04:00:00+00:00", "2024-06-01T19:00:00+00:00");

        _solarWatchServiceMock
            .Setup(x => x.GetSunriseSunsetAsync("Budapest", It.IsAny<string>()))
            .ReturnsAsync(response);

        var result = await _controller.Get("Budapest", "2024-06-01");

        Assert.IsInstanceOf<OkObjectResult>(result.Result);
        var value = (SolarWatchResponse)((OkObjectResult)result.Result!).Value!;
        Assert.That(value.City, Is.EqualTo("Budapest"));
        Assert.That(value.Sunrise, Is.EqualTo(response.Sunrise));
    }

    [Test]
    public async Task Get_Returns500_WhenServiceThrowsUnexpectedException()
    {
        _solarWatchServiceMock
            .Setup(x => x.GetSunriseSunsetAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Unexpected"));

        var result = await _controller.Get("Budapest");
        Assert.IsInstanceOf<ObjectResult>(result.Result);
        Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(500));
    }
}

[TestFixture]
public class SolarWatchServiceTests
{
    private Mock<IGeocodingService> _geocodingServiceMock;
    private Mock<ISunriseSunsetService> _sunriseSunsetServiceMock;
    private Mock<ICityRepository> _cityRepositoryMock;
    private Mock<ISunriseSunsetRepository> _sunriseSunsetRepositoryMock;
    private Mock<ILogger<SolarWatchService>> _loggerMock;
    private SolarWatchService _service;

    private static readonly GeocodingResult FakeGeocodingResult =
        new("Budapest", 47.5, 19.0, "Budapest", "HU");

    private static readonly SunriseSunsetData FakeSunriseSunsetData =
        new("2024-06-01T04:00:00+00:00", "2024-06-01T19:00:00+00:00");

    private static readonly City FakeCityEntity = new()
    {
        Id = 1,
        Name = "Budapest",
        Lat = 47.5,
        Lon = 19.0,
        State = "Budapest",
        Country = "HU"
    };

    private static readonly SunriseSunset FakeSunriseSunsetEntity = new()
    {
        Id = 1,
        CityId = 1,
        Date = "2024-06-01",
        Sunrise = "2024-06-01T04:00:00+00:00",
        Sunset = "2024-06-01T19:00:00+00:00"
    };

    [SetUp]
    public void SetUp()
    {
        _geocodingServiceMock = new Mock<IGeocodingService>();
        _sunriseSunsetServiceMock = new Mock<ISunriseSunsetService>();
        _cityRepositoryMock = new Mock<ICityRepository>();
        _sunriseSunsetRepositoryMock = new Mock<ISunriseSunsetRepository>();
        _loggerMock = new Mock<ILogger<SolarWatchService>>();

        _cityRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<City>()))
            .Returns(Task.CompletedTask);
        _sunriseSunsetRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<SunriseSunset>()))
            .Returns(Task.CompletedTask);

        _service = new SolarWatchService(
            _geocodingServiceMock.Object,
            _sunriseSunsetServiceMock.Object,
            _cityRepositoryMock.Object,
            _sunriseSunsetRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task GetSunriseSunset_CallsGeocodingApi_WhenCityNotInDb()
    {
        _cityRepositoryMock.Setup(r => r.GetByNameAsync("Budapest")).ReturnsAsync((City?)null);
        _geocodingServiceMock.Setup(s => s.GetCoordinatesAsync("Budapest")).ReturnsAsync(FakeGeocodingResult);
        _sunriseSunsetRepositoryMock
            .Setup(r => r.GetByCityAndDateAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync((SunriseSunset?)null);
        _sunriseSunsetServiceMock
            .Setup(s => s.GetSunriseSunsetAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<string>()))
            .ReturnsAsync(FakeSunriseSunsetData);

        await _service.GetSunriseSunsetAsync("Budapest", "2024-06-01");

        _geocodingServiceMock.Verify(s => s.GetCoordinatesAsync("Budapest"), Times.Once);
        _cityRepositoryMock.Verify(r => r.AddAsync(It.IsAny<City>()), Times.Once);
    }

    [Test]
    public async Task GetSunriseSunset_SkipsGeocodingApi_WhenCityInDb()
    {
        _cityRepositoryMock.Setup(r => r.GetByNameAsync("Budapest")).ReturnsAsync(FakeCityEntity);
        _sunriseSunsetRepositoryMock
            .Setup(r => r.GetByCityAndDateAsync(1, "2024-06-01"))
            .ReturnsAsync((SunriseSunset?)null);
        _sunriseSunsetServiceMock
            .Setup(s => s.GetSunriseSunsetAsync(47.5, 19.0, "2024-06-01"))
            .ReturnsAsync(FakeSunriseSunsetData);

        await _service.GetSunriseSunsetAsync("Budapest", "2024-06-01");

        _geocodingServiceMock.Verify(s => s.GetCoordinatesAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetSunriseSunset_CallsSunriseSunsetApi_WhenNotInDb()
    {
        _cityRepositoryMock.Setup(r => r.GetByNameAsync("Budapest")).ReturnsAsync(FakeCityEntity);
        _sunriseSunsetRepositoryMock
            .Setup(r => r.GetByCityAndDateAsync(1, "2024-06-01"))
            .ReturnsAsync((SunriseSunset?)null);
        _sunriseSunsetServiceMock
            .Setup(s => s.GetSunriseSunsetAsync(47.5, 19.0, "2024-06-01"))
            .ReturnsAsync(FakeSunriseSunsetData);

        await _service.GetSunriseSunsetAsync("Budapest", "2024-06-01");

        _sunriseSunsetServiceMock.Verify(
            s => s.GetSunriseSunsetAsync(47.5, 19.0, "2024-06-01"), Times.Once);
        _sunriseSunsetRepositoryMock.Verify(r => r.AddAsync(It.IsAny<SunriseSunset>()), Times.Once);
    }

    [Test]
    public async Task GetSunriseSunset_SkipsSunriseSunsetApi_WhenInDb()
    {
        _cityRepositoryMock.Setup(r => r.GetByNameAsync("Budapest")).ReturnsAsync(FakeCityEntity);
        _sunriseSunsetRepositoryMock
            .Setup(r => r.GetByCityAndDateAsync(1, "2024-06-01"))
            .ReturnsAsync(FakeSunriseSunsetEntity);

        var result = await _service.GetSunriseSunsetAsync("Budapest", "2024-06-01");

        _sunriseSunsetServiceMock.Verify(
            s => s.GetSunriseSunsetAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<string>()),
            Times.Never);
        Assert.That(result.City, Is.EqualTo("Budapest"));
        Assert.That(result.Sunrise, Is.EqualTo(FakeSunriseSunsetEntity.Sunrise));
    }
}
