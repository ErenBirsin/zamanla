using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using Zamanla.Application.Services;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.Customers;
using Zamanla.Domain.SalonServices;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Tenants;
using Zamanla.Infrastructure.Options;

namespace Zamanla.Infrastructure.Services;

internal sealed class AppointmentNotificationService(
    IAppointmentRepository appointmentRepository,
    ICustomerRepository customerRepository,
    IStaffRepository staffRepository,
    ISalonServiceRepository salonServiceRepository,
    ITenantRepository tenantRepository,
    IMailService mailService,
    ISmsService smsService,
    IOptions<ClientOptions> clientOptions,
    ILogger<AppointmentNotificationService> logger) : IAppointmentNotificationService
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("tr-TR");

    public async Task SendConfirmationAsync(Guid appointmentId, string? manageToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var details = await GetDetailsAsync(appointmentId, cancellationToken);

            var footer = manageToken is null
                ? "Randevunuzu değiştirmek ya da iptal etmek için salonu arayabilirsiniz."
                : $"Randevunuzu görüntülemek, saatini değiştirmek ya da iptal etmek için:\n{clientOptions.Value.BaseUrl}/b/{details.Slug}/r/{manageToken}";

            var subject = $"Randevunuz alındı · {details.SalonName}";
            var body = string.Join("\n",
                $"Merhaba {details.CustomerFirstName},",
                "",
                $"{details.SalonName} randevunuz oluşturuldu.",
                "",
                BuildSummary(details),
                "",
                footer);
            var sms = $"{details.SalonName}: Randevunuz {details.LocalStart.ToString("d MMMM HH:mm", Culture)}. Randevu no: {details.AppointmentNumber}";

            await TrySendMailAsync(details, subject, body, cancellationToken);
            await SendSmsAsync(details, sms, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Randevu onayı gönderilemedi: {AppointmentId}", appointmentId);
        }
    }

    public async Task<bool> SendReminderAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var details = await GetDetailsAsync(appointmentId, cancellationToken);

            var footer = details.IsOnline
                ? "Randevunuzu değiştirmeniz gerekirse onay e-postasındaki linki kullanabilir ya da salonu arayabilirsiniz."
                : "Gelemeyecekseniz lütfen salonu arayarak haber verin.";

            var subject = $"Randevu hatırlatması · {details.SalonName}";
            var body = string.Join("\n",
                $"Merhaba {details.CustomerFirstName},",
                "",
                $"{details.SalonName} randevunuzu hatırlatmak istedik.",
                "",
                BuildSummary(details),
                "",
                footer);
            var sms = $"{details.SalonName}: Randevunuzu hatırlatırız, {details.LocalStart.ToString("d MMMM HH:mm", Culture)}. Gelemeyecekseniz lütfen haber verin.";

            // e-posta gitmezse SMS de gönderilmez; ikisi bir sonraki turda birlikte denenir
            if (!await TrySendMailAsync(details, subject, body, cancellationToken))
            {
                return false;
            }
            await SendSmsAsync(details, sms, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Randevu hatırlatması gönderilemedi: {AppointmentId}", appointmentId);
            return false;
        }
    }

    // e-postası olmayan müşteride gönderilecek bir şey yok, başarılı sayılır
    private async Task<bool> TrySendMailAsync(NotificationDetails details, string subject, string body, CancellationToken cancellationToken)
    {
        if (details.Email is null)
        {
            return true;
        }

        try
        {
            await mailService.SendAsync(details.Email, subject, body, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "{AppointmentNumber} için e-posta gönderilemedi", details.AppointmentNumber);
            return false;
        }
    }

    private async Task SendSmsAsync(NotificationDetails details, string sms, CancellationToken cancellationToken)
    {
        if (details.PhoneNumber is not null)
        {
            await smsService.SendAsync(details.PhoneNumber, sms, cancellationToken);
        }
    }

    private static string BuildSummary(NotificationDetails details)
    {
        List<string> lines =
        [
            $"Tarih: {details.LocalStart.ToString("d MMMM yyyy dddd, HH:mm", Culture)}",
            $"Hizmet: {string.Join(", ", details.ServiceNames)}",
            $"Personel: {details.StaffFullName}",
            $"Randevu no: {details.AppointmentNumber}"
        ];
        if (details.SalonAddress is not null)
        {
            lines.Add($"Adres: {details.SalonAddress}");
        }
        if (details.SalonPhoneNumber is not null)
        {
            lines.Add($"Telefon: {FormatPhone(details.SalonPhoneNumber)}");
        }

        return string.Join("\n", lines);
    }

    // +902160000000 → 0216 000 00 00
    private static string FormatPhone(string phoneNumber) =>
        phoneNumber.Length == 13
            ? $"0{phoneNumber[3..6]} {phoneNumber[6..9]} {phoneNumber[9..11]} {phoneNumber[11..]}"
            : phoneNumber;

    private async Task<NotificationDetails> GetDetailsAsync(Guid appointmentId, CancellationToken cancellationToken)
    {
        var online = Source.Online.Value;
        var appointment = await appointmentRepository
            .Where(a => a.Id == appointmentId)
            .Select(a => new
            {
                a.TenantId,
                a.CustomerId,
                a.StaffId,
                AppointmentNumber = a.AppointmentNumber.Value,
                IsOnline = a.Source.Value == online,
                a.Time.Start,
                SalonServiceIds = a.SalonServices.Select(s => s.SalonServiceId).ToList()
            })
            .FirstAsync(cancellationToken);

        var customer = await customerRepository
            .Where(c => c.Id == appointment.CustomerId)
            .Select(c => new
            {
                FirstName = c.FirstName.Value,
                Email = c.Email != null ? c.Email.Value : null,
                PhoneNumber = c.PhoneNumber != null ? c.PhoneNumber.Value : null
            })
            .FirstAsync(cancellationToken);

        var staffFullName = await staffRepository.GetAllIncludingDeleted()
            .Where(s => s.Id == appointment.StaffId)
            .Select(s => s.FirstName.Value + " " + s.LastName.Value)
            .FirstAsync(cancellationToken);

        var serviceNames = await salonServiceRepository.GetAllIncludingDeleted()
            .Where(s => appointment.SalonServiceIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name.Value, cancellationToken);

        var tenant = await tenantRepository
            .Where(t => t.Id == appointment.TenantId)
            .Select(t => new
            {
                Name = t.Name.Value,
                Slug = t.Slug.Value,
                TimeZoneId = t.TimeZoneId.Value,
                Address = t.Address != null ? t.Address.FullAddress + ", " + t.Address.District + " / " + t.Address.City : null,
                PhoneNumber = t.PhoneNumber != null ? t.PhoneNumber.Value : null
            })
            .FirstAsync(cancellationToken);

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(tenant.TimeZoneId);

        return new NotificationDetails(
            appointment.AppointmentNumber,
            appointment.IsOnline,
            TimeZoneInfo.ConvertTime(appointment.Start, timeZone),
            appointment.SalonServiceIds.Select(id => serviceNames[id]).ToList(),
            staffFullName,
            customer.FirstName,
            customer.Email,
            customer.PhoneNumber,
            tenant.Name,
            tenant.Slug,
            tenant.Address,
            tenant.PhoneNumber);
    }

    private sealed record NotificationDetails(
        string AppointmentNumber,
        bool IsOnline,
        DateTimeOffset LocalStart,
        List<string> ServiceNames,
        string StaffFullName,
        string CustomerFirstName,
        string? Email,
        string? PhoneNumber,
        string SalonName,
        string Slug,
        string? SalonAddress,
        string? SalonPhoneNumber);
}
