using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using SolarWatch.Data;
using SolarWatch.Services;

namespace SolarWatch.IntegrationTests;

[CollectionDefinition("IntegrationTests")]
public class IntegrationTestsCollection : ICollectionFixture<SolarWatchWebApplicationFactory> { }

public class SolarWatchWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:IssuerSigningKey"] = "integration-test-signing-key-32chars!"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // ConfigureAppConfiguration runs after AddAuthentication() captures the signing key
            // from config, so we must override the validation key here where we have final say.
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("integration-test-signing-key-32chars!"));
            });

            // Remove existing DbContext registrations. Calling AddDbContext a second time
            // (even for a different provider) adds both providers' infrastructure services
            // to the application service collection, which EF Core rejects at runtime.
            // Registering the context directly with isolated options avoids this conflict.
            services.RemoveAll(typeof(SolarWatchDbContext));
            services.RemoveAll(typeof(DbContextOptions<SolarWatchDbContext>));
            services.RemoveAll(typeof(DbContextOptions));

            services.AddScoped<SolarWatchDbContext>(_ =>
                new SolarWatchDbContext(
                    new DbContextOptionsBuilder<SolarWatchDbContext>()
                        .UseInMemoryDatabase(_dbName)
                        .Options));

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SolarWatchDbContext>();
            db.Database.EnsureCreated();

            var geocodingDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IGeocodingService));
            if (geocodingDescriptor != null)
                services.Remove(geocodingDescriptor);

            services.AddHttpClient<IGeocodingService, GeocodingService>()
                .ConfigurePrimaryHttpMessageHandler(() => new FakeGeocodingHandler());

            var sunriseDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ISunriseSunsetService));
            if (sunriseDescriptor != null)
                services.Remove(sunriseDescriptor);

            services.AddHttpClient<ISunriseSunsetService, SunriseSunsetService>()
                .ConfigurePrimaryHttpMessageHandler(() => new FakeSunriseSunsetHandler());
        });
    }
}

internal sealed class FakeGeocodingHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        const string json = """[{"name":"Budapest","lat":47.5,"lon":19.0,"country":"HU"}]""";
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }
}

internal sealed class FakeSunriseSunsetHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        const string json = """{"results":{"sunrise":"6:13:58 AM","sunset":"8:02:11 PM"},"status":"OK"}""";
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }
}
