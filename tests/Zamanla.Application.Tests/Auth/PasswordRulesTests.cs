using Shouldly;
using Zamanla.Application.Auth;

namespace Zamanla.Application.Tests.Auth;

// kayıt, davet, sıfırlama ve değiştirmedeki şifre kuralları aynı; sıfırlama validator'ı üzerinden denenir
public sealed class PasswordRulesTests
{
    private readonly ResetPasswordCommandValidator _validator = new();

    [Theory]
    [InlineData("Zamanla123!")]
    // Türkçe büyük ve küçük harfler de sayılır
    [InlineData("Şeyma2026.")]
    [InlineData("ÇAĞRI2026ı?")]
    public void ValidPassword_Passes(string password)
    {
        var result = _validator.Validate(new ResetPasswordCommand("kod", password));

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("Za1!xyz", "Şifre en az 8 karakter olmalı")]
    [InlineData("zamanla123!", "Şifre en az bir büyük harf içermeli")]
    [InlineData("ZAMANLA123!", "Şifre en az bir küçük harf içermeli")]
    [InlineData("Zamanlaaa!", "Şifre en az bir rakam içermeli")]
    [InlineData("Zamanla123", "Şifre en az bir özel karakter içermeli")]
    public void MissingRule_ReturnsItsMessage(string password, string message)
    {
        var result = _validator.Validate(new ResetPasswordCommand("kod", password));

        result.Errors.ShouldHaveSingleItem().ErrorMessage.ShouldBe(message);
    }
}
