namespace SolarWatch.Contracts;
public record CityResponse(int Id, string Name, string? State, string? Country, double Lat, double Lon);