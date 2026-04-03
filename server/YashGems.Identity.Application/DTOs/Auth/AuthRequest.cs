namespace YashGems.Identity.Application.DTOs.Auth;

public record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    string PhoneNumber
);

public record LoginRequest(string Email, string Password);

public record RefreshTokenRequest(string RefreshToken);