namespace YashGems.Identity.Application.DTOs.Auth;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    string FullName,
    string Email
);
