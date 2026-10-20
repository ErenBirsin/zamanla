using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TS.Result;
using Zamanla.Application.Appointments;
using Zamanla.Application.Auth;
using Zamanla.Application.Customers;
using Zamanla.Application.Public;
using Zamanla.Application.SalonServices;
using Zamanla.Application.Services;
using Zamanla.Application.Staffs;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Staffs.ValueObjects;
using Zamanla.Infrastructure.Context;
using Zamanla.WebAPI.IntegrationTests.Infrastructure;

namespace Zamanla.WebAPI.IntegrationTests.Notifications;

// hizmet 45 dk + 10 dk hazırlık; personel pazartesi 09–18 çalışır
[Collection(IntegrationTestCollection.Name)]
public sealed class NotificationTests(ZamanlaApiFactory factory) : IntegrationTestBase(factory)
{
    // hatırlatma testlerinde randevu en az bir hafta sonra; kayıt anı 24 saat kuralına takılmaz
    private static readonly DateTimeOffset ReminderStart = NextMonday(10).AddDays(7);

    private FakeMailService Mail => Factory.MailService;

    #region Onay
    [Fact]
    public async Task PanelBooking_WithEmail_SendsConfirmationWithoutLink()
    {
        var (client, _) = await CreateOwnerAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client, "musteri@test.com");

        (await PostAppointmentAsync(client, salon, NextMonday(10))).EnsureSuccessStatusCode();

        var mail = Mail.Mails.ShouldHaveSingleItem();
        mail.To.ShouldBe("musteri@test.com");
        mail.Subject.ShouldBe("Randevunuz alındı · Test Salon");
        mail.Body.ShouldContain("Hizmet: Sac kesimi");
        mail.Body.ShouldContain("Personel: Ayse Sik");
        mail.Body.ShouldNotContain("/r/");
    }

    [Fact]
    public async Task PublicBooking_WithEmail_SendsManageLinkAndSavesEmail()
    {
        var (client, _) = await CreateOwnerAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client, null);

        var command = new PublicAppointmentCreateCommand(
            "Ayse", "Yilmaz", "0532 111 22 33", " Ayse@Test.com ", salon.StaffId, [salon.ServiceId], NextMonday(10), null);
        var response = await Client.PostAsJsonAsync("/api/public/salon-a/appointments", command, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var created = (await response.Content.ReadFromJsonAsync<Result<PublicAppointmentCreatedDto>>(CancellationToken))!.Data!;
        var mail = Mail.Mails.ShouldHaveSingleItem();
        mail.To.ShouldBe("ayse@test.com");
        mail.Body.ShouldContain($"/b/salon-a/r/{created.ManageToken}");

        var customers = await client.GetFromJsonAsync<ODataResponse<CustomerDto>>("/api/odata/customers", CancellationToken);
        customers!.Value.Single(c => c.FirstName == "Ayse").Email.ShouldBe("ayse@test.com");
    }

    [Fact]
    public async Task Booking_WithoutEmail_SendsNoMail()
    {
        var (client, _) = await CreateOwnerAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client, null);

        (await PostAppointmentAsync(client, salon, NextMonday(10))).EnsureSuccessStatusCode();

        Mail.Mails.ShouldBeEmpty();
    }

    [Fact]
    public async Task Booking_WhenMailFails_StillCreatesAppointment()
    {
        var (client, _) = await CreateOwnerAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client, "musteri@test.com");
        Mail.Fail = true;

        var response = await PostAppointmentAsync(client, salon, NextMonday(10));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await GetAppointmentsAsync(client)).ShouldHaveSingleItem();
        Mail.Mails.ShouldBeEmpty();
    }
    #endregion

    #region Hatırlatma
    [Fact]
    public async Task Reminder_ForDueAppointment_IsSentOnce()
    {
        var (client, owner) = await CreateOwnerAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client, "musteri@test.com");
        (await PostAppointmentAsync(client, salon, ReminderStart)).EnsureSuccessStatusCode();
        Mail.Reset();

        // randevuya 12 saat varken iki tur arka arkaya
        var first = await SendRemindersAsync(owner, ReminderStart.AddHours(-12));
        var second = await SendRemindersAsync(owner, ReminderStart.AddHours(-11));

        first.ShouldBe(1);
        second.ShouldBe(0);
        Mail.Mails.ShouldHaveSingleItem().Subject.ShouldBe("Randevu hatırlatması · Test Salon");
    }

    [Fact]
    public async Task Reminder_MoreThan24HoursBefore_IsNotSent()
    {
        var (client, owner) = await CreateOwnerAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client, "musteri@test.com");
        (await PostAppointmentAsync(client, salon, ReminderStart)).EnsureSuccessStatusCode();

        var sent = await SendRemindersAsync(owner, ReminderStart.AddHours(-25));

        sent.ShouldBe(0);
    }

    [Fact]
    public async Task Reminder_BookedWithin24Hours_IsNotSent()
    {
        var (client, owner) = await CreateOwnerAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client, "musteri@test.com");
        (await PostAppointmentAsync(client, salon, ReminderStart)).EnsureSuccessStatusCode();
        var id = (await GetAppointmentsAsync(client)).Single().Id;
        // randevu başlamadan 20 saat önce alınmış gibi
        await SetCreatedAtAsync(owner, id, ReminderStart.AddHours(-20));

        var sent = await SendRemindersAsync(owner, ReminderStart.AddHours(-12));

        sent.ShouldBe(0);
    }

    [Fact]
    public async Task Reminder_ForCancelledAppointment_IsNotSent()
    {
        var (client, owner) = await CreateOwnerAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client, "musteri@test.com");
        (await PostAppointmentAsync(client, salon, ReminderStart)).EnsureSuccessStatusCode();
        var id = (await GetAppointmentsAsync(client)).Single().Id;
        (await client.PutAsJsonAsync(
            $"/api/appointments/{id}/cancel", new AppointmentCancelCommand(id, null), CancellationToken))
            .EnsureSuccessStatusCode();

        var sent = await SendRemindersAsync(owner, ReminderStart.AddHours(-12));

        sent.ShouldBe(0);
    }

    [Fact]
    public async Task Reminder_AfterReschedule_IsSentAgain()
    {
        var (client, owner) = await CreateOwnerAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client, "musteri@test.com");
        (await PostAppointmentAsync(client, salon, ReminderStart)).EnsureSuccessStatusCode();
        var id = (await GetAppointmentsAsync(client)).Single().Id;
        (await SendRemindersAsync(owner, ReminderStart.AddHours(-12))).ShouldBe(1);

        (await client.PutAsJsonAsync(
            $"/api/appointments/{id}/reschedule",
            new AppointmentRescheduleCommand(id, salon.StaffId, ReminderStart.AddHours(2)),
            CancellationToken)).EnsureSuccessStatusCode();
        var sent = await SendRemindersAsync(owner, ReminderStart.AddHours(-11));

        sent.ShouldBe(1);
        Mail.Mails.Count(m => m.Subject.StartsWith("Randevu hatırlatması")).ShouldBe(2);
    }

    [Fact]
    public async Task Reminder_WhenMailFails_IsRetriedNextRun()
    {
        var (client, owner) = await CreateOwnerAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client, "musteri@test.com");
        (await PostAppointmentAsync(client, salon, ReminderStart)).EnsureSuccessStatusCode();
        Mail.Reset();

        Mail.Fail = true;
        var failed = await SendRemindersAsync(owner, ReminderStart.AddHours(-12));
        Mail.Fail = false;
        var retried = await SendRemindersAsync(owner, ReminderStart.AddHours(-11));

        failed.ShouldBe(0);
        retried.ShouldBe(1);
        Mail.Mails.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Reminder_OnlySendsCurrentSalonsAppointments()
    {
        var (clientA, ownerA) = await CreateOwnerAsync("salon-a", "a@test.com");
        var (clientB, _) = await CreateOwnerAsync("salon-b", "b@test.com");
        var salonA = await SetupSalonAsync(clientA, "musteri-a@test.com");
        var salonB = await SetupSalonAsync(clientB, "musteri-b@test.com");
        (await PostAppointmentAsync(clientA, salonA, ReminderStart)).EnsureSuccessStatusCode();
        (await PostAppointmentAsync(clientB, salonB, ReminderStart)).EnsureSuccessStatusCode();
        Mail.Reset();

        var sent = await SendRemindersAsync(ownerA, ReminderStart.AddHours(-12));

        sent.ShouldBe(1);
        Mail.Mails.ShouldHaveSingleItem().To.ShouldBe("musteri-a@test.com");
    }
    #endregion

    #region Yardımcılar
    private sealed record SalonData(Guid ServiceId, Guid StaffId, Guid CustomerId);

    private async Task<(HttpClient Client, CurrentUserDto Owner)> CreateOwnerAsync(string slug, string email)
    {
        var tokens = await RegisterAsync(slug, email);
        var owner = await GetMeAsync(tokens.AccessToken);
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", tokens.AccessToken);
        return (client, owner);
    }

    // arka plan servisinin bir turda o salon için yaptığı iş
    private async Task<int> SendRemindersAsync(CurrentUserDto owner, DateTimeOffset now)
    {
        using var scope = CreateScopeAs(owner);
        var reminderService = scope.ServiceProvider.GetRequiredService<AppointmentReminderService>();
        return await reminderService.SendDueRemindersAsync(now, CancellationToken);
    }

    private async Task SetCreatedAtAsync(CurrentUserDto owner, Guid appointmentId, DateTimeOffset createdAt)
    {
        using var scope = CreateScopeAs(owner);
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Set<Appointment>()
            .Where(a => a.Id == appointmentId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.CreatedAt, createdAt), CancellationToken);
    }

    private async Task<SalonData> SetupSalonAsync(HttpClient client, string? customerEmail)
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

        var customerCommand = new CustomerCreateCommand("Elif", "Yilmaz", "0532 123 45 67", customerEmail, null);
        (await client.PostAsJsonAsync("/api/customers", customerCommand, CancellationToken)).EnsureSuccessStatusCode();
        var customers = await client.GetFromJsonAsync<ODataResponse<CustomerDto>>("/api/odata/customers", CancellationToken);
        var customerId = customers!.Value.Single().Id;

        return new SalonData(serviceId, staffId, customerId);
    }

    private Task<HttpResponseMessage> PostAppointmentAsync(HttpClient client, SalonData salon, DateTimeOffset startAt)
    {
        var command = new AppointmentCreateCommand(salon.CustomerId, salon.StaffId, [salon.ServiceId], startAt, null);
        return client.PostAsJsonAsync("/api/appointments", command, CancellationToken);
    }

    private async Task<List<AppointmentDto>> GetAppointmentsAsync(HttpClient client)
    {
        var response = await client.GetFromJsonAsync<ODataResponse<AppointmentDto>>("/api/odata/appointments", CancellationToken);
        return response!.Value;
    }
    #endregion
}
