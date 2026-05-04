namespace SolarWatch.Models;

public class SunriseSunset
{
    public int Id { get; set; }
    public int CityId { get; set; }
    public string Date { get; set; } = string.Empty;
    public string Sunrise { get; set; } = string.Empty;
    public string Sunset { get; set; } = string.Empty;
    public City City { get; set; } = null!;
}
