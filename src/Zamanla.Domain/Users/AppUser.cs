using Zamanla.Domain.Abstractions;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Users.ValueObjects;

namespace Zamanla.Domain.Users;

public sealed class AppUser : Entity, ITenantEntity
{
    public const int ForgotPasswordValidHours = 1;
    private readonly List<Permission> _permissions = new();
    private AppUser() { }

    public AppUser(
        Guid tenantId,
        FirstName firstName,
        LastName lastName,
        Email email,
        Password password,
        UserRole role)
    {
        TenantId = tenantId;
        SetFirstName(firstName);
        SetLastName(lastName);
        SetEmail(email);
        SetPassword(password);
        SetRole(role);
        Activate();
    }

    public Guid TenantId { get; private set; }
    public FirstName FirstName { get; private set; } = default!;
    public LastName LastName { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public Password Password { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    // personel hesabıysa takvimdeki personel kaydı; sahipte boş
    public Guid? StaffId { get; private set; }
    // sahibin yetki listesi boştur, her şeyi yapabilir
    public IReadOnlyCollection<Permission> Permissions => _permissions;
    public bool IsOwner => Role == UserRole.Owner;
    public string FullName => $"{FirstName.Value} {LastName.Value}";
    public ForgotPasswordCode? ForgotPasswordCode { get; private set; }
    public DateTimeOffset? ForgotPasswordDate { get; private set; }
    public bool IsForgotPasswordCompleted { get; private set; }

    #region Behaviors
    public bool HasPermission(string permission)
    {
        return IsOwner || _permissions.Any(p => p.Value == permission);
    }
    public bool VerifyPassword(string password)
    {
        return Password.Verify(password);
    }

    // yeni istek eski linki geçersiz yapar
    public void CreateForgotPasswordCode(ForgotPasswordCode code, DateTimeOffset now)
    {
        ForgotPasswordCode = code;
        ForgotPasswordDate = now;
        IsForgotPasswordCompleted = false;
    }

    public bool IsForgotPasswordCodeValid(DateTimeOffset now)
    {
        return ForgotPasswordCode is not null
            && !IsForgotPasswordCompleted
            && ForgotPasswordDate!.Value.AddHours(ForgotPasswordValidHours) > now;
    }

    public void SetIsForgotPasswordCompleted(bool isForgotPasswordCompleted)
    {
        IsForgotPasswordCompleted = isForgotPasswordCompleted;
    }

    public void SetFirstName(FirstName firstName)
    {
        FirstName = firstName;
    }

    public void SetLastName(LastName lastName)
    {
        LastName = lastName;
    }

    public void SetEmail(Email email)
    {
        Email = email;
    }

    public void SetPassword(Password password)
    {
        Password = password;
    }

    public void SetRole(UserRole role)
    {
        Role = role;
    }

    public void SetStaffId(Guid? staffId)
    {
        StaffId = staffId;
    }

    public void SetPermissions(IEnumerable<Permission> permissions)
    {
        _permissions.Clear();
        _permissions.AddRange(permissions);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
    #endregion
}
