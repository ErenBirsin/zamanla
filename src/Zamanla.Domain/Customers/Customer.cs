using Zamanla.Domain.Abstractions;
using Zamanla.Domain.Customers.ValueObjects;
using Zamanla.Domain.Shared;

namespace Zamanla.Domain.Customers;

public sealed class Customer : Entity, ITenantEntity
{
    private Customer() { }

    public Customer(
        FirstName firstName,
        LastName lastName,
        PhoneNumber phoneNumber,
        Email? email,
        Note? note)
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        SetPhoneNumber(phoneNumber);
        SetEmail(email);
        SetNote(note);
    }

    // kayıt sırasında TenantInterceptor doldurur
    public Guid TenantId { get; private set; }
    public FirstName FirstName { get; private set; } = default!;
    public LastName LastName { get; private set; } = default!;
    public PhoneNumber? PhoneNumber { get; private set; }
    public Email? Email { get; private set; }
    public Note? Note { get; private set; }
    public bool IsAnonymized { get; private set; }

    public string FullName => $"{FirstName.Value} {LastName.Value}";

    #region Behaviors
    public void SetFirstName(FirstName firstName)
    {
        FirstName = firstName;
    }

    public void SetLastName(LastName lastName)
    {
        LastName = lastName;
    }

    public void SetPhoneNumber(PhoneNumber phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }

    public void SetEmail(Email? email)
    {
        Email = email;
    }

    public void SetNote(Note? note)
    {
        Note = note;
    }
    // KVKK silme talebi: kişisel bilgiler gider, geçmiş randevular için kayıt kalır
    public void Anonymize()
    {
        FirstName = new FirstName("Silinmiş");
        LastName = new LastName("Müşteri");
        PhoneNumber = null;
        Email = null;
        Note = null;
        IsAnonymized = true;
    }
    #endregion
}
