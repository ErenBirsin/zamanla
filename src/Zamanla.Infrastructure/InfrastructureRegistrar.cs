using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zamanla.Application.Services;
using Zamanla.Infrastructure.Context;
using Zamanla.Infrastructure.Interceptors;
using Zamanla.Infrastructure.Options;
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
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<TenantInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            options
                .UseSqlServer(connectionString)
                .AddInterceptors(
                    serviceProvider.GetRequiredService<AuditInterceptor>(),
                    serviceProvider.GetRequiredService<TenantInterceptor>()));

        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<ApplicationDbContext>());

        services.Scan(scan => scan
            .FromAssemblies(typeof(InfrastructureRegistrar).Assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.ConfigureOptions<JwtSetupOptions>();
        services.AddAuthentication().AddJwtBearer();
        services.AddAuthorization();
        services.AddScoped<IJwtProvider, JwtProvider>();

        // geliştirmede smtp4dev (localhost:25); gerçek sunucunun kullanıcı adı ve şifresi User Secrets'ta
        var mailSettings = configuration.GetSection("MailSettings").Get<MailSettingOptions>()
            ?? throw new InvalidOperationException("'MailSettings' ayarı bulunamadı.");
        if (string.IsNullOrEmpty(mailSettings.UserId))
        {
            services.AddFluentEmail(mailSettings.Email)
                .AddSmtpSender(mailSettings.Smtp, mailSettings.Port);
        }
        else
        {
            services.AddFluentEmail(mailSettings.Email)
                .AddSmtpSender(mailSettings.Smtp, mailSettings.Port, mailSettings.UserId, mailSettings.Password);
        }
        services.AddScoped<IMailService, MailService>();
        services.AddScoped<ISmsService, FakeSmsService>();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>("database");

        return services;
    }
}
