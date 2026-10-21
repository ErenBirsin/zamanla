using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using TS.MediatR;
using Zamanla.Application.Appointments;
using Zamanla.Application.Customers;
using Zamanla.Application.Notifications;
using Zamanla.Application.SalonServices;
using Zamanla.Application.Staffs;
using Zamanla.Application.TimeOffs;

namespace Zamanla.WebAPI.Controllers;

[Route("api/odata")]
[ApiController]
[EnableQuery]
[Authorize]
public class MainODataController : ODataController
{
    public static IEdmModel GetEdmModel()
    {
        ODataConventionModelBuilder builder = new();
        builder.EnableLowerCamelCase();
        builder.EntitySet<SalonServiceDto>("services");
        builder.EntitySet<StaffDto>("staff");
        builder.EntitySet<CustomerDto>("customers");
        builder.EntitySet<AppointmentDto>("appointments");
        builder.EntitySet<TimeOffDto>("timeOffs");
        builder.EntitySet<NotificationDto>("notifications");
        return builder.GetEdmModel();
    }

    [HttpGet("services")]
    public IQueryable<SalonServiceDto> Services(ISender sender, CancellationToken cancellationToken = default)
        => sender.Send(new SalonServiceGetAllQuery(), cancellationToken).Result;

    [HttpGet("staff")]
    public IQueryable<StaffDto> Staff(ISender sender, CancellationToken cancellationToken = default)
    => sender.Send(new StaffGetAllQuery(), cancellationToken).Result;

    [HttpGet("customers")]
    public IQueryable<CustomerDto> Customers(ISender sender, CancellationToken cancellationToken = default)
    => sender.Send(new CustomerGetAllQuery(), cancellationToken).Result;

    [HttpGet("appointments")]
    public IQueryable<AppointmentDto> Appointments(ISender sender, CancellationToken cancellationToken = default)
    => sender.Send(new AppointmentGetAllQuery(), cancellationToken).Result;

    [HttpGet("timeOffs")]
    public IQueryable<TimeOffDto> TimeOffs(ISender sender, CancellationToken cancellationToken = default)
    => sender.Send(new TimeOffGetAllQuery(), cancellationToken).Result;

    [HttpGet("notifications")]
    public IQueryable<NotificationDto> Notifications(ISender sender, CancellationToken cancellationToken = default)
    => sender.Send(new NotificationGetAllQuery(), cancellationToken).Result;
}
