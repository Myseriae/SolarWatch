using Microsoft.EntityFrameworkCore;
using SolarWatch.Data;
using SolarWatch.Repositories;
using SolarWatch.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHttpClient<IGeocodingService, GeocodingService>();
builder.Services.AddHttpClient<ISunriseSunsetService, SunriseSunsetService>();

builder.Services.AddDbContext<SolarWatchDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ISunriseSunsetRepository, SunriseSunsetRepository>();
builder.Services.AddScoped<ISolarWatchService, SolarWatchService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
