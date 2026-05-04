namespace SolarWatch.Models;

public class City
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lon { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public ICollection<SunriseSunset> SunriseSunsets { get; set; } = new List<SunriseSunset>();
}
