using Microsoft.AspNetCore.Identity;

namespace SolarWatch.Services.Authentication;

public interface ITokenService
{
    /// <summary>Creates a signed JWT for the given user with the specified role.</summary>
    string CreateToken(IdentityUser user, string role);
}