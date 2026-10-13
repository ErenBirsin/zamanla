using GenericRepository;
using TS.MediatR;
using TS.Result;
using Zamanla.Domain.Staffs;

namespace Zamanla.Application.Staffs;

public sealed record StaffDeleteCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class StaffDeleteCommandHandler(
    IStaffRepository staffRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<StaffDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(StaffDeleteCommand request, CancellationToken cancellationToken)
    {
        var staff = await staffRepository.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (staff is null)
        {
            return Result<string>.Failure(404, "Personel bulunamadı");
        }

        staff.Delete();
        staffRepository.Update(staff);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Personel silindi";
    }
}
