using Microsoft.AspNetCore.Identity;

namespace SolarWatch.Services.Authentication;

/// <summary>
/// Ensures the Admin and User roles exist in the database before the application
/// starts accepting requests. Safe to call on every startup (idempotent).
/// </summary>
public class AuthenticationSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AuthenticationSeeder> _logger;

    public AuthenticationSeeder(
        RoleManager<IdentityRole> roleManager,
        ILogger<AuthenticationSeeder> logger)
    {
        _roleManager = roleManager;
        _logger      = logger;
    }

    public async Task SeedRolesAsync()
    {
        await CreateRoleIfNotExistsAsync(Roles.Admin);
        await CreateRoleIfNotExistsAsync(Roles.User);
    }

    private async Task CreateRoleIfNotExistsAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
                _logger.LogInformation("Role '{RoleName}' created.", roleName);
            else
                _logger.LogError(
                    "Failed to create role '{RoleName}': {Errors}",
                    roleName,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}