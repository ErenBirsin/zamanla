using System.Net;
using System.Net.Http.Json;
using Shouldly;
using TS.Result;
using Zamanla.Application.Appointments;
using Zamanla.Application.Customers;
using Zamanla.Application.SalonServices;
using Zamanla.Application.Staffs;
using Zamanla.Application.TimeOffs;
using Zamanla.Domain.Staffs.ValueObjects;
using Zamanla.WebAPI.IntegrationTests.Infrastructure;

namespace Zamanla.WebAPI.IntegrationTests.TimeOffs;

// hizmet 45 dk + 10 dk hazırlık; personel pazartesi 09–18 çalışır
[Collection(IntegrationTestCollection.Name)]
public sealed class TimeOffTests(ZamanlaApiFactory factory) : IntegrationTestBase(factory)
{
    #region Ekleme, listeleme ve silme
    [Fact]
    public async Task Create_ForStaff_IsListedWithStaffName()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);

        var response = await PostTimeOffAsync(client, salon.StaffId, NextMonday(12), NextMonday(14), " Doktor ");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var timeOff = (await GetTimeOffsAsync(client)).ShouldHaveSingleItem();
        timeOff.StaffId.ShouldBe(salon.StaffId);
        timeOff.StaffFullName.ShouldBe("Ayse Sik");
        timeOff.StartAt.ShouldBe(NextMonday(12));
        timeOff.EndAt.ShouldBe(NextMonday(14));
        timeOff.Note.ShouldBe("Doktor");
    }

    [Fact]
    public async Task Create_ForSalon_IsListedWithoutStaff()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");

        var response = await PostTimeOffAsync(client, null, NextMonday(0), NextMonday(0).AddDays(1), "Bayram");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<string>>(CancellationToken);
        result!.Data.ShouldBe("Kapalı gün kaydedildi");
        var timeOff = (await GetTimeOffsAsync(client)).ShouldHaveSingleItem();
        timeOff.StaffId.ShouldBeNull();
        timeOff.StaffFullName.ShouldBeNull();
    }

    [Fact]
    public async Task Create_OverlappingSameStaff_ReturnsConflict()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        (await PostTimeOffAsync(client, salon.StaffId, NextMonday(12), NextMonday(14))).EnsureSuccessStatusCode();

        var overlapping = await PostTimeOffAsync(client, salon.StaffId, NextMonday(13), NextMonday(15));
        var after = await PostTimeOffAsync(client, salon.StaffId, NextMonday(14), NextMonday(15));

        overlapping.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        after.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await GetTimeOffsAsync(client)).Count.ShouldBe(2);
    }

    [Fact]
    public async Task Create_OverAppointment_ReturnsConflictWithAppointmentList()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        (await PostAppointmentAsync(client, salon, NextMonday(10))).EnsureSuccessStatusCode();

        var staffTimeOff = await PostTimeOffAsync(client, salon.StaffId, NextMonday(9), NextMonday(12));
        var salonClosed = await PostTimeOffAsync(client, null, NextMonday(0), NextMonday(0).AddDays(1));

        staffTimeOff.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await GetErrorAsync(staffTimeOff)).ShouldStartWith("Bu izin 1 randevuyu etkiliyor");
        salonClosed.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await GetErrorAsync(salonClosed)).ShouldStartWith("Bu kapalı gün 1 randevuyu etkiliyor");
        (await GetTimeOffsAsync(client)).ShouldBeEmpty();
    }

    [Fact]
    public async Task Create_InPast_ReturnsBadRequest()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var now = DateTimeOffset.UtcNow;

        var response = await PostTimeOffAsync(client, null, now.AddDays(-2), now.AddDays(-1));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await GetTimeOffsAsync(client)).ShouldBeEmpty();
    }

    [Fact]
    public async Task Delete_RemovesTimeOff()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        (await PostTimeOffAsync(client, null, NextMonday(0), NextMonday(0).AddDays(1))).EnsureSuccessStatusCode();
        var id = (await GetTimeOffsAsync(client)).Single().Id;

        var delete = await client.DeleteAsync($"/api/time-offs/{id}", CancellationToken);
        var again = await client.DeleteAsync($"/api/time-offs/{id}", CancellationToken);

        delete.StatusCode.ShouldBe(HttpStatusCode.OK);
        again.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await GetTimeOffsAsync(client)).ShouldBeEmpty();
    }

    [Fact]
    public async Task OtherTenantTimeOff_IsNotAccessible()
    {
        using var ownerA = await CreateOwnerClientAsync("salon-a", "a@test.com");
        using var ownerB = await CreateOwnerClientAsync("salon-b", "b@test.com");
        var salonB = await SetupSalonAsync(ownerB);
        (await PostTimeOffAsync(ownerA, null, NextMonday(0), NextMonday(0).AddDays(1))).EnsureSuccessStatusCode();
        var id = (await GetTimeOffsAsync(ownerA)).Single().Id;

        var delete = await ownerB.DeleteAsync($"/api/time-offs/{id}", CancellationToken);

        delete.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await GetTimeOffsAsync(ownerB)).ShouldBeEmpty();
        (await GetTimeOffsAsync(ownerA)).ShouldHaveSingleItem();
        // A'nın kapalı günü B'nin boş saatlerini etkilemez
        (await GetSlotsAsync(ownerB, salonB)).ShouldContain(NextMonday(10));
    }
    #endregion

    #region Boş saatler ve randevular
    [Fact]
    public async Task Availability_SkipsStaffTimeOff()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        (await PostTimeOffAsync(client, salon.StaffId, NextMonday(12), NextMonday(14))).EnsureSuccessStatusCode();

        var slots = await GetSlotsAsync(client, salon);

        slots.ShouldContain(NextMonday(11));
        slots.ShouldNotContain(NextMonday(12));
        slots.ShouldNotContain(NextMonday(13, 45));
        slots.ShouldContain(NextMonday(14));
    }

    [Fact]
    public async Task Availability_WhenSalonClosed_ReturnsNoSlots()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        (await PostTimeOffAsync(client, null, NextMonday(0), NextMonday(0).AddDays(1))).EnsureSuccessStatusCode();

        var slots = await GetSlotsAsync(client, salon);

        slots.ShouldBeEmpty();
    }

    [Fact]
    public async Task CreateAppointment_DuringTimeOff_ReturnsBadRequest()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        (await PostTimeOffAsync(client, salon.StaffId, NextMonday(12), NextMonday(14))).EnsureSuccessStatusCode();

        var response = await PostAppointmentAsync(client, salon, NextMonday(12, 30));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await GetErrorAsync(response)).ShouldBe("Personel bu saatte izinli");
    }

    [Fact]
    public async Task CreateAppointment_WhenSalonClosed_ReturnsBadRequest()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        (await PostTimeOffAsync(client, null, NextMonday(0), NextMonday(0).AddDays(1))).EnsureSuccessStatusCode();

        var response = await PostAppointmentAsync(client, salon, NextMonday(10));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await GetErrorAsync(response)).ShouldBe("Salon bu tarihte kapalı");
    }

    [Fact]
    public async Task Reschedule_IntoTimeOff_ReturnsBadRequest()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        (await PostAppointmentAsync(client, salon, NextMonday(10))).EnsureSuccessStatusCode();
        (await PostTimeOffAsync(client, salon.StaffId, NextMonday(14), NextMonday(16))).EnsureSuccessStatusCode();
        var id = (await GetAppointmentsAsync(client)).Single().Id;

        var response = await client.PutAsJsonAsync(
            $"/api/appointments/{id}/reschedule",
            new AppointmentRescheduleCommand(id, salon.StaffId, NextMonday(15)),
            CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await GetAppointmentsAsync(client)).Single().StartAt.ShouldBe(NextMonday(10));
    }
    #endregion

    #region Yardımcılar
    private sealed record SalonData(Guid ServiceId, Guid StaffId, Guid CustomerId);

    private async Task<SalonData> SetupSalonAsync(HttpClient client)
    {
        var serviceCommand = new SalonServiceCreateCommand("Sac kesimi", "Sac", 45, 10, 400);
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

    private Task<HttpResponseMessage> PostTimeOffAsync(
        HttpClient client, Guid? staffId, DateTimeOffset startAt, DateTimeOffset endAt, string? note = null) =>
        client.PostAsJsonAsync("/api/time-offs", new TimeOffCreateCommand(staffId, startAt, endAt, note), CancellationToken);

    private Task<HttpResponseMessage> PostAppointmentAsync(HttpClient client, SalonData salon, DateTimeOffset startAt)
    {
        var command = new AppointmentCreateCommand(salon.CustomerId, salon.StaffId, [salon.ServiceId], startAt, null);
        return client.PostAsJsonAsync("/api/appointments", command, CancellationToken);
    }

    private async Task<List<DateTimeOffset>> GetSlotsAsync(HttpClient client, SalonData salon)
    {
        var date = NextMonday(10).ToString("yyyy-MM-dd");
        var result = await client.GetFromJsonAsync<Result<List<AvailabilitySlotDto>>>(
            $"/api/appointments/availability?date={date}&salonServiceIds={salon.ServiceId}&staffId={salon.StaffId}",
            CancellationToken);
        return result!.Data!.Select(s => s.StartAt).ToList();
    }

    private async Task<List<TimeOffDto>> GetTimeOffsAsync(HttpClient client)
    {
        var response = await client.GetFromJsonAsync<ODataResponse<TimeOffDto>>("/api/odata/timeOffs", CancellationToken);
        return response!.Value;
    }

    private async Task<List<AppointmentDto>> GetAppointmentsAsync(HttpClient client)
    {
        var response = await client.GetFromJsonAsync<ODataResponse<AppointmentDto>>("/api/odata/appointments", CancellationToken);
        return response!.Value;
    }

    private static async Task<string> GetErrorAsync(HttpResponseMessage response)
    {
        var result = await response.Content.ReadFromJsonAsync<Result<string>>(CancellationToken);
        return result!.ErrorMessages!.ShouldHaveSingleItem();
    }
    #endregion
}
