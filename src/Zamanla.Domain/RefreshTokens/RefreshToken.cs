using Zamanla.Domain.Abstractions;
using Zamanla.Domain.RefreshTokens.ValueObjects;

namespace Zamanla.Domain.RefreshTokens;

public sealed class RefreshToken : Entity
{
    private RefreshToken() { }

    public RefreshToken(Token token, Guid userId, ExpiresDate expiresDate)
    {
        SetToken(token);
        SetUserId(userId);
        SetIsActive(new(true));
        SetExpiresDate(expiresDate);
    }

    public Token Token { get; private set; } = default!;
    public Guid UserId { get; private set; }
    public IsActive IsActive { get; private set; } = default!;
    public ExpiresDate ExpiresDate { get; private set; } = default!;

    #region Behaviors
    public void SetToken(Token token)
    {
        Token = token;
    }

    public void SetUserId(Guid userId)
    {
        UserId = userId;
    }

    public void SetIsActive(IsActive isActive)
    {
        IsActive = isActive;
    }

    public void SetExpiresDate(ExpiresDate expiresDate)
    {
        ExpiresDate = expiresDate;
    }
    #endregion
}
