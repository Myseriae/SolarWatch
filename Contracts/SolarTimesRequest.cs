using System.ComponentModel.DataAnnotations;
namespace SolarWatch.Contracts;
public record SolarTimesRequest(
    int CityId,         // [Required] has no effect on value types — omit it
    DateOnly Date,      // same
    [Required] string Sunrise,
    [Required] string Sunset);