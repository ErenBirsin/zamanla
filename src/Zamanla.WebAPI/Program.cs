using Microsoft.AspNetCore.OData;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;
using Zamanla.Application;
using Zamanla.Infrastructure;
using Zamanla.Infrastructure.Hubs;
using Zamanla.WebAPI;
using Zamanla.WebAPI.Controllers;
using Zamanla.WebAPI.Modules;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHostedService<AppointmentReminderBackgroundService>();
builder.Services.AddHostedService<RefreshTokenCleanupBackgroundService>();
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
var loginPermitLimit = builder.Configuration.GetValue("RateLimiting:LoginPermitLimit", 5);
var publicPermitLimit = builder.Configuration.GetValue("RateLimiting:PublicPermitLimit", 60);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login-fixed", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = loginPermitLimit,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.AddPolicy("public-fixed", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = publicPermitLimit,
                Window = TimeSpan.FromMinutes(1)
            }));
});

builder.Services
    .AddControllers()
    .AddOData(options => options
        .Select()
        .Filter()
        .Count()
        .Expand()
        .OrderBy()
        .SetMaxTop(null)
        .AddRouteComponents("api/odata", MainODataController.GetEdmModel()));

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("Zamanla API"));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapHealthChecks("/api/health");
app.MapControllers();
app.MapAuth();
app.MapSalonService();
app.MapStaff();
app.MapCustomer();
app.MapAppointment();
app.MapSettings();
app.MapTimeOff();
app.MapNotification();
app.MapDashboard();
app.MapPermission();
app.MapInvitation();
app.MapHub<NotificationHub>("/api/hubs/notifications");
app.MapPublic();
if (app.Environment.IsDevelopment())
{
    await app.SeedDemoDataAsync();
}
app.Run();
