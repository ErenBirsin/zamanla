using TS.MediatR;
using Zamanla.Domain.SalonServices;

namespace Zamanla.Application.SalonServices;

public sealed record SalonServiceGetAllQuery : IRequest<IQueryable<SalonServiceDto>>;

internal sealed class SalonServiceGetAllQueryHandler(
    ISalonServiceRepository salonServiceRepository) : IRequestHandler<SalonServiceGetAllQuery, IQueryable<SalonServiceDto>>
{
    public Task<IQueryable<SalonServiceDto>> Handle(SalonServiceGetAllQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(salonServiceRepository.GetAllWithAudit().MapTo());
}
