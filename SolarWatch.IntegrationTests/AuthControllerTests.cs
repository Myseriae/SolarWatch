using System.Net;
using System.Net.Http.Json;
using SolarWatch.Contracts;

namespace SolarWatch.IntegrationTests;

[Collection("IntegrationTests")]
public class AuthControllerTests : IAsyncLifetime
{
    private readonly SolarWatchWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public AuthControllerTests(SolarWatchWebApplicationFactory factory)
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
    public async Task Register_ReturnsCreated_WithValidRequest()
    {
        var response = await _client.PostAsJsonAsync("/Auth/Register",
            new RegistrationRequest("register1@test.com", "reguser1", "password123"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsOk_WithTokenInResponse_AfterRegistration()
    {
        await RegisterUserAsync("login1@test.com", "loginuser1", "password123");

        var response = await _client.PostAsJsonAsync("/Auth/Login",
            new AuthRequest("login1@test.com", "password123"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body?.Token);
    }

    private async Task RegisterUserAsync(string email, string username, string password)
    {
        var response = await _client.PostAsJsonAsync("/Auth/Register",
            new RegistrationRequest(email, username, password));
        response.EnsureSuccessStatusCode();
    }
}
