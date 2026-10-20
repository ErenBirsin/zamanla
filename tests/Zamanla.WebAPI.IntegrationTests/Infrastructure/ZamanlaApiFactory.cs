using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Respawn;
using Zamanla.Infrastructure.Context;

namespace Zamanla.WebAPI.IntegrationTests.Infrastructure;

public sealed class ZamanlaApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("ZAMANLA_TEST_CONNECTION")
        ?? @"Server=.\SQLEXPRESS;Database=Zamanla_Test;Trusted_Connection=True;TrustServerCertificate=True;";

    private Respawner _respawner = default!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:SqlServer", ConnectionString);
        builder.UseSetting("Jwt:SecretKey", "zamanla-integration-test-only-secret-key-never-use-in-production-0001");
        builder.UseSetting("RateLimiting:LoginPermitLimit", "1000");
        builder.UseSetting("RateLimiting:PublicPermitLimit", "1000");

        // testler hatırlatma servisini doğrudan çağırır; arka plandaki döngü test verisine karışmasın
        builder.ConfigureTestServices(services =>
        {
            var backgroundServices = services
            .Where(s => s.ServiceType == typeof(IHostedService)
                && (s.ImplementationType == typeof(AppointmentReminderBackgroundService)
                    || s.ImplementationType == typeof(RefreshTokenCleanupBackgroundService)))
            .ToList();
            foreach (var descriptor in backgroundServices)
            {
                services.Remove(descriptor);
            }
        });
    }

    public async ValueTask InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = [new("__EFMigrationsHistory")]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }
}
