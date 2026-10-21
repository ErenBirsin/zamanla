using GenericRepository;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Behaviors;
using Zamanla.Application.Permissions;
using Zamanla.Domain.TimeOffs;

namespace Zamanla.Application.TimeOffs;

[Permission(PermissionCatalog.TimeOffsManage)]
public sealed record TimeOffDeleteCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class TimeOffDeleteCommandHandler(
    ITimeOffRepository timeOffRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<TimeOffDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(TimeOffDeleteCommand request, CancellationToken cancellationToken)
    {
        var timeOff = await timeOffRepository.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (timeOff is null)
        {
            return Result<string>.Failure(404, "İzin bulunamadı");
        }

        // yanlış girilen izin geri dönüşsüz silinir; başka kayıt ona bağlı değil
        timeOffRepository.Delete(timeOff);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "İzin silindi";
    }
}
