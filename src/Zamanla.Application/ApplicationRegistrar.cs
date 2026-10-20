using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using Zamanla.Application.Behaviors;
using Zamanla.Application.Services;

namespace Zamanla.Application;

public static class ApplicationRegistrar
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationRegistrar).Assembly;

        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(assembly);
            options.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        services.AddScoped<AppointmentBookingService>();
        services.AddScoped<AppointmentReminderService>();

        return services;
    }
}
