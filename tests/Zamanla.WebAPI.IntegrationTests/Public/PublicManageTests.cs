using GenericRepository;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using TS.Result;
using Zamanla.Application.Appointments;
using Zamanla.Application.Auth;
using Zamanla.Application.Public;
using Zamanla.Application.SalonServices;
using Zamanla.Application.Settings;
using Zamanla.Application.Staffs;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Staffs.ValueObjects;
using Zamanla.Domain.Tenants.ValueObjects;
using Zamanla.WebAPI.IntegrationTests.Infrastructure;

namespace Zamanla.WebAPI.IntegrationTests.Public;

// müşterinin randevu linki: görüntüleme, iptal ve saat değiştirme
[Collection(IntegrationTestCollection.Name)]
public sealed class PublicManageTests(ZamanlaApiFactory factory) : IntegrationTestBase(factory)
{
    #region Görüntüleme
    [Fact]
    public async Task Get_WithToken_ReturnsAppointment()
    {
        using var owner = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(owner);
        var token = await BookAsync("salon-a", salon, NextMonday(10));

        var appointment = await GetAsync("salon-a", token);

        appointment.CustomerFullName.ShouldBe("Ayse Yilmaz");
        appointment.StartAt.ShouldBe(NextMonday(10));
        appointment.Services.ShouldHaveSingleItem().Name.ShouldBe("Sac kesimi");
        appointment.CanCancel.ShouldBeTrue();
        appointment.CanReschedule.ShouldBeTrue();
    }

    [Fact]
    public async Task Get_WithWrongToken_ReturnsNotFound()
    {
        using var owner = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(owner);
        await BookAsync("salon-a", salon, NextMonday(10));

        var response = await Client.GetAsync("/api/public/salon-a/manage/yanlis-token", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_WithOtherSalonsSlug_ReturnsNotFound()
    {
        using var ownerA = await CreateOwnerClientAsync("salon-a", "a@test.com");
        using var ownerB = await CreateOwnerClientAsync("salon-b", "b@test.com");
        var salon = await SetupSalonAsync(ownerA);
        var token = await BookAsync("salon-a", salon, NextMonday(10));

        // A'nın randevu linki B'nin adresiyle açılamaz
        var response = await Client.GetAsync($"/api/public/salon-b/manage/{token}", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    #endregion

    #region İptal
    [Fact]
    public async Task Cancel_ByLink_FreesTheSlot()
    {
        using var owner = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(owner);
        var token = await BookAsync("salon-a", salon, NextMonday(10));

        var response = await Client.PutAsync($"/api/public/salon-a/manage/{token}/cancel", null, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var appointment = await GetAsync("salon-a", token);
        appointment.Status.ShouldBe("İptal Edildi");
        appointment.CanCancel.ShouldBeFalse();
        // boşalan saate yeniden randevu alınabilir
        await BookAsync("salon-a", salon, NextMonday(10), "Mehmet");
    }

    [Fact]
    public async Task Cancel_LessThanThreeHoursBefore_ReturnsBadRequest()
    {
        var tokens = await RegisterAsync("salon-a", "a@test.com");
        var user = await GetMeAsync(tokens.AccessToken);
        using var owner = Factory.CreateClient();
        owner.DefaultRequestHeaders.Authorization = new("Bearer", tokens.AccessToken);
        var salon = await SetupSalonAsync(owner);
        var token = await BookAsync("salon-a", salon, NextMonday(10));
        await MoveAppointmentAsync(user, token, DateTimeOffset.UtcNow.AddHours(1));

        var response = await Client.PutAsync($"/api/public/salon-a/manage/{token}/cancel", null, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var appointment = await GetAsync("salon-a", token);
        appointment.Status.ShouldBe("Onaylandı");
        appointment.CanCancel.ShouldBeFalse();
        appointment.CanReschedule.ShouldBeTrue();
    }

    [Fact]
    public async Task Cancel_WhenSalonAllowsLateCancel_Succeeds()
    {
        var tokens = await RegisterAsync("salon-a", "a@test.com");
        var user = await GetMeAsync(tokens.AccessToken);
        using var owner = Factory.CreateClient();
        owner.DefaultRequestHeaders.Authorization = new("Bearer", tokens.AccessToken);
        var salon = await SetupSalonAsync(owner);
        var token = await BookAsync("salon-a", salon, NextMonday(10));
        await MoveAppointmentAsync(user, token, DateTimeOffset.UtcNow.AddHours(1));
        // salon son ana kadar linkten iptale izin veriyor
        var settings = new SettingsUpdateCommand("Test Salon", null, null, BookingPolicy.Default with { CancelHoursBefore = 0 });
        (await owner.PutAsJsonAsync("/api/settings", settings, CancellationToken)).EnsureSuccessStatusCode();

        var response = await Client.PutAsync($"/api/public/salon-a/manage/{token}/cancel", null, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await GetAsync("salon-a", token)).Status.ShouldBe("İptal Edildi");
    }

    #endregion

    #region Saat değiştirme
    [Fact]
    public async Task Reschedule_ByLink_MovesAppointment()
    {
        using var owner = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(owner);
        var token = await BookAsync("salon-a", salon, NextMonday(10));

        var response = await RescheduleAsync("salon-a", token, salon, NextMonday(14));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await GetAsync("salon-a", token)).StartAt.ShouldBe(NextMonday(14));
    }

    [Fact]
    public async Task Reschedule_AfterStart_ReturnsBadRequest()
    {
        var tokens = await RegisterAsync("salon-a", "a@test.com");
        var user = await GetMeAsync(tokens.AccessToken);
        using var owner = Factory.CreateClient();
        owner.DefaultRequestHeaders.Authorization = new("Bearer", tokens.AccessToken);
        var salon = await SetupSalonAsync(owner);
        var token = await BookAsync("salon-a", salon, NextMonday(10));
        await MoveAppointmentAsync(user, token, DateTimeOffset.UtcNow.AddMinutes(-10));

        var response = await RescheduleAsync("salon-a", token, salon, NextMonday(14));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await GetAsync("salon-a", token)).CanReschedule.ShouldBeFalse();
    }

    [Fact]
    public async Task RescheduleAvailability_TreatsOwnTimeAsFree()
    {
        using var owner = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(owner);
        var token = await BookAsync("salon-a", salon, NextMonday(10));

        var date = NextMonday(10).ToString("yyyy-MM-dd");
        var result = await Client.GetFromJsonAsync<Result<List<AvailabilitySlotDto>>>(
            $"/api/public/salon-a/manage/{token}/availability?date={date}", CancellationToken);

        var slots = result!.Data!.Select(s => s.StartAt).ToList();
        slots.ShouldContain(NextMonday(10));
        slots.ShouldContain(NextMonday(10, 15));
    }
    #endregion

    private sealed record SalonData(Guid ServiceId, Guid StaffId);

    private async Task<SalonData> SetupSalonAsync(HttpClient client)
    {
        (await client.PostAsJsonAsync(
            "/api/services", new SalonServiceCreateCommand("Sac kesimi", "Sac", 45, 10, 400), CancellationToken))
            .EnsureSuccessStatusCode();
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

        return new SalonData(serviceId, staff!.Value.Single().Id);
    }

    // online randevu alır, yönetim linkindeki token'ı döner
    private async Task<string> BookAsync(string slug, SalonData salon, DateTimeOffset startAt, string firstName = "Ayse")
    {
        var command = new PublicAppointmentCreateCommand(
            firstName, "Yilmaz", "0532 111 22 33", salon.StaffId, [salon.ServiceId], startAt, null);
        var response = await Client.PostAsJsonAsync($"/api/public/{slug}/appointments", command, CancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Result<PublicAppointmentCreatedDto>>(CancellationToken);
        return result!.Data!.ManageToken;
    }

    private async Task<PublicAppointmentDto> GetAsync(string slug, string token)
    {
        var result = await Client.GetFromJsonAsync<Result<PublicAppointmentDto>>(
            $"/api/public/{slug}/manage/{token}", CancellationToken);
        return result!.Data!;
    }

    private Task<HttpResponseMessage> RescheduleAsync(string slug, string token, SalonData salon, DateTimeOffset startAt) =>
        Client.PutAsJsonAsync(
            $"/api/public/{slug}/manage/{token}/reschedule",
            new PublicAppointmentRescheduleCommand(token, salon.StaffId, startAt),
            CancellationToken);

    // saat kuralları için randevu doğrudan veritabanında şimdiye yaklaştırılır
    private async Task MoveAppointmentAsync(CurrentUserDto owner, string token, DateTimeOffset startAt)
    {
        using var scope = CreateScopeAs(owner);
        var repository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var appointment = await repository.GetByManageTokenAsync(token, CancellationToken);
        appointment!.SetTime(startAt);
        repository.Update(appointment);
        await unitOfWork.SaveChangesAsync(CancellationToken);
    }
}
