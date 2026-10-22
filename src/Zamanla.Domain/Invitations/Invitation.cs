using Zamanla.Domain.Abstractions;
using Zamanla.Domain.Invitations.ValueObjects;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Users;

namespace Zamanla.Domain.Invitations;

// personele panel hesabı açması için gönderilen davet; kabul edilince hesap oluşur
public sealed class Invitation : Entity, ITenantEntity
{
    public const int ValidDays = 7;

    private readonly List<Permission> _permissions = new();

    private Invitation() { }

    public Invitation(
        Guid staffId,
        Email email,
        InvitationToken token,
        IEnumerable<Permission> permissions,
        DateTimeOffset now)
    {
        StaffId = staffId;
        Email = email;
        Token = token;
        _permissions.AddRange(permissions);
        ExpiresAt = now.AddDays(ValidDays);
    }

    // kayıt sırasında TenantInterceptor doldurur
    public Guid TenantId { get; private set; }
    public Guid StaffId { get; private set; }
    public Email Email { get; private set; } = default!;
    public InvitationToken Token { get; private set; } = default!;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? AcceptedAt { get; private set; }
    // hesap açılınca personele bu yetkiler verilir
    public IReadOnlyCollection<Permission> Permissions => _permissions;

    #region Behaviors
    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

    public void Accept(DateTimeOffset now)
    {
        AcceptedAt = now;
    }
    #endregion
}
