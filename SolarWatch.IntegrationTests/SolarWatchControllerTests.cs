using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SolarWatch.Contracts;

namespace SolarWatch.IntegrationTests;

[Collection("IntegrationTests")]
public class SolarWatchControllerTests : IAsyncLifetime
{
    private readonly SolarWatchWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public SolarWatchControllerTests(SolarWatchWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetSunriseSunset_Returns401_WithoutToken()
    {
        var response = await _client.GetAsync("/api/sunrise-sunset?city=Budapest");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSunriseSunset_ReturnsOk_WithValidToken()
    {
        await RegisterUserAsync("solar1@test.com", "solaruser1", "password123");
        var token = await GetTokenAsync("solar1@test.com", "password123");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/sunrise-sunset?city=Budapest");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task RegisterUserAsync(string email, string username, string password)
    {
        var response = await _client.PostAsJsonAsync("/Auth/Register",
            new RegistrationRequest(email, username, password));
        response.EnsureSuccessStatusCode();
    }

    private async Task<string> GetTokenAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/Auth/Login",
            new AuthRequest(email, password));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return body!.Token;
    }
}
