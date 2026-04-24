using System.Text.Json.Serialization;

namespace SolarWatch.Models;

public record GeocodingResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("lat")] double Lat,
    [property: JsonPropertyName("lon")] double Lon
);