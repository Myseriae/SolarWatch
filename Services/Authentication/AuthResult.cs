namespace SolarWatch.Services.Authentication;

public record AuthResult(bool Success, string Email, string UserName, string Token)
{
    // Key = error code (e.g. "DuplicateEmail"), Value = human-readable message.
    public readonly Dictionary<string, string> ErrorMessages = [];
}