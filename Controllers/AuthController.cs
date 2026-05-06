using Microsoft.AspNetCore.Mvc;
using SolarWatch.Contracts;
using SolarWatch.Services.Authentication;

namespace SolarWatch.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>
    /// Registers a new user. All public registrations receive the User role.
    /// </summary>
    [HttpPost("Register")]
    [ProducesResponseType(typeof(RegistrationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegistrationResponse>> Register(
        [FromBody] RegistrationRequest request)
    {
        var result = await _authService.RegisterAsync(
            request.Email, request.Username, request.Password, Roles.User);

        if (!result.Success)
        {
            AddErrors(result);
            return ValidationProblem();
        }

        return CreatedAtAction(
            nameof(Register),
            new RegistrationResponse(result.Email, result.UserName));
    }

    /// <summary>Returns a signed JWT token on successful authentication.</summary>
    [HttpPost("Login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] AuthRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (!result.Success)
        {
            AddErrors(result);
            return ValidationProblem();
        }

        return Ok(new AuthResponse(result.Email, result.UserName, result.Token));
    }

    private void AddErrors(AuthResult result)
    {
        foreach (var (key, message) in result.ErrorMessages)
            ModelState.AddModelError(key, message);
    }
}