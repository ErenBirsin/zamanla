using System.Net.Http.Json;
using GenericRepository;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TS.Result;
using Zamanla.Application.Auth;
using Zamanla.Application.Customers;
using Zamanla.Application.Dashboard;
using Zamanla.Application.SalonServices;
using Zamanla.Application.Staffs;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.Staffs.ValueObjects;
using Zamanla.WebAPI.IntegrationTests.Infrastructure;

namespace Zamanla.WebAPI.IntegrationTests.Dashboard;

// özet "şu an"a göre hesaplandığı için randevular saat kuralına takılmadan doğrudan veritabanına yazılır;
// hizmet 45 dk, 400 ₺
[Collection(IntegrationTestCollection.Name)]
public sealed class DashboardTests(ZamanlaApiFactory factory) : IntegrationTestBase(factory)
{
    private static readonly TimeZoneInfo Istanbul = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");

    [Fact]
    public async Task EmptySalon_ReturnsZerosAndSevenDays()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");

        var dashboard = await GetDashboardAsync(client);

        dashboard.Today.AppointmentCount.ShouldBe(0);
        dashboard.Today.ExpectedRevenue.ShouldBe(0m);
        dashboard.Upcoming.ShouldBeEmpty();
        dashboard.LastSevenDays.Count.ShouldBe(7);
        dashboard.LastSevenDays[^1].Date.ShouldBe(Today());
        dashboard.LastSevenDays.ShouldAllBe(d => d.AppointmentCount == 0 && d.Revenue == 0m);
    }

    [Fact]
    public async Task Today_CountsByStatusAndListsUpcoming()
    {
        SkipNearMidnight();
        var (client, owner) = await CreateOwnerAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        var now = DateTimeOffset.UtcNow;

        await SeedAsync(owner, salon, now.AddMinutes(-30), a => a.Complete(now));
        await SeedAsync(owner, salon, now.AddMinutes(-25), a => a.MarkAsNoShow(now));
        await SeedAsync(owner, salon, now.AddMinutes(10), a => a.Cancel("Müşteri aradı", now));
        await SeedAsync(owner, salon, now.AddMinutes(30));
        await SeedAsync(owner, salon, now.AddMinutes(20));

        var dashboard = await GetDashboardAsync(client);

        dashboard.Today.AppointmentCount.ShouldBe(4);
        dashboard.Today.CompletedCount.ShouldBe(1);
        dashboard.Today.NoShowCount.ShouldBe(1);
        dashboard.Today.CancelledCount.ShouldBe(1);
        // gelmeyen ve iptal hariç: tamamlanan + sıradaki iki randevu
        dashboard.Today.ExpectedRevenue.ShouldBe(1200m);

        // iptal edilen listede yok, sıra saate göre
        dashboard.Upcoming.Count.ShouldBe(2);
        dashboard.Upcoming[0].StartAt.ShouldBe(now.AddMinutes(20), TimeSpan.FromSeconds(1));
        dashboard.Upcoming[0].CustomerFullName.ShouldBe("Elif Yilmaz");
        dashboard.Upcoming[0].StaffFullName.ShouldBe("Ayse Sik");
        dashboard.Upcoming[0].ServiceNames.ShouldBe(new[] { "Sac kesimi" });

        var todayBar = dashboard.LastSevenDays[^1];
        todayBar.AppointmentCount.ShouldBe(4);
        // grafikte sadece tamamlananların cirosu
        todayBar.Revenue.ShouldBe(400m);
    }

    [Fact]
    public async Task LastSevenDays_GroupsBySalonDay()
    {
        var (client, owner) = await CreateOwnerAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        var now = DateTimeOffset.UtcNow;

        await SeedAsync(owner, salon, now.AddDays(-1), a => a.Complete(now));
        await SeedAsync(owner, salon, now.AddDays(-3), a => a.Complete(now));
        await SeedAsync(owner, salon, now.AddDays(-3));
        // 7 günün dışında
        await SeedAsync(owner, salon, now.AddDays(-8), a => a.Complete(now));

        var days = (await GetDashboardAsync(client)).LastSevenDays;

        days.Single(d => d.Date == Today().AddDays(-1)).Revenue.ShouldBe(400m);
        var threeDaysAgo = days.Single(d => d.Date == Today().AddDays(-3));
        threeDaysAgo.AppointmentCount.ShouldBe(2);
        threeDaysAgo.Revenue.ShouldBe(400m);
        days.Sum(d => d.AppointmentCount).ShouldBe(3);
    }

    [Fact]
    public async Task Dashboard_DoesNotCountOtherSalon()
    {
        SkipNearMidnight();
        using var clientA = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var (clientB, ownerB) = await CreateOwnerAsync("salon-b", "b@test.com");
        var salonB = await SetupSalonAsync(clientB);
        await SeedAsync(ownerB, salonB, DateTimeOffset.UtcNow.AddMinutes(20));

        var dashboardA = await GetDashboardAsync(clientA);
        var dashboardB = await GetDashboardAsync(clientB);

        dashboardA.Today.AppointmentCount.ShouldBe(0);
        dashboardA.Upcoming.ShouldBeEmpty();
        dashboardB.Today.AppointmentCount.ShouldBe(1);
    }

    #region Yardımcılar
    private sealed record SalonData(Guid ServiceId, Guid StaffId, Guid CustomerId);

    private static DateOnly Today() =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Istanbul).DateTime);

    // randevular şu andan ±30 dk; gece yarısına yakınsa bir kısmı yarına ya da düne düşer
    private static void SkipNearMidnight()
    {
        var local = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Istanbul).TimeOfDay;
        if (local < TimeSpan.FromMinutes(35) || local > TimeSpan.FromHours(24) - TimeSpan.FromMinutes(35))
        {
            Assert.Skip("Gece yarısına yakın; randevular güne sığmıyor");
        }
    }

    private async Task<(HttpClient Client, CurrentUserDto Owner)> CreateOwnerAsync(string slug, string email)
    {
        var tokens = await RegisterAsync(slug, email);
        var owner = await GetMeAsync(tokens.AccessToken);
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", tokens.AccessToken);
        return (client, owner);
    }

    // saat ve çalışma saati kurallarını atlayıp doğrudan kaydeder; durum değişikliği isteğe bağlı
    private async Task SeedAsync(CurrentUserDto owner, SalonData salon, DateTimeOffset startAt, Action<Appointment>? change = null)
    {
        using var scope = CreateScopeAs(owner);
        var repository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        Appointment appointment = new(
            salon.CustomerId,
            salon.StaffId,
            [new AppointmentSalonService(salon.ServiceId, 45, 0, 400)],
            startAt,
            Source.Panel,
            null,
            startAt);
        change?.Invoke(appointment);
        repository.Add(appointment);
        await unitOfWork.SaveChangesAsync(CancellationToken);
    }

    private async Task<DashboardDto> GetDashboardAsync(HttpClient client)
    {
        var result = await client.GetFromJsonAsync<Result<DashboardDto>>("/api/dashboard", CancellationToken);
        return result!.Data!;
    }

    private async Task<SalonData> SetupSalonAsync(HttpClient client)
    {
        var serviceCommand = new SalonServiceCreateCommand("Sac kesimi", "Sac", 45, 0, 400);
        (await client.PostAsJsonAsync("/api/services", serviceCommand, CancellationToken)).EnsureSuccessStatusCode();
        var services = await client.GetFromJsonAsync<ODataResponse<SalonServiceDto>>("/api/odata/services", CancellationToken);
        var serviceId = services!.Value.Single().Id;

        var staffCommand = new StaffCreateCommand(
            "Ayse",
            "Sik",
            "peach",
            [new WorkingHour(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(18, 0))],
            [serviceId]);
        (await client.PostAsJsonAsync("/api/staff", staffCommand, CancellationToken)).EnsureSuccessStatusCode();
        var staff = await client.GetFromJsonAsync<ODataResponse<StaffDto>>("/api/odata/staff", CancellationToken);
        var staffId = staff!.Value.Single().Id;

        var customerCommand = new CustomerCreateCommand("Elif", "Yilmaz", "0532 123 45 67", null, null);
        (await client.PostAsJsonAsync("/api/customers", customerCommand, CancellationToken)).EnsureSuccessStatusCode();
        var customers = await client.GetFromJsonAsync<ODataResponse<CustomerDto>>("/api/odata/customers", CancellationToken);
        var customerId = customers!.Value.Single().Id;

        return new SalonData(serviceId, staffId, customerId);
    }
    #endregion
}
