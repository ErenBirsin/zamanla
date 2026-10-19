using System.Buffers.Text;
using System.Security.Cryptography;
using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Services;
using Zamanla.Application.Tenants;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.Customers;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Tenants;

namespace Zamanla.Application.Public;

// müşteri giriş yapmadan randevu alır; ad, soyad ve telefonla müşteri kaydı bulunur ya da açılır
public sealed record PublicAppointmentCreateCommand(
    string FirstName,
    string LastName,
    string PhoneNumber,
    Guid StaffId,
    List<Guid> SalonServiceIds,
    DateTimeOffset StartAt,
    string? Note) : IRequest<Result<PublicAppointmentCreatedDto>>;

public sealed class PublicAppointmentCreatedDto
{
    public string AppointmentNumber { get; set; } = default!;
    // yönetim linki için; yalnızca bu cevapta döner, veritabanında özeti var
    public string ManageToken { get; set; } = default!;
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
}

public sealed class PublicAppointmentCreateCommandValidator : AbstractValidator<PublicAppointmentCreateCommand>
{
    public PublicAppointmentCreateCommandValidator()
    {
        RuleFor(p => p.FirstName)
            .NotEmpty().WithMessage("Geçerli bir ad girin")
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir");
        RuleFor(p => p.LastName)
            .NotEmpty().WithMessage("Geçerli bir soyad girin")
            .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir");
        RuleFor(p => p.PhoneNumber)
            .NotEmpty().WithMessage("Geçerli bir telefon numarası girin")
            .Must(PhoneNumber.IsValid).WithMessage("Geçerli bir telefon numarası girin");
        RuleFor(p => p.StaffId)
            .NotEmpty().WithMessage("Personel seçin");
        RuleFor(p => p.SalonServiceIds)
            .NotEmpty().WithMessage("En az bir hizmet seçin");
        RuleFor(p => p.StartAt)
            .NotEmpty().WithMessage("Randevu saati seçin");
        RuleFor(p => p.Note)
            .MaximumLength(500).WithMessage("Not en fazla 500 karakter olabilir");
    }
}

internal sealed class PublicAppointmentCreateCommandHandler(
    ICustomerRepository customerRepository,
    IAppointmentRepository appointmentRepository,
    ITenantRepository tenantRepository,
    ITenantContext tenantContext,
    AppointmentBookingService bookingService,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork) : IRequestHandler<PublicAppointmentCreateCommand, Result<PublicAppointmentCreatedDto>>
{
    public async Task<Result<PublicAppointmentCreatedDto>> Handle(PublicAppointmentCreateCommand request, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        #region Tarih Sınırı
        var timeZone = await tenantRepository.GetTimeZoneAsync(tenantContext.GetTenantId(), cancellationToken);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, timeZone).DateTime);
        var day = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(request.StartAt, timeZone).DateTime);
        var policy = await tenantRepository.GetBookingPolicyAsync(tenantContext.GetTenantId(), cancellationToken);
        if (day > today.AddDays(policy.MaxDaysAhead))
        {
            return Result<PublicAppointmentCreatedDto>.Failure(400, $"Online randevu en fazla {policy.MaxDaysAhead} gün sonrası için alınabilir");
        }
        #endregion

        #region Telefon Sınırı
        PhoneNumber phoneNumber = new(request.PhoneNumber);
        var phoneCustomerIds = await customerRepository
            .Where(c => c.PhoneNumber!.Value == phoneNumber.Value)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var online = Source.Online.Value;
        var confirmed = Status.Confirmed.Value;
        var upcomingCount = await appointmentRepository.CountAsync(a =>
            phoneCustomerIds.Contains(a.CustomerId)
            && a.Source.Value == online
            && a.Status.Value == confirmed
            && a.Time.Start > now, cancellationToken);
        if (upcomingCount >= policy.MaxUpcomingPerPhone)
        {
            return Result<PublicAppointmentCreatedDto>.Failure(400, $"Bu telefonla en fazla {policy.MaxUpcomingPerPhone} ileri tarihli online randevu alınabilir, salonu arayabilirsiniz");
        }
        #endregion

        #region Müşteri
        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        // aynı telefon farklı kişilerde olabilir; ad ve soyad da aynıysa aynı müşteridir
        var customer = await customerRepository.FirstOrDefaultAsync(c =>
            !c.IsAnonymized &&
            c.PhoneNumber!.Value == phoneNumber.Value &&
            c.FirstName.Value == firstName &&
            c.LastName.Value == lastName,
            cancellationToken);
        if (customer is null)
        {
            // randevuyla birlikte kaydedilir; randevu alınamazsa müşteri de açılmaz
            customer = new Customer(new FirstName(firstName), new LastName(lastName), phoneNumber, null, null);
            customerRepository.Add(customer);
        }
        #endregion

        Note? note = string.IsNullOrWhiteSpace(request.Note) ? null : new Note(request.Note.Trim());
        // online'da geçmişe tolerans yok
        var booking = await bookingService.CreateAsync(
            customer.Id,
            request.StaffId,
            request.SalonServiceIds,
            request.StartAt,
            Source.Online,
            note,
            now,
            TimeSpan.Zero,
            cancellationToken);
        if (!booking.IsSuccessful)
        {
            return Result<PublicAppointmentCreatedDto>.Failure(booking.StatusCode, booking.ErrorMessages!);
        }

        var appointment = booking.Data!;
        var manageToken = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        appointment.SetManageToken(ManageToken.FromPlainText(manageToken));

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<PublicAppointmentCreatedDto>.Failure(409, "Aynı anda başka bir randevu kaydedildi, lütfen tekrar deneyin");
        }

        return new PublicAppointmentCreatedDto
        {
            AppointmentNumber = appointment.AppointmentNumber.Value,
            ManageToken = manageToken,
            StartAt = appointment.Time.Start,
            EndAt = appointment.Time.End
        };
    }
}
