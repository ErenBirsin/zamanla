using TS.MediatR;
using Zamanla.Domain.Staffs;

namespace Zamanla.Application.Staffs;

public sealed record StaffGetAllQuery : IRequest<IQueryable<StaffDto>>;

internal sealed class StaffGetAllQueryHandler(
    IStaffRepository staffRepository) : IRequestHandler<StaffGetAllQuery, IQueryable<StaffDto>>
{
    public Task<IQueryable<StaffDto>> Handle(StaffGetAllQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(staffRepository.GetAllWithAudit().MapTo());
}
