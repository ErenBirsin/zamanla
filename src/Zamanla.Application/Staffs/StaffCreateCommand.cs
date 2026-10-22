using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Behaviors;
using Zamanla.Application.Permissions;
using Zamanla.Domain.SalonServices;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Staffs.ValueObjects;

namespace Zamanla.Application.Staffs;

[Permission(PermissionCatalog.StaffManage)]
public sealed record StaffCreateCommand(
    string FirstName,
    string LastName,
    string Color,
    List<WorkingHour> WorkingHours,
    List<Guid> SalonServiceIds) : IRequest<Result<string>>;

public sealed class StaffCreateCommandValidator : AbstractValidator<StaffCreateCommand>
{
    public StaffCreateCommandValidator()
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

internal sealed class StaffCreateCommandHandler(
    IStaffRepository staffRepository,
    ISalonServiceRepository salonServiceRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<StaffCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(StaffCreateCommand request, CancellationToken cancellationToken)
    {
        var salonServiceIds = request.SalonServiceIds.Distinct().ToList();
        var foundCount = await salonServiceRepository.CountAsync(s => salonServiceIds.Contains(s.Id), cancellationToken);
        if (foundCount != salonServiceIds.Count)
        {
            return Result<string>.Failure(404, "Seçilen hizmetlerden biri bulunamadı");
        }

        Staff staff = new(
            new FirstName(request.FirstName.Trim()),
            new LastName(request.LastName.Trim()),
            new Color(request.Color),
            request.WorkingHours,
            salonServiceIds.Select(id => new StaffSalonService(id)));

        staffRepository.Add(staff);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Personel kaydedildi";
    }
}
