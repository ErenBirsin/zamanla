using Zamanla.Domain.Abstractions;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Users.ValueObjects;

namespace Zamanla.Domain.Users;

public sealed class AppUser : Entity, ITenantEntity
{
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

    public string FullName => $"{FirstName.Value} {LastName.Value}";

    #region Behaviors
    public bool VerifyPassword(string password)
    {
        return Password.Verify(password);
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
