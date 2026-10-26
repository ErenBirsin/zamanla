using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zamanla.Application.Services;
using Zamanla.Application.Tenants;
using Zamanla.Domain.Invitations;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Tenants;
using Zamanla.Infrastructure.Options;

namespace Zamanla.Infrastructure.Services;

internal sealed class InvitationMailService(
    ITenantRepository tenantRepository,
    IStaffRepository staffRepository,
    IMailService mailService,
    IOptions<ClientOptions> clientOptions,
    ILogger<InvitationMailService> logger) : IInvitationMailService
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("tr-TR");

    public async Task<InvitationLink> SendAsync(Invitation invitation, string token, CancellationToken cancellationToken = default)
    {
        var tenant = await tenantRepository.FirstAsync(t => t.Id == invitation.TenantId, cancellationToken, isTrackingActive: false);
        var staff = await staffRepository.FirstAsync(s => s.Id == invitation.StaffId, cancellationToken, isTrackingActive: false);
        var url = $"{clientOptions.Value.BaseUrl}/davet/{tenant.Slug.Value}/{token}";

        var timeZone = await tenantRepository.GetTimeZoneAsync(tenant.Id, cancellationToken);
        var localExpires = TimeZoneInfo.ConvertTime(invitation.ExpiresAt, timeZone);
        var subject = $"Panel davetiniz · {tenant.Name.Value}";
        var text = string.Join("\n",
            $"Merhaba {staff.FirstName.Value},",
            "",
            $"{tenant.Name.Value} sizi Zamanla paneline davet etti. Randevularınızı görmek ve yönetmek için aşağıdaki linkten şifrenizi belirleyin:",
            "",
            url,
            "",
            $"Link {localExpires.ToString("d MMMM HH:mm", Culture)} tarihine kadar geçerli.");
        var html = MailLayout.Render("Panele davet edildiniz",
            MailLayout.Paragraph($"Merhaba {staff.FirstName.Value},") +
            MailLayout.Paragraph($"{tenant.Name.Value} sizi Zamanla paneline davet etti. Randevularınızı görmek ve yönetmek için şifrenizi belirleyin.") +
            MailLayout.Button("Şifremi belirle", url) +
            MailLayout.LinkFallback(url) +
            MailLayout.Note($"Link {localExpires.ToString("d MMMM HH:mm", Culture)} tarihine kadar geçerli."));

        try
        {
            await mailService.SendAsync(invitation.Email.Value, subject, html, text, cancellationToken);
            return new InvitationLink(url, true);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Davet e-postası gönderilemedi: {InvitationId}", invitation.Id);
            return new InvitationLink(url, false);
        }
    }
}
