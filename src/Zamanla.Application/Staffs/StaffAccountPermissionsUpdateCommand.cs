using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Behaviors;
using Zamanla.Application.Permissions;
using Zamanla.Domain.Users;

namespace Zamanla.Application.Staffs;

// yeni yetkiler bir sonraki istekte geçerli olur; behavior her istekte veritabanından okur
[Permission]
public sealed record StaffAccountPermissionsUpdateCommand(
    Guid UserId,
    List<string> Permissions) : IRequest<Result<string>>;

public sealed class StaffAccountPermissionsUpdateCommandValidator : AbstractValidator<StaffAccountPermissionsUpdateCommand>
{
    public StaffAccountPermissionsUpdateCommandValidator()
    {
        RuleFor(p => p.Permissions)
            .NotNull().WithMessage("Yetki listesi gönderilmeli");
        RuleForEach(p => p.Permissions)
            .Must(PermissionCatalog.Exists).WithMessage("Geçersiz bir yetki seçildi");
    }
}

internal sealed class StaffAccountPermissionsUpdateCommandHandler(
    IAppUserRepository appUserRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<StaffAccountPermissionsUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(StaffAccountPermissionsUpdateCommand request, CancellationToken cancellationToken)
    {
        // sahibin kendi yetkileri değiştirilemez, her zaman hepsine sahip
        var user = await appUserRepository.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null || user.IsOwner)
        {
            return Result<string>.Failure(404, "Personel hesabı bulunamadı");
        }

        user.SetPermissions(request.Permissions.Distinct().Select(p => new Permission(p)));
        appUserRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Yetkiler güncellendi";
    }
}
