using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SolarWatch.Data;
using SolarWatch.Repositories;
using SolarWatch.Services;
using SolarWatch.Services.Authentication;

var builder = WebApplication.CreateBuilder(args);

AddServices();
AddDbContext();
AddAuthentication();
AddIdentity();

var app = builder.Build();

await SeedRolesAsync();
await ApplyMigrationsAsync();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

void AddServices()
{
    builder.Services.AddControllers();
    builder.Services.AddOpenApi();

    builder.Services.AddHttpClient<IGeocodingService, GeocodingService>();
    builder.Services.AddHttpClient<ISunriseSunsetService, SunriseSunsetService>();

    builder.Services.AddScoped<ICityRepository, CityRepository>();
    builder.Services.AddScoped<ISunriseSunsetRepository, SunriseSunsetRepository>();

    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<AuthenticationSeeder>();
}

void AddDbContext()
{
    builder.Services.AddDbContext<SolarWatchDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' is missing from configuration.")));
}

void AddAuthentication()
{
    var validIssuer = builder.Configuration["Jwt:ValidIssuer"]
                      ?? throw new InvalidOperationException("Jwt:ValidIssuer is missing.");
    var validAudience = builder.Configuration["Jwt:ValidAudience"]
                        ?? throw new InvalidOperationException("Jwt:ValidAudience is missing.");
    var issuerSigningKey = builder.Configuration["Jwt:IssuerSigningKey"]
                           ?? throw new InvalidOperationException(
                               "Jwt:IssuerSigningKey is missing. " +
                               "Set it via user secrets: " +
                               "dotnet user-secrets set \"Jwt:IssuerSigningKey\" \"<secret>\"");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ClockSkew                = TimeSpan.Zero,
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer              = validIssuer,
                ValidAudience            = validAudience,
                IssuerSigningKey         = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(issuerSigningKey))
            };
        });
}

void AddIdentity()
{
    builder.Services
        .AddIdentityCore<IdentityUser>(options =>
        {
            options.Password.RequireDigit           = false;
            options.Password.RequiredLength         = 6;
            options.Password.RequireLowercase       = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase       = false;
            options.User.RequireUniqueEmail         = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<SolarWatchDbContext>();
}

async Task SeedRolesAsync()
{
    using var scope = app.Services.CreateScope();
    var seeder      = scope.ServiceProvider.GetRequiredService<AuthenticationSeeder>();
    await seeder.SeedRolesAsync();
}

async Task ApplyMigrationsAsync()
{
    using var scope = app.Services.CreateScope();
    var context     = scope.ServiceProvider.GetRequiredService<SolarWatchDbContext>();
    if (context.Database.IsRelational())
        await context.Database.MigrateAsync();
}

public partial class Program { }
