using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using TS.MediatR;
using Zamanla.Application.SalonServices;

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
        return builder.GetEdmModel();
    }

    [HttpGet("services")]
    public IQueryable<SalonServiceDto> Services(ISender sender, CancellationToken cancellationToken = default)
        => sender.Send(new SalonServiceGetAllQuery(), cancellationToken).Result;
}
