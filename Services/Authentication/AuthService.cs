using Microsoft.AspNetCore.Identity;

namespace SolarWatch.Services.Authentication;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthService(UserManager<IdentityUser> userManager, ITokenService tokenService)
    {
        _userManager  = userManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResult> RegisterAsync(
        string email, string username, string password, string role)
    {
        var user   = new IdentityUser { UserName = username, Email = email };
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var authResult = new AuthResult(false, email, username, string.Empty);
            foreach (var error in result.Errors)
                authResult.ErrorMessages[error.Code] = error.Description;
            return authResult;
        }

        await _userManager.AddToRoleAsync(user, role);
        return new AuthResult(true, email, username, string.Empty);
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Fail(email, "Bad credentials");

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
            return Fail(email, "Bad credentials");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateToken(user, roles.FirstOrDefault() ?? string.Empty);

        return new AuthResult(true, user.Email!, user.UserName!, token);
    }

    private static AuthResult Fail(string email, string errorDescription)
    {
        var result = new AuthResult(false, email, string.Empty, string.Empty);
        result.ErrorMessages["BadCredentials"] = errorDescription;
        return result;
    }
}