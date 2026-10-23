using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TS.Result;
using Zamanla.Application.Auth;
using Zamanla.Application.Services;
using Zamanla.Domain.Users;
using Zamanla.Infrastructure.Context;
using Zamanla.WebAPI.IntegrationTests.Infrastructure;

namespace Zamanla.WebAPI.IntegrationTests.Auth;

// RegisterAsync'in açtığı salon sahibi: owner@test.com / Zamanla123
[Collection(IntegrationTestCollection.Name)]
public sealed class PasswordResetTests(ZamanlaApiFactory factory) : IntegrationTestBase(factory)
{
    private const string Email = "owner@test.com";
    private const string OldPassword = "Zamanla123";
    private const string NewPassword = "YeniSifre123";

    #region Şifremi unuttum
    [Fact]
    public async Task ForgotPassword_SendsMailWithLink()
    {
        await RegisterAsync(email: Email);

        var response = await ForgotPasswordAsync(Email);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var mail = Factory.MailService.Mails.ShouldHaveSingleItem();
        mail.To.ShouldBe(Email);
        mail.Subject.ShouldBe("Şifre sıfırlama · Zamanla");
        (await CheckAsync(ReadCode(mail))).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_ReturnsSameMessageWithoutMail()
    {
        await RegisterAsync(email: Email);
        var known = await ReadDataAsync(await ForgotPasswordAsync(Email));
        Factory.MailService.Reset();

        var response = await ForgotPasswordAsync("yok@test.com");

        // kayıtlı olup olmadığı cevaptan anlaşılmamalı
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ReadDataAsync(response)).ShouldBe(known);
        Factory.MailService.Mails.ShouldBeEmpty();
    }

    [Fact]
    public async Task ForgotPassword_NewRequest_InvalidatesOldLink()
    {
        await RegisterAsync(email: Email);
        var oldCode = await ForgotPasswordAndGetCodeAsync();
        var newCode = await ForgotPasswordAndGetCodeAsync();

        (await CheckAsync(oldCode)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CheckAsync(newCode)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    #endregion

    #region Şifre sıfırlama
    [Fact]
    public async Task ResetPassword_ChangesPasswordAndClosesSessions()
    {
        var tokens = await RegisterAsync(email: Email);
        var code = await ForgotPasswordAndGetCodeAsync();

        var response = await ResetPasswordAsync(code, NewPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await LoginAsync(OldPassword)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        var login = await LoginAsync(NewPassword);
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        var fresh = (await login.Content.ReadFromJsonAsync<Result<AuthTokens>>(CancellationToken))!.Data!;
        (await RefreshAsync(tokens.RefreshToken)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        // eski cihazın denemesi yeni girişi kapatmamalı
        (await RefreshAsync(fresh.RefreshToken)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_UsedLink_Returns404()
    {
        await RegisterAsync(email: Email);
        var code = await ForgotPasswordAndGetCodeAsync();
        (await ResetPasswordAsync(code, NewPassword)).EnsureSuccessStatusCode();

        var response = await ResetPasswordAsync(code, "BaskaSifre123");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CheckAsync(code)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResetPassword_ExpiredLink_Returns404()
    {
        var tokens = await RegisterAsync(email: Email);
        var code = await ForgotPasswordAndGetCodeAsync();
        var me = await GetMeAsync(tokens.AccessToken);
        using (var scope = CreateScopeAs(me))
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            DateTimeOffset? requestedAt = DateTimeOffset.UtcNow.AddHours(-AppUser.ForgotPasswordValidHours).AddMinutes(-1);
            await context.Set<AppUser>()
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.ForgotPasswordDate, requestedAt), CancellationToken);
        }

        var response = await ResetPasswordAsync(code, NewPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await LoginAsync(OldPassword)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    #endregion

    #region Şifre değiştirme
    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_Returns400()
    {
        var tokens = await RegisterAsync(email: Email);
        using var client = CreateClientWith(tokens.AccessToken);

        var response = await ChangePasswordAsync(client, "YanlisSifre1", NewPassword);

        // 401 dönseydi client oturumun bittiğini sanıp kullanıcıyı çıkışa atardı
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await LoginAsync(OldPassword)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_ReturnsNewTokensAndClosesOtherSessions()
    {
        var first = await RegisterAsync(email: Email);
        var other = (await (await LoginAsync(OldPassword)).Content.ReadFromJsonAsync<Result<AuthTokens>>(CancellationToken))!.Data!;
        using var client = CreateClientWith(first.AccessToken);

        var response = await ChangePasswordAsync(client, OldPassword, NewPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var current = (await response.Content.ReadFromJsonAsync<Result<AuthTokens>>(CancellationToken))!.Data!;
        (await RefreshAsync(other.RefreshToken)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        // diğer cihazın denemesi şifreyi değiştiren cihazın oturumunu kapatmamalı
        (await RefreshAsync(current.RefreshToken)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await LoginAsync(NewPassword)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    #endregion

    #region Yardımcılar
    private Task<HttpResponseMessage> ForgotPasswordAsync(string email) =>
        Client.PostAsJsonAsync("/api/auth/forgot-password", new ForgotPasswordCommand(email), CancellationToken);

    private async Task<string> ForgotPasswordAndGetCodeAsync()
    {
        Factory.MailService.Reset();
        (await ForgotPasswordAsync(Email)).EnsureSuccessStatusCode();
        return ReadCode(Factory.MailService.Mails.ShouldHaveSingleItem());
    }

    // maildeki linkin son parçası sıfırlama anahtarı
    private static string ReadCode(SentMail mail) =>
        mail.Body.Split('\n').Single(line => line.Contains("/sifre-sifirla/")).Split('/')[^1];

    private Task<HttpResponseMessage> CheckAsync(string code) =>
        Client.GetAsync($"/api/auth/check-forgot-password-code/{code}", CancellationToken);

    private Task<HttpResponseMessage> ResetPasswordAsync(string code, string newPassword) =>
        Client.PostAsJsonAsync("/api/auth/reset-password", new ResetPasswordCommand(code, newPassword), CancellationToken);

    private Task<HttpResponseMessage> ChangePasswordAsync(HttpClient client, string currentPassword, string newPassword) =>
        client.PutAsJsonAsync("/api/auth/password", new ChangePasswordCommand(currentPassword, newPassword), CancellationToken);

    private Task<HttpResponseMessage> LoginAsync(string password) =>
        Client.PostAsJsonAsync("/api/auth/login", new LoginCommand(Email, password), CancellationToken);

    private Task<HttpResponseMessage> RefreshAsync(string refreshToken) =>
        Client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenCommand(refreshToken), CancellationToken);

    private HttpClient CreateClientWith(string accessToken)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);
        return client;
    }

    private async Task<string?> ReadDataAsync(HttpResponseMessage response)
    {
        var result = await response.Content.ReadFromJsonAsync<Result<string>>(CancellationToken);
        return result!.Data;
    }
    #endregion
}
