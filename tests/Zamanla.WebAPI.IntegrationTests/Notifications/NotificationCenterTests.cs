using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Shouldly;
using TS.Result;
using Zamanla.Application.Appointments;
using Zamanla.Application.Customers;
using Zamanla.Application.Notifications;
using Zamanla.Application.Public;
using Zamanla.Application.SalonServices;
using Zamanla.Application.Staffs;
using Zamanla.Application.TimeOffs;
using Zamanla.Domain.Staffs.ValueObjects;
using Zamanla.Infrastructure.Hubs;
using Zamanla.WebAPI.IntegrationTests.Infrastructure;

namespace Zamanla.WebAPI.IntegrationTests.Notifications;

// hizmet 45 dk + 10 dk hazırlık; personel pazartesi 09–18 çalışır
[Collection(IntegrationTestCollection.Name)]
public sealed class NotificationCenterTests(ZamanlaApiFactory factory) : IntegrationTestBase(factory)
{
    private static readonly TimeSpan EventTimeout = TimeSpan.FromSeconds(5);

    #region Bildirimler
    [Fact]
    public async Task PublicBooking_CreatesNotification()
    {
        var (client, _) = await CreateOwnerAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);

        await BookOnlineAsync("salon-a", salon, NextMonday(10));

        var notification = (await GetNotificationsAsync(client)).ShouldHaveSingleItem();
        notification.Type.ShouldBe("Yeni randevu");
        notification.CustomerFullName.ShouldBe("Ayse Yilmaz");
        notification.AppointmentStartAt.ShouldBe(NextMonday(10));
        notification.IsRead.ShouldBeFalse();
    }

    [Fact]
    public async Task PublicRescheduleAndCancel_CreateNotifications()
    {
        var (client, _) = await CreateOwnerAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        var token = await BookOnlineAsync("salon-a", salon, NextMonday(10));

        (await Client.PutAsJsonAsync(
            $"/api/public/salon-a/manage/{token}/reschedule",
            new PublicAppointmentRescheduleCommand(token, salon.StaffId, NextMonday(14)),
            CancellationToken)).EnsureSuccessStatusCode();
        (await Client.PutAsync($"/api/public/salon-a/manage/{token}/cancel", null, CancellationToken))
            .EnsureSuccessStatusCode();

        var types = (await GetNotificationsAsync(client)).Select(n => n.Type);
        types.ShouldBe(new[] { "Randevu iptal edildi", "Randevu saati değişti", "Yeni randevu" });
    }

    [Fact]
    public async Task PanelBooking_DoesNotCreateNotification()
    {
        var (client, _) = await CreateOwnerAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);

        (await PostAppointmentAsync(client, salon, NextMonday(10))).EnsureSuccessStatusCode();

        (await GetNotificationsAsync(client)).ShouldBeEmpty();
    }

    [Fact]
    public async Task Notification_AfterCustomerDeleted_ShowsAnonymizedName()
    {
        var (client, _) = await CreateOwnerAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        await BookOnlineAsync("salon-a", salon, NextMonday(10));
        var customers = await client.GetFromJsonAsync<ODataResponse<CustomerDto>>("/api/odata/customers", CancellationToken);
        var customerId = customers!.Value.Single(c => c.FirstName == "Ayse").Id;

        // ileri tarihli randevusu olan müşteri silinemez; panelden iptal bildirim oluşturmaz
        var appointments = await client.GetFromJsonAsync<ODataResponse<AppointmentDto>>("/api/odata/appointments", CancellationToken);
        var appointmentId = appointments!.Value.Single().Id;
        (await client.PutAsJsonAsync(
            $"/api/appointments/{appointmentId}/cancel", new AppointmentCancelCommand(appointmentId, null), CancellationToken))
            .EnsureSuccessStatusCode();

        (await client.DeleteAsync($"/api/customers/{customerId}", CancellationToken)).EnsureSuccessStatusCode();

        // KVKK: bildirim adı saklamadığı için silinen müşterinin adı burada da görünmez
        (await GetNotificationsAsync(client)).ShouldHaveSingleItem().CustomerFullName.ShouldBe("Silinmiş Müşteri");
    }

    [Fact]
    public async Task Read_MarksOnlyThatNotification()
    {
        var (client, _) = await CreateOwnerAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        await BookOnlineAsync("salon-a", salon, NextMonday(10));
        await BookOnlineAsync("salon-a", salon, NextMonday(14), "Mehmet");
        var first = (await GetNotificationsAsync(client)).Last();

        var response = await client.PutAsync($"/api/notifications/{first.Id}/read", null, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var notifications = await GetNotificationsAsync(client);
        notifications.Single(n => n.Id == first.Id).IsRead.ShouldBeTrue();
        notifications.Count(n => !n.IsRead).ShouldBe(1);
    }

    [Fact]
    public async Task ReadAll_DoesNotChangeOtherSalon()
    {
        var (clientA, _) = await CreateOwnerAsync("salon-a", "a@test.com");
        var (clientB, _) = await CreateOwnerAsync("salon-b", "b@test.com");
        var salonA = await SetupSalonAsync(clientA);
        var salonB = await SetupSalonAsync(clientB);
        await BookOnlineAsync("salon-a", salonA, NextMonday(10));
        await BookOnlineAsync("salon-b", salonB, NextMonday(10));

        (await clientA.PutAsync("/api/notifications/read-all", null, CancellationToken)).EnsureSuccessStatusCode();

        (await GetNotificationsAsync(clientA)).ShouldAllBe(n => n.IsRead);
        (await GetNotificationsAsync(clientB)).ShouldHaveSingleItem().IsRead.ShouldBeFalse();
    }

    [Fact]
    public async Task OtherSalonsNotification_IsNotAccessible()
    {
        var (clientA, _) = await CreateOwnerAsync("salon-a", "a@test.com");
        var (clientB, _) = await CreateOwnerAsync("salon-b", "b@test.com");
        var salonA = await SetupSalonAsync(clientA);
        await BookOnlineAsync("salon-a", salonA, NextMonday(10));
        var id = (await GetNotificationsAsync(clientA)).Single().Id;

        var response = await clientB.PutAsync($"/api/notifications/{id}/read", null, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await GetNotificationsAsync(clientB)).ShouldBeEmpty();
        (await GetNotificationsAsync(clientA)).Single().IsRead.ShouldBeFalse();
    }
    #endregion

    #region Canlı güncelleme
    [Fact]
    public async Task Hub_WithoutToken_IsRejected()
    {
        await using var connection = CreateHubConnection(null);

        await Should.ThrowAsync<HttpRequestException>(() => connection.StartAsync(CancellationToken));
    }

    [Fact]
    public async Task PanelBooking_SendsCalendarChanged_OnlyToOwnSalon()
    {
        var (clientA, tokenA) = await CreateOwnerAsync("salon-a", "a@test.com");
        var (_, tokenB) = await CreateOwnerAsync("salon-b", "b@test.com");
        var salonA = await SetupSalonAsync(clientA);
        await using var connectionA = await ConnectAsync(tokenA);
        await using var connectionB = await ConnectAsync(tokenB);
        var calendarA = Listen(connectionA, NotificationHub.CalendarChanged);
        var calendarB = Listen(connectionB, NotificationHub.CalendarChanged);
        var notificationA = Listen(connectionA, NotificationHub.NotificationCreated);

        (await PostAppointmentAsync(clientA, salonA, NextMonday(10))).EnsureSuccessStatusCode();

        await calendarA.WaitAsync(EventTimeout, CancellationToken);
        // A'ya gelen olay B'ye de gidecek olsaydı bu sürede gelmiş olurdu
        await Task.Delay(300, CancellationToken);
        calendarB.IsCompleted.ShouldBeFalse();
        notificationA.IsCompleted.ShouldBeFalse();
    }

    [Fact]
    public async Task PublicBooking_SendsNotificationCreated()
    {
        var (client, token) = await CreateOwnerAsync("salon-a", "a@test.com");
        var salon = await SetupSalonAsync(client);
        await using var connection = await ConnectAsync(token);
        var calendarChanged = Listen(connection, NotificationHub.CalendarChanged);
        var notificationCreated = Listen(connection, NotificationHub.NotificationCreated);

        await BookOnlineAsync("salon-a", salon, NextMonday(10));

        await calendarChanged.WaitAsync(EventTimeout, CancellationToken);
        await notificationCreated.WaitAsync(EventTimeout, CancellationToken);
    }

    [Fact]
    public async Task TimeOff_SendsCalendarChanged()
    {
        var (client, token) = await CreateOwnerAsync("salon-a", "a@test.com");
        await using var connection = await ConnectAsync(token);
        var calendarChanged = Listen(connection, NotificationHub.CalendarChanged);

        var command = new TimeOffCreateCommand(null, NextMonday(0), NextMonday(0).AddDays(1), null);
        (await client.PostAsJsonAsync("/api/time-offs", command, CancellationToken)).EnsureSuccessStatusCode();

        await calendarChanged.WaitAsync(EventTimeout, CancellationToken);
    }
    #endregion

    #region Yardımcılar
    private sealed record SalonData(Guid ServiceId, Guid StaffId, Guid CustomerId);

    private async Task<(HttpClient Client, string AccessToken)> CreateOwnerAsync(string slug, string email)
    {
        var tokens = await RegisterAsync(slug, email);
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", tokens.AccessToken);
        return (client, tokens.AccessToken);
    }

    // test sunucusu websocket desteklemiyor; long polling aynı hub'a HTTP üzerinden bağlanır
    private HubConnection CreateHubConnection(string? accessToken) =>
        new HubConnectionBuilder()
            .WithUrl(new Uri(Factory.Server.BaseAddress, "/api/hubs/notifications"), options =>
            {
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult(accessToken);
            })
            .Build();

    private async Task<HubConnection> ConnectAsync(string accessToken)
    {
        var connection = CreateHubConnection(accessToken);
        await connection.StartAsync(CancellationToken);
        // sunucu gruba eklemeyi bağlantı kurulduktan hemen sonra yapıyor; olay kaçmasın diye kısa bekleme
        await Task.Delay(200, CancellationToken);
        return connection;
    }

    // olay gelince tamamlanan bir görev döner
    private static Task Listen(HubConnection connection, string eventName)
    {
        TaskCompletionSource received = new(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On(eventName, () => received.TrySetResult());
        return received.Task;
    }

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

    // yönetim linkindeki token'ı döner
    private async Task<string> BookOnlineAsync(string slug, SalonData salon, DateTimeOffset startAt, string firstName = "Ayse")
    {
        var command = new PublicAppointmentCreateCommand(
            firstName, "Yilmaz", "0532 111 22 33", null, salon.StaffId, [salon.ServiceId], startAt, null);
        var response = await Client.PostAsJsonAsync($"/api/public/{slug}/appointments", command, CancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Result<PublicAppointmentCreatedDto>>(CancellationToken);
        return result!.Data!.ManageToken;
    }

    private Task<HttpResponseMessage> PostAppointmentAsync(HttpClient client, SalonData salon, DateTimeOffset startAt)
    {
        var command = new AppointmentCreateCommand(salon.CustomerId, salon.StaffId, [salon.ServiceId], startAt, null);
        return client.PostAsJsonAsync("/api/appointments", command, CancellationToken);
    }

    private async Task<List<NotificationDto>> GetNotificationsAsync(HttpClient client)
    {
        var response = await client.GetFromJsonAsync<ODataResponse<NotificationDto>>(
            "/api/odata/notifications?$orderby=createdAt desc", CancellationToken);
        return response!.Value;
    }
    #endregion
}
