using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TS.Result;
using Zamanla.Application.Appointments;
using Zamanla.Application.Auth;
using Zamanla.Application.Customers;
using Zamanla.Application.Dashboard;
using Zamanla.Application.Invitations;
using Zamanla.Application.Permissions;
using Zamanla.Application.Public;
using Zamanla.Application.SalonServices;
using Zamanla.Application.Services;
using Zamanla.Application.Staffs;
using Zamanla.Domain.Invitations;
using Zamanla.Domain.Staffs.ValueObjects;
using Zamanla.Infrastructure.Context;
using Zamanla.WebAPI.IntegrationTests.Infrastructure;

namespace Zamanla.WebAPI.IntegrationTests.Staffs;

// salonda iki personel var: Elif davet edilip hesap açar, Mert'in hesabı yoktur; ikisi de pazartesi 09–18 çalışır
[Collection(IntegrationTestCollection.Name)]
public sealed class StaffAccountTests(ZamanlaApiFactory factory) : IntegrationTestBase(factory)
{
    private const string Slug = "salon-a";
    private const string ElifEmail = "elif@test.com";
    private const string Password = "Personel123";

    #region Davet
    [Fact]
    public async Task Invite_SendsMailWithLink()
    {
        var salon = await SetupSalonAsync();

        var response = await InviteAsync(salon.Owner, salon.ElifId, ElifEmail);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var created = (await response.Content.ReadFromJsonAsync<Result<InvitationCreatedDto>>(CancellationToken))!.Data!;
        created.EmailSent.ShouldBeTrue();
        created.Link.ShouldContain($"/davet/{Slug}/");
        var mail = Factory.MailService.Mails.ShouldHaveSingleItem();
        mail.To.ShouldBe(ElifEmail);
        mail.Subject.ShouldBe("Panel davetiniz · Test Salon");
        mail.Body.ShouldContain(created.Link);
    }

    [Fact]
    public async Task Accept_OpensStaffAccountWithInvitedPermissions()
    {
        var salon = await SetupSalonAsync();
        var token = await InviteAndGetTokenAsync(salon.Owner, salon.ElifId, ElifEmail, PermissionCatalog.TimeOffsManage);

        var response = await AcceptAsync(token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tokens = (await response.Content.ReadFromJsonAsync<Result<AuthTokens>>(CancellationToken))!.Data!;
        var me = await GetMeAsync(tokens.AccessToken);
        me.Role.ShouldBe("Staff");
        me.FirstName.ShouldBe("Elif");
        me.StaffId.ShouldBe(salon.ElifId);
        me.Permissions.ShouldHaveSingleItem().ShouldBe(PermissionCatalog.TimeOffsManage);
    }

    [Fact]
    public async Task Accept_UsedLink_Returns404()
    {
        var salon = await SetupSalonAsync();
        var token = await InviteAndGetTokenAsync(salon.Owner, salon.ElifId, ElifEmail);
        (await AcceptAsync(token)).EnsureSuccessStatusCode();

        var response = await AcceptAsync(token);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Accept_ExpiredLink_Returns410()
    {
        var salon = await SetupSalonAsync();
        var token = await InviteAndGetTokenAsync(salon.Owner, salon.ElifId, ElifEmail);
        using (var scope = CreateScopeAs(salon.OwnerUser))
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Set<Invitation>()
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.ExpiresAt, DateTimeOffset.UtcNow.AddMinutes(-1)), CancellationToken);
        }

        var response = await AcceptAsync(token);

        response.StatusCode.ShouldBe(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task Reinvite_InvalidatesOldLink()
    {
        var salon = await SetupSalonAsync();
        var oldToken = await InviteAndGetTokenAsync(salon.Owner, salon.ElifId, ElifEmail);
        var newToken = await InviteAndGetTokenAsync(salon.Owner, salon.ElifId, ElifEmail);

        (await Client.GetAsync($"/api/public/{Slug}/invitations/{oldToken}", CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await Client.GetAsync($"/api/public/{Slug}/invitations/{newToken}", CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WithdrawInvitation_InvalidatesLink()
    {
        var salon = await SetupSalonAsync();
        var token = await InviteAndGetTokenAsync(salon.Owner, salon.MertId, "mert@test.com");

        var response = await salon.Owner.DeleteAsync($"/api/invitations/{salon.MertId}", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await Client.GetAsync($"/api/public/{Slug}/invitations/{token}", CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await GetAccountsAsync(salon.Owner)).Single(a => a.StaffId == salon.MertId).Status.ShouldBe(StaffAccountStatus.None);
        // geri alınacak bekleyen davet kalmadı
        (await salon.Owner.DeleteAsync($"/api/invitations/{salon.MertId}", CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Invite_UsedEmail_Returns409()
    {
        var salon = await SetupSalonAsync();

        var response = await InviteAsync(salon.Owner, salon.ElifId, "a@test.com");

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Invite_ByStaff_Returns403()
    {
        var salon = await SetupSalonAsync();
        using var elif = await CreateElifClientAsync(salon);

        var response = await InviteAsync(elif, salon.MertId, "mert@test.com");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await ReadErrorAsync(response)).ShouldBe("Bu işlemi sadece salon sahibi yapabilir");
    }
    #endregion

    #region Yetki ve Hesap Durumu
    [Fact]
    public async Task Staff_WithoutPermission_Gets403_AndGrantAppliesImmediately()
    {
        var salon = await SetupSalonAsync();
        using var elif = await CreateElifClientAsync(salon);

        var denied = await UpdateServiceAsync(elif, salon.ServiceId);
        denied.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await ReadErrorAsync(denied)).ShouldContain("Hizmetleri yönetir");

        // aynı token'la, yeniden giriş yapmadan
        (await SetPermissionsAsync(salon, PermissionCatalog.ServicesManage)).EnsureSuccessStatusCode();
        (await UpdateServiceAsync(elif, salon.ServiceId)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeactivatedAccount_CannotUseTokenLoginOrRefresh()
    {
        var salon = await SetupSalonAsync();
        var tokens = await CreateElifTokensAsync(salon, PermissionCatalog.ServicesManage);
        using var elif = CreateClientWith(tokens.AccessToken);

        var status = new StaffAccountStatusUpdateCommand(salon.ElifUserId(), false);
        (await salon.Owner.PutAsJsonAsync("/api/staff/accounts/status", status, CancellationToken)).EnsureSuccessStatusCode();

        var update = await UpdateServiceAsync(elif, salon.ServiceId);
        update.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await ReadErrorAsync(update)).ShouldBe("Hesabınız pasif durumda");
        (await LoginAsync()).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var refresh = await Client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenCommand(tokens.RefreshToken), CancellationToken);
        refresh.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeletingStaff_ClosesAccount()
    {
        var salon = await SetupSalonAsync();
        await CreateElifTokensAsync(salon);

        (await salon.Owner.DeleteAsync($"/api/staff/{salon.ElifId}", CancellationToken)).EnsureSuccessStatusCode();

        (await LoginAsync()).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Accounts_ShowsStatusForEachStaff_OnlyToOwner()
    {
        var salon = await SetupSalonAsync();
        using var elif = await CreateElifClientAsync(salon);
        await InviteAsync(salon.Owner, salon.MertId, "mert@test.com");

        var accounts = await GetAccountsAsync(salon.Owner);

        accounts.Single(a => a.StaffId == salon.ElifId).Status.ShouldBe(StaffAccountStatus.Active);
        accounts.Single(a => a.StaffId == salon.MertId).Status.ShouldBe(StaffAccountStatus.Invited);
        (await elif.GetAsync("/api/staff/accounts", CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task OtherSalonOwner_CannotChangePermissions()
    {
        var salon = await SetupSalonAsync();
        await CreateElifTokensAsync(salon);
        using var ownerB = await CreateOwnerClientAsync("salon-b", "b@test.com");

        var command = new StaffAccountPermissionsUpdateCommand(salon.ElifUserId(), [PermissionCatalog.ServicesManage]);
        var response = await ownerB.PutAsJsonAsync("/api/staff/accounts/permissions", command, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    #endregion

    #region Randevu ve Ciro
    [Fact]
    public async Task Staff_ManagesOwnAppointments_OthersNeedPermission()
    {
        var salon = await SetupSalonAsync();
        using var elif = await CreateElifClientAsync(salon);
        (await PostAppointmentAsync(salon.Owner, salon, salon.ElifId, NextMonday(10))).EnsureSuccessStatusCode();
        (await PostAppointmentAsync(salon.Owner, salon, salon.MertId, NextMonday(10))).EnsureSuccessStatusCode();
        var appointments = await GetAppointmentsAsync(salon.Owner);
        var own = appointments.Single(a => a.StaffId == salon.ElifId).Id;
        var other = appointments.Single(a => a.StaffId == salon.MertId).Id;

        (await CancelAsync(elif, own)).StatusCode.ShouldBe(HttpStatusCode.OK);
        var denied = await CancelAsync(elif, other);
        denied.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await ReadErrorAsync(denied)).ShouldContain("Başka personelin randevularını yönetir");

        (await SetPermissionsAsync(salon, PermissionCatalog.AppointmentsManageAll)).EnsureSuccessStatusCode();
        (await CancelAsync(elif, other)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Staff_CannotBookForOtherStaff()
    {
        var salon = await SetupSalonAsync();
        using var elif = await CreateElifClientAsync(salon);

        (await PostAppointmentAsync(elif, salon, salon.MertId, NextMonday(10))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await PostAppointmentAsync(elif, salon, salon.ElifId, NextMonday(10))).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Dashboard_HidesRevenueWithoutPermission()
    {
        var salon = await SetupSalonAsync();
        using var elif = await CreateElifClientAsync(salon);

        var hidden = await GetDashboardAsync(elif);
        hidden.Today.ExpectedRevenue.ShouldBeNull();
        hidden.LastSevenDays.ShouldAllBe(d => d.Revenue == null);

        (await SetPermissionsAsync(salon, PermissionCatalog.RevenueView)).EnsureSuccessStatusCode();
        var visible = await GetDashboardAsync(elif);
        visible.Today.ExpectedRevenue.ShouldBe(0m);
        visible.LastSevenDays.ShouldAllBe(d => d.Revenue == 0m);
    }
    #endregion

    #region Yardımcılar
    private sealed class SalonData(HttpClient owner, CurrentUserDto ownerUser, Guid serviceId, Guid elifId, Guid mertId, Guid customerId)
    {
        public HttpClient Owner { get; } = owner;
        public CurrentUserDto OwnerUser { get; } = ownerUser;
        public Guid ServiceId { get; } = serviceId;
        public Guid ElifId { get; } = elifId;
        public Guid MertId { get; } = mertId;
        public Guid CustomerId { get; } = customerId;
        // Elif hesabını açınca doldurulur
        public Guid? ElifUser { get; set; }

        public Guid ElifUserId() => ElifUser ?? throw new InvalidOperationException("Elif'in hesabı açılmadı");
    }

    private async Task<SalonData> SetupSalonAsync()
    {
        var tokens = await RegisterAsync(Slug, "a@test.com");
        var ownerUser = await GetMeAsync(tokens.AccessToken);
        var owner = CreateClientWith(tokens.AccessToken);

        var serviceCommand = new SalonServiceCreateCommand("Sac kesimi", "Sac", 45, 10, 400);
        (await owner.PostAsJsonAsync("/api/services", serviceCommand, CancellationToken)).EnsureSuccessStatusCode();
        var services = await owner.GetFromJsonAsync<ODataResponse<SalonServiceDto>>("/api/odata/services", CancellationToken);
        var serviceId = services!.Value.Single().Id;

        foreach (var (firstName, color) in new[] { ("Elif", "peach"), ("Mert", "sage") })
        {
            var staffCommand = new StaffCreateCommand(
                firstName,
                "Test",
                color,
                [new WorkingHour(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(18, 0))],
                [serviceId]);
            (await owner.PostAsJsonAsync("/api/staff", staffCommand, CancellationToken)).EnsureSuccessStatusCode();
        }
        var staff = (await owner.GetFromJsonAsync<ODataResponse<StaffDto>>("/api/odata/staff", CancellationToken))!.Value;

        var customerCommand = new CustomerCreateCommand("Ayse", "Yilmaz", "0532 123 45 67", null, null);
        (await owner.PostAsJsonAsync("/api/customers", customerCommand, CancellationToken)).EnsureSuccessStatusCode();
        var customers = await owner.GetFromJsonAsync<ODataResponse<CustomerDto>>("/api/odata/customers", CancellationToken);

        return new SalonData(
            owner,
            ownerUser,
            serviceId,
            staff.Single(s => s.FirstName == "Elif").Id,
            staff.Single(s => s.FirstName == "Mert").Id,
            customers!.Value.Single().Id);
    }

    private HttpClient CreateClientWith(string accessToken)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);
        return client;
    }

    private Task<HttpResponseMessage> InviteAsync(HttpClient client, Guid staffId, string email, params string[] permissions)
    {
        var command = new InvitationCreateCommand(staffId, email, [.. permissions]);
        return client.PostAsJsonAsync("/api/invitations", command, CancellationToken);
    }

    // linkin son parçası davet anahtarı
    private async Task<string> InviteAndGetTokenAsync(HttpClient owner, Guid staffId, string email, params string[] permissions)
    {
        var response = await InviteAsync(owner, staffId, email, permissions);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<Result<InvitationCreatedDto>>(CancellationToken))!.Data!;
        return created.Link.Split('/')[^1];
    }

    private Task<HttpResponseMessage> AcceptAsync(string token)
    {
        var command = new PublicInvitationAcceptCommand(token, Password);
        return Client.PostAsJsonAsync($"/api/public/{Slug}/invitations/{token}/accept", command, CancellationToken);
    }

    // Elif davet edilir, hesabını açar; kullanıcı id'si salon verisine yazılır
    private async Task<AuthTokens> CreateElifTokensAsync(SalonData salon, params string[] permissions)
    {
        var token = await InviteAndGetTokenAsync(salon.Owner, salon.ElifId, ElifEmail, permissions);
        var response = await AcceptAsync(token);
        response.EnsureSuccessStatusCode();
        var tokens = (await response.Content.ReadFromJsonAsync<Result<AuthTokens>>(CancellationToken))!.Data!;
        salon.ElifUser = (await GetMeAsync(tokens.AccessToken)).Id;
        return tokens;
    }

    private async Task<HttpClient> CreateElifClientAsync(SalonData salon, params string[] permissions)
    {
        var tokens = await CreateElifTokensAsync(salon, permissions);
        return CreateClientWith(tokens.AccessToken);
    }

    private Task<HttpResponseMessage> SetPermissionsAsync(SalonData salon, params string[] permissions)
    {
        var command = new StaffAccountPermissionsUpdateCommand(salon.ElifUserId(), [.. permissions]);
        return salon.Owner.PutAsJsonAsync("/api/staff/accounts/permissions", command, CancellationToken);
    }

    private Task<HttpResponseMessage> LoginAsync()
    {
        return Client.PostAsJsonAsync("/api/auth/login", new LoginCommand(ElifEmail, Password), CancellationToken);
    }

    private Task<HttpResponseMessage> UpdateServiceAsync(HttpClient client, Guid serviceId)
    {
        var command = new SalonServiceUpdateCommand(serviceId, "Sac kesimi", "Sac", 45, 10, 400, true);
        return client.PutAsJsonAsync("/api/services", command, CancellationToken);
    }

    private Task<HttpResponseMessage> PostAppointmentAsync(HttpClient client, SalonData salon, Guid staffId, DateTimeOffset startAt)
    {
        var command = new AppointmentCreateCommand(salon.CustomerId, staffId, [salon.ServiceId], startAt, null);
        return client.PostAsJsonAsync("/api/appointments", command, CancellationToken);
    }

    private Task<HttpResponseMessage> CancelAsync(HttpClient client, Guid appointmentId)
    {
        var command = new AppointmentCancelCommand(appointmentId, "Deneme");
        return client.PutAsJsonAsync($"/api/appointments/{appointmentId}/cancel", command, CancellationToken);
    }

    private async Task<List<AppointmentDto>> GetAppointmentsAsync(HttpClient client)
    {
        var response = await client.GetFromJsonAsync<ODataResponse<AppointmentDto>>("/api/odata/appointments", CancellationToken);
        return response!.Value;
    }

    private async Task<List<StaffAccountDto>> GetAccountsAsync(HttpClient client)
    {
        var result = await client.GetFromJsonAsync<Result<List<StaffAccountDto>>>("/api/staff/accounts", CancellationToken);
        return result!.Data!;
    }

    private async Task<DashboardDto> GetDashboardAsync(HttpClient client)
    {
        var result = await client.GetFromJsonAsync<Result<DashboardDto>>("/api/dashboard", CancellationToken);
        return result!.Data!;
    }

    private async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        var result = await response.Content.ReadFromJsonAsync<Result<string>>(CancellationToken);
        return result!.ErrorMessages!.Single();
    }
    #endregion
}
