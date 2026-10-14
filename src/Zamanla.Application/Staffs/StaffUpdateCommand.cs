using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;
using Zamanla.Domain.SalonServices;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Staffs.ValueObjects;

namespace Zamanla.Application.Staffs;

public sealed record StaffUpdateCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Color,
    List<WorkingHour> WorkingHours,
    List<Guid> SalonServiceIds,
    bool IsActive) : IRequest<Result<string>>;

public sealed class StaffUpdateCommandValidator : AbstractValidator<StaffUpdateCommand>
{
    public StaffUpdateCommandValidator()
    {
        RuleFor(p => p.FirstName)
            .NotEmpty().WithMessage("Geçerli bir ad girin")
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir");
        RuleFor(p => p.LastName)
            .NotEmpty().WithMessage("Geçerli bir soyad girin")
            .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir");
        RuleFor(p => p.Color)
            .Must(c => StaffRules.Colors.Contains(c)).WithMessage("Geçerli bir renk seçin");
        RuleFor(p => p.WorkingHours)
            .NotNull().WithMessage("Çalışma saatleri gönderilmeli")
            .Must(StaffRules.HaveNoOverlap).WithMessage("Aynı gündeki çalışma saatleri çakışamaz");
        RuleForEach(p => p.WorkingHours).ChildRules(hour =>
        {
            hour.RuleFor(h => h.Day)
                .IsInEnum().WithMessage("Geçerli bir gün seçin");
            hour.RuleFor(h => h.EndTime)
                .GreaterThan(h => h.StartTime).WithMessage("Bitiş saati başlangıç saatinden sonra olmalı");
        });
        RuleFor(p => p.SalonServiceIds)
            .NotNull().WithMessage("Hizmet listesi gönderilmeli");
    }
}

internal sealed class StaffUpdateCommandHandler(
    IStaffRepository staffRepository,
    ISalonServiceRepository salonServiceRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<StaffUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(StaffUpdateCommand request, CancellationToken cancellationToken)
    {
        var staff = await staffRepository.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (staff is null)
        {
            return Result<string>.Failure(404, "Personel bulunamadı");
        }

        var salonServiceIds = request.SalonServiceIds.Distinct().ToList();
        var foundCount = await salonServiceRepository.CountAsync(s => salonServiceIds.Contains(s.Id), cancellationToken);
        if (foundCount != salonServiceIds.Count)
        {
            return Result<string>.Failure(404, "Seçilen hizmetlerden biri bulunamadı");
        }

        staff.SetFirstName(new FirstName(request.FirstName.Trim()));
        staff.SetLastName(new LastName(request.LastName.Trim()));
        staff.SetColor(new Color(request.Color));
        staff.SetWorkingHours(request.WorkingHours);
        staff.SetSalonServices(salonServiceIds.Select(id => new StaffSalonService(id)));

        if (request.IsActive)
        {
            staff.Activate();
        }
        else
        {
            staff.Deactivate();
        }

        staffRepository.Update(staff);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Personel güncellendi";
    }
}
