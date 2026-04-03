namespace YashGems.Identity.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    public string AccessToken { get; set; } = string.Empty;
    public string AccessTokenJti { get; set; } = string.Empty;
    public string? ReplacedByRefreshToken { get; set; }

    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    public bool IsActive => RevokedAt == null && !IsExpired;

    public virtual User? User { get; set; }
}
