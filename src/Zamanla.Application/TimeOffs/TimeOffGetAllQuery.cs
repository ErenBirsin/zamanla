using TS.MediatR;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.TimeOffs;

namespace Zamanla.Application.TimeOffs;

public sealed record TimeOffGetAllQuery : IRequest<IQueryable<TimeOffDto>>;

public sealed class TimeOffDto
{
    public Guid Id { get; set; }
    // boşsa bütün salon kapalı
    public Guid? StaffId { get; set; }
    public string? StaffFullName { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public string? Note { get; set; }
}

internal sealed class TimeOffGetAllQueryHandler(
    ITimeOffRepository timeOffRepository,
    IStaffRepository staffRepository) : IRequestHandler<TimeOffGetAllQuery, IQueryable<TimeOffDto>>
{
    public Task<IQueryable<TimeOffDto>> Handle(TimeOffGetAllQuery request, CancellationToken cancellationToken)
    {
        // personel sonradan silinmiş olsa da eski izninde adı görünsün
        var timeOffs =
            from timeOff in timeOffRepository.GetAll()
            join staff in staffRepository.GetAllIncludingDeleted() on timeOff.StaffId equals staff.Id into staffs
            from staff in staffs.DefaultIfEmpty()
            select new TimeOffDto
            {
                Id = timeOff.Id,
                StaffId = timeOff.StaffId,
                StaffFullName = staff != null ? staff.FirstName.Value + " " + staff.LastName.Value : null,
                StartAt = timeOff.Time.Start,
                EndAt = timeOff.Time.End,
                Note = timeOff.Note != null ? timeOff.Note.Value : null
            };

        return Task.FromResult(timeOffs);
    }
}
