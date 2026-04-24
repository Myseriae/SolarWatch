using System.Text.Json.Serialization;

namespace SolarWatch.Models;

public record SunriseSunsetData(
    [property: JsonPropertyName("sunrise")] string Sunrise,
    [property: JsonPropertyName("sunset")] string Sunset
);

public record SunriseSunsetApiResponse(
    [property: JsonPropertyName("results")] SunriseSunsetData Results,
    [property: JsonPropertyName("status")] string Status
);