using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using Zamanla.Application.Behaviors;
using Zamanla.Application.Services;
using Zamanla.Application.Staffs;

namespace Zamanla.Application;

public static class ApplicationRegistrar
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationRegistrar).Assembly;

        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(assembly);
            // yetkisi olmayanın isteği doğrulanmadan reddedilir
            options.AddOpenBehavior(typeof(PermissionBehavior<,>));
            options.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        services.AddScoped<AppointmentBookingService>();
        services.AddScoped<AppointmentReminderService>();
        services.AddScoped<StaffAccountService>();

        return services;
    }
}
