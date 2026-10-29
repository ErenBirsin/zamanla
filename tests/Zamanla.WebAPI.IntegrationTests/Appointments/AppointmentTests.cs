using System.Net;
using System.Net.Http.Json;
using GenericRepository;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TS.Result;
using Zamanla.Application.Appointments;
using Zamanla.Application.Auth;
using Zamanla.Application.Customers;
using Zamanla.Application.SalonServices;
using Zamanla.Application.Staffs;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.Staffs.ValueObjects;
using Zamanla.WebAPI.IntegrationTests.Infrastructure;

namespace Zamanla.WebAPI.IntegrationTests.Appointments;

// hizmetler 45 dk + 10 dk hazırlık, 400 ₺; personel pazartesi 09–18 çalışır
[Collection(IntegrationTestCollection.Name)]
public sealed class AppointmentTests(ZamanlaApiFactory factory) : IntegrationTestBase(factory)
{
    #region Oluşturma ve listeleme
    [Fact]
    public async Task Create_WithTwoServices_CopiesDurationAndPrice()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var haircutId = await CreateServiceAsync(client, "Sac kesimi");
        var beardId = await CreateServiceAsync(client, "Sakal");
        var staffId = await CreateStaffAsync(client, haircutId, beardId);
        var customerId = await CreateCustomerAsync(client);

        var command = new AppointmentCreateCommand(customerId, staffId, [haircutId, beardId], NextMonday(10), "Kısa olsun");
        var response = await client.PostAsJsonAsync("/api/appointments", command, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var appointment = (await GetAppointmentsAsync(client)).ShouldHaveSingleItem();
        appointment.StartAt.ShouldBe(NextMonday(10));
        appointment.EndAt.ShouldBe(NextMonday(10).AddMinutes(90));
        appointment.BlockedUntil.ShouldBe(NextMonday(10).AddMinutes(100));
        appointment.Total.ShouldBe(800m);
        appointment.Status.ShouldBe("Onaylandı");
        appointment.Note.ShouldBe("Kısa olsun");
        appointment.SalonServices.Select(s => s.Name).ShouldBe(new[] { "Sac kesimi", "Sakal" }, ignoreOrder: true);
        appointment.Histories.ShouldHaveSingleItem().Title.ShouldBe("Randevu oluşturuldu");
    }

    [Fact]
    public async Task Create_OutsideWorkingHours_ReturnsBadRequest()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);

        var response = await PostAppointmentAsync(client, salon, NextMonday(20));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await GetAppointmentsAsync(client)).ShouldBeEmpty();
    }

    [Fact]
    public async Task Create_InsideOtherAppointmentsBuffer_ReturnsConflict()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        // 10:00 randevusu hazırlık payıyla 10:55'e kadar dolu
        (await PostAppointmentAsync(client, salon, NextMonday(10))).EnsureSuccessStatusCode();

        var inBuffer = await PostAppointmentAsync(client, salon, NextMonday(10, 45));
        var afterBuffer = await PostAppointmentAsync(client, salon, NextMonday(11));

        inBuffer.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        afterBuffer.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_WithParallelRequestsForSameSlot_OnlyOneSucceeds()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);

        // iki müşteri aynı anda aynı saate basıyormuş gibi 20 istek
        var responses = await Task.WhenAll(Enumerable.Range(0, 20)
            .Select(_ => PostAppointmentAsync(client, salon, NextMonday(10))));

        responses.Count(r => r.StatusCode == HttpStatusCode.OK).ShouldBe(1);
        responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).ShouldBe(19);
        (await GetAppointmentsAsync(client)).ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Create_WithOtherTenantStaff_ReturnsNotFound()
    {
        using var ownerA = await CreateOwnerClientAsync("salon-a", "a@test.com");
        using var ownerB = await CreateOwnerClientAsync("salon-b", "b@test.com");
        var salonA = await SetupSalonAsync(ownerA);
        var salonB = await SetupSalonAsync(ownerB);

        var response = await PostAppointmentAsync(ownerB, salonB with { StaffId = salonA.StaffId }, NextMonday(10));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await GetAppointmentsAsync(ownerA)).ShouldBeEmpty();
    }

    [Fact]
    public async Task OtherTenantAppointment_IsNotAccessible()
    {
        using var ownerA = await CreateOwnerClientAsync("salon-a", "a@test.com");
        using var ownerB = await CreateOwnerClientAsync("salon-b", "b@test.com");
        var salonA = await SetupSalonAsync(ownerA);
        var id = await CreateAppointmentAsync(ownerA, salonA, NextMonday(10));

        var get = await ownerB.GetAsync($"/api/appointments/{id}", CancellationToken);
        var cancel = await ownerB.PutAsJsonAsync(
            $"/api/appointments/{id}/cancel", new AppointmentCancelCommand(id, null), CancellationToken);

        get.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        cancel.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await GetAppointmentsAsync(ownerB)).ShouldBeEmpty();
        (await GetAppointmentsAsync(ownerA)).ShouldHaveSingleItem().Status.ShouldBe("Onaylandı");
    }

    [Fact]
    public async Task List_KeepsDeletedServiceName()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        await CreateAppointmentAsync(client, salon, NextMonday(10));

        var delete = await client.DeleteAsync($"/api/services/{salon.ServiceId}", CancellationToken);

        delete.StatusCode.ShouldBe(HttpStatusCode.OK);
        var appointment = (await GetAppointmentsAsync(client)).ShouldHaveSingleItem();
        appointment.SalonServices.ShouldHaveSingleItem().Name.ShouldBe("Sac kesimi");
    }

    [Fact]
    public async Task Availability_SkipsBusySlots()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        await CreateAppointmentAsync(client, salon, NextMonday(10));

        var date = NextMonday(10).ToString("yyyy-MM-dd");
        var result = await client.GetFromJsonAsync<Result<List<AvailabilitySlotDto>>>(
            $"/api/appointments/availability?date={date}&salonServiceIds={salon.ServiceId}&staffId={salon.StaffId}",
            CancellationToken);

        // 45 + 10 dk: 09:00 sığar, 09:15 10:00 randevusuna taşar, 10:55'ten sonra yine boş
        var slots = result!.Data!.Select(s => s.StartAt).ToList();
        slots.ShouldContain(NextMonday(9));
        slots.ShouldNotContain(NextMonday(9, 15));
        slots.ShouldNotContain(NextMonday(10));
        slots.ShouldNotContain(NextMonday(10, 45));
        slots.ShouldContain(NextMonday(11));
    }
    #endregion

    #region Durum değişiklikleri ve taşıma
    [Fact]
    public async Task Cancel_FreesTheSlot()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        var id = await CreateAppointmentAsync(client, salon, NextMonday(10));

        var cancel = await client.PutAsJsonAsync(
            $"/api/appointments/{id}/cancel", new AppointmentCancelCommand(id, "Müşteri aradı"), CancellationToken);
        var again = await PostAppointmentAsync(client, salon, NextMonday(10));

        cancel.StatusCode.ShouldBe(HttpStatusCode.OK);
        again.StatusCode.ShouldBe(HttpStatusCode.OK);
        var cancelled = await GetAppointmentAsync(client, id);
        cancelled.Status.ShouldBe("İptal Edildi");
        cancelled.Histories.Count.ShouldBe(2);
        cancelled.Histories[^1].Description.ShouldBe("Müşteri aradı");
    }

    [Fact]
    public async Task Complete_BeforeStart_ReturnsBadRequest()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        var id = await CreateAppointmentAsync(client, salon, NextMonday(10));

        var response = await client.PutAsync($"/api/appointments/{id}/complete", null, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await GetAppointmentAsync(client, id)).Status.ShouldBe("Onaylandı");
    }

    [Fact]
    public async Task NoShow_AfterStart_IncreasesCustomerCountAndClosesAppointment()
    {
        var tokens = await RegisterAsync("salon-a", "a@test.com");
        var owner = await GetMeAsync(tokens.AccessToken);
        using var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", tokens.AccessToken);
        var salon = await SetupSalonAsync(client);
        var id = await SeedPastAppointmentAsync(owner, salon);

        var noShow = await client.PutAsync($"/api/appointments/{id}/no-show", null, CancellationToken);
        var complete = await client.PutAsync($"/api/appointments/{id}/complete", null, CancellationToken);

        noShow.StatusCode.ShouldBe(HttpStatusCode.OK);
        complete.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var appointment = await GetAppointmentAsync(client, id);
        appointment.Status.ShouldBe("Gelmedi");
        appointment.CustomerNoShowCount.ShouldBe(1);
    }

    [Fact]
    public async Task Reschedule_ToFreeSlot_KeepsPriceAndAddsHistory()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        var id = await CreateAppointmentAsync(client, salon, NextMonday(10));

        var response = await RescheduleAsync(client, id, salon.StaffId, NextMonday(14));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var appointment = await GetAppointmentAsync(client, id);
        appointment.StartAt.ShouldBe(NextMonday(14));
        appointment.EndAt.ShouldBe(NextMonday(14).AddMinutes(45));
        appointment.Total.ShouldBe(400m);
        appointment.Histories[^1].Title.ShouldBe("Randevu saati değiştirildi");
    }

    [Fact]
    public async Task Reschedule_NearOwnTime_DoesNotConflictWithItself()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        var id = await CreateAppointmentAsync(client, salon, NextMonday(10));

        var response = await RescheduleAsync(client, id, salon.StaffId, NextMonday(10, 15));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await GetAppointmentAsync(client, id)).StartAt.ShouldBe(NextMonday(10, 15));
    }

    [Fact]
    public async Task Availability_ForReschedule_TreatsOwnTimeAsFree()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        var id = await CreateAppointmentAsync(client, salon, NextMonday(10));

        var date = NextMonday(10).ToString("yyyy-MM-dd");
        var result = await client.GetFromJsonAsync<Result<List<AvailabilitySlotDto>>>(
            $"/api/appointments/availability?date={date}&salonServiceIds={salon.ServiceId}&staffId={salon.StaffId}&appointmentId={id}",
            CancellationToken);

        // taşırken randevunun kendi saati ve ona taşan saatler boş görünür
        var slots = result!.Data!.Select(s => s.StartAt).ToList();
        slots.ShouldContain(NextMonday(10));
        slots.ShouldContain(NextMonday(10, 15));
    }

    [Fact]
    public async Task Reschedule_ToBusySlot_ReturnsConflict()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        await CreateAppointmentAsync(client, salon, NextMonday(10));
        var id = await CreateAppointmentAsync(client, salon, NextMonday(14));

        var response = await RescheduleAsync(client, id, salon.StaffId, NextMonday(10, 30));

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await GetAppointmentAsync(client, id)).StartAt.ShouldBe(NextMonday(14));
    }

    [Fact]
    public async Task Reschedule_CancelledAppointment_ReturnsBadRequest()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        var id = await CreateAppointmentAsync(client, salon, NextMonday(10));
        (await client.PutAsJsonAsync(
            $"/api/appointments/{id}/cancel", new AppointmentCancelCommand(id, null), CancellationToken))
            .EnsureSuccessStatusCode();

        var response = await RescheduleAsync(client, id, salon.StaffId, NextMonday(14));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    #endregion

    #region Personel değişikliği
    [Fact]
    public async Task StaffUpdate_LeavingAppointmentOutsideHours_ReturnsConflict()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        await CreateAppointmentAsync(client, salon, NextMonday(14));

        // pazartesi öğleden sonra kapanıyor, 14:00 randevusu dışarıda kalıyor
        var shorter = await UpdateStaffAsync(client, salon, new TimeOnly(12, 0), "peach");
        var colorOnly = await UpdateStaffAsync(client, salon, new TimeOnly(18, 0), "sage");

        shorter.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var result = await shorter.Content.ReadFromJsonAsync<Result<string>>(CancellationToken);
        result!.ErrorMessages!.ShouldHaveSingleItem().ShouldStartWith("Bu değişiklik 1 randevuyu etkiliyor");
        colorOnly.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task StaffDelete_WithUpcomingAppointment_ReturnsConflict()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        var id = await CreateAppointmentAsync(client, salon, NextMonday(10));

        var blocked = await client.DeleteAsync($"/api/staff/{salon.StaffId}", CancellationToken);
        (await client.PutAsJsonAsync(
            $"/api/appointments/{id}/cancel", new AppointmentCancelCommand(id, null), CancellationToken))
            .EnsureSuccessStatusCode();
        var allowed = await client.DeleteAsync($"/api/staff/{salon.StaffId}", CancellationToken);

        blocked.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        allowed.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CustomerDelete_WithUpcomingAppointment_ReturnsConflict()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        var id = await CreateAppointmentAsync(client, salon, NextMonday(10));

        var blocked = await client.DeleteAsync($"/api/customers/{salon.CustomerId}", CancellationToken);
        (await client.PutAsJsonAsync(
            $"/api/appointments/{id}/cancel", new AppointmentCancelCommand(id, null), CancellationToken))
            .EnsureSuccessStatusCode();
        var allowed = await client.DeleteAsync($"/api/customers/{salon.CustomerId}", CancellationToken);

        blocked.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var result = await blocked.Content.ReadFromJsonAsync<Result<string>>(CancellationToken);
        result!.ErrorMessages!.ShouldHaveSingleItem().ShouldStartWith("Müşterinin 1 ileri tarihli randevusu var");
        allowed.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    #endregion

    #region Yardımcılar
    private sealed record SalonData(Guid ServiceId, Guid StaffId, Guid CustomerId);

    private async Task<SalonData> SetupSalonAsync(HttpClient client)
    {
        var serviceId = await CreateServiceAsync(client, "Sac kesimi");
        var staffId = await CreateStaffAsync(client, serviceId);
        var customerId = await CreateCustomerAsync(client);
        return new SalonData(serviceId, staffId, customerId);
    }

    private Task<HttpResponseMessage> PostAppointmentAsync(HttpClient client, SalonData salon, DateTimeOffset startAt)
    {
        var command = new AppointmentCreateCommand(salon.CustomerId, salon.StaffId, [salon.ServiceId], startAt, null);
        return client.PostAsJsonAsync("/api/appointments", command, CancellationToken);
    }

    private async Task<Guid> CreateAppointmentAsync(HttpClient client, SalonData salon, DateTimeOffset startAt)
    {
        (await PostAppointmentAsync(client, salon, startAt)).EnsureSuccessStatusCode();
        return (await GetAppointmentsAsync(client)).Single(a => a.StartAt == startAt).Id;
    }

    // geçmiş saate API ile randevu alınamadığı için doğrudan veritabanına yazılır
    private async Task<Guid> SeedPastAppointmentAsync(CurrentUserDto owner, SalonData salon)
    {
        using var scope = CreateScopeAs(owner);
        var repository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var now = DateTimeOffset.UtcNow;

        Appointment appointment = new(
            salon.CustomerId,
            salon.StaffId,
            [new AppointmentSalonService(salon.ServiceId, 45, 10, 400)],
            now.AddHours(-2),
            Source.Panel,
            null,
            now);
        repository.Add(appointment);
        await unitOfWork.SaveChangesAsync(CancellationToken);

        return appointment.Id;
    }

    private Task<HttpResponseMessage> RescheduleAsync(HttpClient client, Guid id, Guid staffId, DateTimeOffset startAt) =>
        client.PutAsJsonAsync(
            $"/api/appointments/{id}/reschedule", new AppointmentRescheduleCommand(id, staffId, startAt), CancellationToken);

    private Task<HttpResponseMessage> UpdateStaffAsync(HttpClient client, SalonData salon, TimeOnly mondayEnd, string color)
    {
        var command = new StaffUpdateCommand(
            salon.StaffId,
            "Ayse",
            "Sik",
            color,
            [new WorkingHour(DayOfWeek.Monday, new TimeOnly(9, 0), mondayEnd)],
            [salon.ServiceId],
            IsActive: true);
        return client.PutAsJsonAsync("/api/staff", command, CancellationToken);
    }

    private async Task<AppointmentDto> GetAppointmentAsync(HttpClient client, Guid id)
    {
        var result = await client.GetFromJsonAsync<Result<AppointmentDto>>($"/api/appointments/{id}", CancellationToken);
        return result!.Data!;
    }

    private async Task<List<AppointmentDto>> GetAppointmentsAsync(HttpClient client)
    {
        var response = await client.GetFromJsonAsync<ODataResponse<AppointmentDto>>("/api/odata/appointments", CancellationToken);
        return response!.Value;
    }

    private async Task<Guid> CreateServiceAsync(HttpClient client, string name)
    {
        var command = new SalonServiceCreateCommand(name, "Sac", 45, 10, 400);
        var response = await client.PostAsJsonAsync("/api/services", command, CancellationToken);
        response.EnsureSuccessStatusCode();

        var services = await client.GetFromJsonAsync<ODataResponse<SalonServiceDto>>("/api/odata/services", CancellationToken);
        return services!.Value.Single(s => s.Name == name).Id;
    }

    private async Task<Guid> CreateStaffAsync(HttpClient client, params Guid[] salonServiceIds)
    {
        var command = new StaffCreateCommand(
            "Ayse",
            "Sik",
            "peach",
            [new WorkingHour(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(18, 0))],
            [.. salonServiceIds]);
        var response = await client.PostAsJsonAsync("/api/staff", command, CancellationToken);
        response.EnsureSuccessStatusCode();

        var staff = await client.GetFromJsonAsync<ODataResponse<StaffDto>>("/api/odata/staff", CancellationToken);
        return staff!.Value.Single().Id;
    }

    private async Task<Guid> CreateCustomerAsync(HttpClient client)
    {
        var command = new CustomerCreateCommand("Elif", "Yilmaz", "0532 123 45 67", null, null);
        var response = await client.PostAsJsonAsync("/api/customers", command, CancellationToken);
        response.EnsureSuccessStatusCode();

        var customers = await client.GetFromJsonAsync<ODataResponse<CustomerDto>>("/api/odata/customers", CancellationToken);
        return customers!.Value.Single().Id;
    }
    #endregion
}
