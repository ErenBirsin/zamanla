using System.Net;
using System.Net.Http.Json;
using Shouldly;
using TS.Result;
using Zamanla.Application.Auth;
using Zamanla.Application.Services;
using Zamanla.WebAPI.IntegrationTests.Infrastructure;

namespace Zamanla.WebAPI.IntegrationTests.Auth;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuthTests(ZamanlaApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Register_WithValidData_ReturnsTokens()
    {
        var tokens = await RegisterAsync();

        tokens.AccessToken.ShouldNotBeNullOrWhiteSpace();
        tokens.RefreshToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_WithExistingSlug_ReturnsConflict()
    {
        await RegisterAsync("ayni-salon", "ilk@test.com");

        var command = new RegisterBusinessCommand("Başka Salon", "ayni-salon", "Test", "Kullanıcı", "ikinci@test.com", "Zamanla123!");
        var response = await Client.PostAsJsonAsync("/api/auth/register", command, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithInvalidSlug_ReturnsUnprocessableEntity()
    {
        var command = new RegisterBusinessCommand("Test Salon", "Test Salon", "Test", "Kullanıcı", "owner@test.com", "Zamanla123!");
        var response = await Client.PostAsJsonAsync("/api/auth/register", command, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        await RegisterAsync(email: "owner@test.com");

        // mail büyük harfle yazılsa da aynı kullanıcı bulunmalı
        var response = await LoginAsync("OWNER@test.com", "Zamanla123!");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        await RegisterAsync(email: "owner@test.com");

        var response = await LoginAsync("owner@test.com", "YanlisSifre1");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokens()
    {
        var tokens = await RegisterAsync();

        var response = await RefreshAsync(tokens.RefreshToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<AuthTokens>>(CancellationToken);
        result!.Data!.RefreshToken.ShouldNotBe(tokens.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithUsedToken_RevokesAllSessions()
    {
        var first = await RegisterAsync();
        var refreshResponse = await RefreshAsync(first.RefreshToken);
        var second = (await refreshResponse.Content.ReadFromJsonAsync<Result<AuthTokens>>(CancellationToken))!.Data!;

        // kullanılmış token tekrar gelirse çalınmış sayılır
        var reuseResponse = await RefreshAsync(first.RefreshToken);
        reuseResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // bu yüzden yeni alınan token da artık geçersiz olmalı
        var afterReuseResponse = await RefreshAsync(second.RefreshToken);
        afterReuseResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/auth/me", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithToken_ReturnsCurrentUser()
    {
        var tokens = await RegisterAsync("kuafor-ali", "ali@test.com");

        var me = await GetMeAsync(tokens.AccessToken);

        me.Email.ShouldBe("ali@test.com");
        me.TenantSlug.ShouldBe("kuafor-ali");
        me.Role.ShouldBe("Owner");
    }

    private Task<HttpResponseMessage> LoginAsync(string email, string password) =>
        Client.PostAsJsonAsync("/api/auth/login", new LoginCommand(email, password), CancellationToken);

    private Task<HttpResponseMessage> RefreshAsync(string refreshToken) =>
        Client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenCommand(refreshToken), CancellationToken);
}
