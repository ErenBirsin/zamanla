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
        var body = string.Join("\n",
            $"Merhaba {user.FirstName.Value},",
            "",
            "Zamanla hesabınız için şifre sıfırlama isteği aldık. Yeni şifrenizi belirlemek için aşağıdaki linki açın:",
            "",
            url,
            "",
            $"Link {AppUser.ForgotPasswordValidHours} saat geçerli ve bir kez kullanılabilir. Bu isteği siz yapmadıysanız bu e-postayı dikkate almayın; şifreniz değişmez.");

        try
        {
            await mailService.SendAsync(user.Email.Value, subject, body, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Şifre sıfırlama e-postası gönderilemedi: {UserId}", user.Id);
        }
    }
}
