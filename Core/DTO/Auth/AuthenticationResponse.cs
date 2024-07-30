namespace Core.DTO.Auth;

public class AuthenticationResponse
{
    public string? Email { get; init; } = string.Empty;
    public string? Token { get; init; } = string.Empty;
    public DateTime Expiration { get; init; }
}