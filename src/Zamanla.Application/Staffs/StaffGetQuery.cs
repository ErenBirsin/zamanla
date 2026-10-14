using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Domain.Staffs;

namespace Zamanla.Application.Staffs;

public sealed record StaffGetQuery(Guid Id) : IRequest<Result<StaffDto>>;

internal sealed class StaffGetQueryHandler(
    IStaffRepository staffRepository) : IRequestHandler<StaffGetQuery, Result<StaffDto>>
{
    public async Task<Result<StaffDto>> Handle(StaffGetQuery request, CancellationToken cancellationToken)
    {
        var res = await staffRepository
            .GetAllWithAudit()
            .MapTo()
            .Where(s => s.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (res is null)
        {
            return Result<StaffDto>.Failure(404, "Personel bulunamadı");
        }

        return res;
    }
}
