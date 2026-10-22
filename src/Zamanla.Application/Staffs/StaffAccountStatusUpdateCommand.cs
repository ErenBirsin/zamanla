using GenericRepository;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Behaviors;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Users;

namespace Zamanla.Application.Staffs;

// pasif hesap giriş yapamaz ve açık oturumları kapanır; kayıt ve yetkileri silinmez
[Permission]
public sealed record StaffAccountStatusUpdateCommand(
    Guid UserId,
    bool IsActive) : IRequest<Result<string>>;

internal sealed class StaffAccountStatusUpdateCommandHandler(
    IAppUserRepository appUserRepository,
    IStaffRepository staffRepository,
    StaffAccountService staffAccountService,
    IUnitOfWork unitOfWork) : IRequestHandler<StaffAccountStatusUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(StaffAccountStatusUpdateCommand request, CancellationToken cancellationToken)
    {
        var user = await appUserRepository.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null || user.IsOwner)
        {
            return Result<string>.Failure(404, "Personel hesabı bulunamadı");
        }

        if (request.IsActive)
        {
            // silinmiş ya da pasif personelin hesabı açılamaz
            var staffIsActive = await staffRepository.AnyAsync(s => s.Id == user.StaffId && s.IsActive, cancellationToken);
            if (!staffIsActive)
            {
                return Result<string>.Failure(400, "Personel kaydı aktif değil, önce personeli aktif yapın");
            }

            user.Activate();
        }
        else
        {
            user.Deactivate();
            await staffAccountService.RevokeSessionsAsync(user.Id, cancellationToken);
        }

        appUserRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return request.IsActive ? "Hesap aktif yapıldı" : "Hesap pasif yapıldı";
    }
}
