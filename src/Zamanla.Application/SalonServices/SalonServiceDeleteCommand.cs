using GenericRepository;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Behaviors;
using Zamanla.Application.Permissions;
using Zamanla.Domain.SalonServices;

namespace Zamanla.Application.SalonServices;

[Permission(PermissionCatalog.ServicesManage)]
public sealed record SalonServiceDeleteCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class SalonServiceDeleteCommandHandler(
    ISalonServiceRepository salonServiceRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SalonServiceDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(SalonServiceDeleteCommand request, CancellationToken cancellationToken)
    {
        var salonService = await salonServiceRepository.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (salonService is null)
        {
            return Result<string>.Failure(404, "Hizmet bulunamadı");
        }

        salonService.Delete();
        salonServiceRepository.Update(salonService);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Hizmet silindi";
    }
}
