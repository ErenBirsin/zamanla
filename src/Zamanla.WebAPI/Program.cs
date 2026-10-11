using Scalar.AspNetCore;
using Zamanla.Application;
using Zamanla.Infrastructure;
using Zamanla.WebAPI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

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

app.MapHealthChecks("/api/health");

app.Run();
