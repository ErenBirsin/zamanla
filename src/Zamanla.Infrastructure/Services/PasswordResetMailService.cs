using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zamanla.Application.Services;
using Zamanla.Domain.Users;
using Zamanla.Infrastructure.Options;

namespace Zamanla.Infrastructure.Services;

internal sealed class PasswordResetMailService(
    IMailService mailService,
    IOptions<ClientOptions> clientOptions,
    ILogger<PasswordResetMailService> logger) : IPasswordResetMailService
{
    public async Task SendAsync(AppUser user, string code, CancellationToken cancellationToken = default)
    {
        var url = $"{clientOptions.Value.BaseUrl}/sifre-sifirla/{code}";
        var subject = "Şifre sıfırlama · Zamanla";
        var text = string.Join("\n",
            $"Merhaba {user.FirstName.Value},",
            "",
            "Zamanla hesabınız için şifre sıfırlama isteği aldık. Yeni şifrenizi belirlemek için aşağıdaki linki açın:",
            "",
            url,
            "",
            $"Link {AppUser.ForgotPasswordValidHours} saat geçerli ve bir kez kullanılabilir. Bu isteği siz yapmadıysanız bu e-postayı dikkate almayın; şifreniz değişmez.");
        var html = MailLayout.Render("Şifrenizi sıfırlayın",
            MailLayout.Paragraph($"Merhaba {user.FirstName.Value},") +
            MailLayout.Paragraph("Zamanla hesabınız için şifre sıfırlama isteği aldık. Yeni şifrenizi belirlemek için aşağıdaki düğmeye basın.") +
            MailLayout.Button("Yeni şifre belirle", url) +
            MailLayout.LinkFallback(url) +
            MailLayout.Note($"Link {AppUser.ForgotPasswordValidHours} saat geçerli ve bir kez kullanılabilir. Bu isteği siz yapmadıysanız bu e-postayı dikkate almayın; şifreniz değişmez."));

        try
        {
            await mailService.SendAsync(user.Email.Value, subject, html, text, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Şifre sıfırlama e-postası gönderilemedi: {UserId}", user.Id);
        }
    }
}
