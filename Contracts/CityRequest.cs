using System.ComponentModel.DataAnnotations;
namespace SolarWatch.Contracts;
public record CityRequest(
    [Required] string Name,
    string? State,
    [Required] string Country,
    double Lat,    // [Required] has no effect on value types — omit it
    double Lon);