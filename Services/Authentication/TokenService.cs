using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;   // JsonWebTokenHandler + JwtRegisteredClaimNames
using Microsoft.IdentityModel.Tokens;

namespace SolarWatch.Services.Authentication;

public class TokenService : ITokenService
{
    private const int ExpirationMinutes = 30;

    private readonly string _validIssuer;
    private readonly string _validAudience;
    private readonly string _issuerSigningKey;

    public TokenService(IConfiguration configuration)
    {
        _validIssuer = configuration["Jwt:ValidIssuer"]
                       ?? throw new InvalidOperationException("Jwt:ValidIssuer is missing.");
        _validAudience = configuration["Jwt:ValidAudience"]
                         ?? throw new InvalidOperationException("Jwt:ValidAudience is missing.");
        _issuerSigningKey = configuration["Jwt:IssuerSigningKey"]
                            ?? throw new InvalidOperationException(
                                "Jwt:IssuerSigningKey is missing. " +
                                "Set it via: dotnet user-secrets set \"Jwt:IssuerSigningKey\" \"<secret>\"");
    }

    public string CreateToken(IdentityUser user, string role)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject            = new ClaimsIdentity(CreateClaims(user, role)),
            Expires            = DateTime.UtcNow.AddMinutes(ExpirationMinutes),
            Issuer             = _validIssuer,
            Audience           = _validAudience,
            SigningCredentials = CreateSigningCredentials()
        };

        // JsonWebTokenHandler is the .NET 10 replacement for the obsolete JwtSecurityTokenHandler.
        // CreateToken() returns the token string directly — no .WriteToken() call needed.
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private static List<Claim> CreateClaims(IdentityUser user, string role)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),                              // user's unique ID
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),            // token's unique ID
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),               // issued-at timestamp
            new(ClaimTypes.NameIdentifier, user.Id),                               // readable by HttpContext.User
            new(ClaimTypes.Name,  user.UserName!),
            new(ClaimTypes.Email, user.Email!)
        };

        if (!string.IsNullOrEmpty(role))
            claims.Add(new Claim(ClaimTypes.Role, role));                          // required for [Authorize(Roles)]

        return claims;
    }

    private SigningCredentials CreateSigningCredentials()
        => new(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_issuerSigningKey)),
            SecurityAlgorithms.HmacSha256);
}