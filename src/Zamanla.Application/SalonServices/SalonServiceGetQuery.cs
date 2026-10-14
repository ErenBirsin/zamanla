using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Domain.SalonServices;

namespace Zamanla.Application.SalonServices;

public sealed record SalonServiceGetQuery(Guid Id) : IRequest<Result<SalonServiceDto>>;

internal sealed class SalonServiceGetQueryHandler(
    ISalonServiceRepository salonServiceRepository) : IRequestHandler<SalonServiceGetQuery, Result<SalonServiceDto>>
{
    public async Task<Result<SalonServiceDto>> Handle(SalonServiceGetQuery request, CancellationToken cancellationToken)
    {
        var res = await salonServiceRepository
            .GetAllWithAudit()
            .MapTo()
            .Where(s => s.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (res is null)
        {
            return Result<SalonServiceDto>.Failure(404, "Hizmet bulunamadı");
        }

        return res;
    }
}
