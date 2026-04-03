using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using YashGems.Identity.Application.Interfaces;
using YashGems.Identity.Domain.Entities;

namespace YashGems.Identity.Infrastructure.Authentication;

public class JwtProvider : ITokenProvider
{
    private readonly IConfiguration _configuration;

    public JwtProvider(IConfiguration configuration)
        => _configuration = configuration;

    public (string AccessToken, string Jti) CreateAccessToken(User user)
    {
        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, jti),
            new(ClaimTypes.Role, user.Role.ToString()) ,
            new("kyc_status", user.KycStatus.ToString())
        };

        var textKey = _configuration["Jwt:Key"] ?? "";
        var expiryMinutes = double.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "15");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(textKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), jti);
    }

    public RefreshToken CreateRefreshToken(Guid userId, string accessTokenJti)
    {
        var expriyDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

        return new RefreshToken
        {
            UserId = userId,
            AccessTokenJti = accessTokenJti,
            AccessToken = GenerateRefreshTokenString(),
            ExpiryDate = DateTime.UtcNow.AddDays(expriyDays),
            CreatedAt = DateTime.UtcNow
        };
    }

    public string GenerateRefreshTokenString()
    {
        var randomNumber = new byte[64];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }
}
