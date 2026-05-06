namespace SolarWatch.Contracts;
public record SolarTimesResponse(int Id, int CityId, DateOnly Date, string Sunrise, string Sunset);