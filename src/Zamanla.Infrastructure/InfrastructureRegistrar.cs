using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zamanla.Application.Services;
using Zamanla.Infrastructure.Context;
using Zamanla.Infrastructure.Interceptors;
using Zamanla.Infrastructure.Services;

namespace Zamanla.Infrastructure;
public static class InfrastructureRegistrar
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException(
                "'SqlServer' bağlantı cümlesi bulunamadı. User Secrets'a eklendiğinden emin ol.");

        services.AddHttpContextAccessor();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        options
            .UseSqlServer(connectionString)
            .AddInterceptors(
                serviceProvider.GetRequiredService<AuditInterceptor>(),
                serviceProvider.GetRequiredService<TenantInterceptor>()));

        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<TenantInterceptor>();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>("database");

        return services;
    }
}
