using System.Net;
using System.Net.Http.Json;
using Shouldly;
using TS.Result;
using Zamanla.Application.Appointments;
using Zamanla.Application.Customers;
using Zamanla.Application.Public;
using Zamanla.Application.SalonServices;
using Zamanla.Application.Staffs;
using Zamanla.Domain.Staffs.ValueObjects;
using Zamanla.WebAPI.IntegrationTests.Infrastructure;

namespace Zamanla.WebAPI.IntegrationTests.Public;

[Collection(IntegrationTestCollection.Name)]
public sealed class PublicBookingTests(ZamanlaApiFactory factory) : IntegrationTestBase(factory)
{
    private const string Phone = "0532 111 22 33";

    [Fact]
    public async Task Book_WithoutLogin_CreatesOnlineAppointment()
    {
        using var owner = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(owner);

        var response = await BookAsync("salon-a", salon, "Ayse", "Yilmaz", NextMonday(10));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var created = (await response.Content.ReadFromJsonAsync<Result<PublicAppointmentCreatedDto>>(CancellationToken))!.Data!;
        created.ManageToken.Length.ShouldBe(43);
        created.StartAt.ShouldBe(NextMonday(10));

        var appointment = (await GetAppointmentsAsync(owner)).ShouldHaveSingleItem();
        appointment.Source.ShouldBe("Online");
        appointment.CustomerFullName.ShouldBe("Ayse Yilmaz");
    }

    [Fact]
    public async Task Book_SamePhone_ReusesOnlyTheSamePerson()
    {
        using var owner = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(owner);

        (await BookAsync("salon-a", salon, "Ayse", "Yilmaz", NextMonday(9))).EnsureSuccessStatusCode();
        (await BookAsync("salon-a", salon, "Ayse", "Yilmaz", NextMonday(10))).EnsureSuccessStatusCode();
        // aynı telefonla kardeşi için
        (await BookAsync("salon-a", salon, "Mehmet", "Yilmaz", NextMonday(11))).EnsureSuccessStatusCode();

        var customers = await owner.GetFromJsonAsync<ODataResponse<CustomerDto>>("/api/odata/customers", CancellationToken);
        customers!.Value.Select(c => c.FullName).ShouldBe(new[] { "Ayse Yilmaz", "Mehmet Yilmaz" }, ignoreOrder: true);
    }

    [Fact]
    public async Task Book_OverPhoneLimit_ReturnsBadRequest()
    {
        using var owner = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(owner);
        for (var hour = 9; hour < 9 + OnlineBooking.MaxUpcomingPerPhone; hour++)
        {
            (await BookAsync("salon-a", salon, "Ayse", "Yilmaz", NextMonday(hour))).EnsureSuccessStatusCode();
        }

        var response = await BookAsync("salon-a", salon, "Ayse", "Yilmaz", NextMonday(15));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await GetAppointmentsAsync(owner)).Count.ShouldBe(OnlineBooking.MaxUpcomingPerPhone);
    }

    [Fact]
    public async Task Book_WithOtherSalonsStaff_ReturnsNotFound()
    {
        using var ownerA = await CreateOwnerClientAsync("salon-a", "a@test.com");
        using var ownerB = await CreateOwnerClientAsync("salon-b", "b@test.com");
        await SetupSalonAsync(ownerA);
        var salonB = await SetupSalonAsync(ownerB);

        // A'nın sayfasından B'nin personeline randevu alınamaz
        var response = await BookAsync("salon-a", salonB, "Ayse", "Yilmaz", NextMonday(10));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await GetAppointmentsAsync(ownerB)).ShouldBeEmpty();
    }

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

    // Client oturum açmamış ziyaretçi
    private Task<HttpResponseMessage> BookAsync(string slug, SalonData salon, string firstName, string lastName, DateTimeOffset startAt)
    {
        var command = new PublicAppointmentCreateCommand(
            firstName, lastName, Phone, salon.StaffId, [salon.ServiceId], startAt, null);
        return Client.PostAsJsonAsync($"/api/public/{slug}/appointments", command, CancellationToken);
    }

    private async Task<List<AppointmentDto>> GetAppointmentsAsync(HttpClient client)
    {
        var response = await client.GetFromJsonAsync<ODataResponse<AppointmentDto>>("/api/odata/appointments", CancellationToken);
        return response!.Value;
    }
}
