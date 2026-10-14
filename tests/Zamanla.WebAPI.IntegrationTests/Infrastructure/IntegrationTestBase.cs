using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TS.Result;
using Zamanla.Application.Auth;
using Zamanla.Application.Services;
using Zamanla.Infrastructure.Services;

namespace Zamanla.WebAPI.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase(ZamanlaApiFactory factory) : IAsyncLifetime
{
    protected ZamanlaApiFactory Factory { get; } = factory;
    protected HttpClient Client { get; } = factory.CreateClient();
    protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    // her test boş veritabanıyla başlar
    public async ValueTask InitializeAsync() => await Factory.ResetDatabaseAsync();

    public ValueTask DisposeAsync()
    {
        Client.Dispose();
        return ValueTask.CompletedTask;
    }

    protected async Task<AuthTokens> RegisterAsync(string slug = "test-salon", string email = "owner@test.com")
    {
        var command = new RegisterBusinessCommand("Test Salon", slug, "Test", "Kullanıcı", email, "Zamanla123");
        var response = await Client.PostAsJsonAsync("/api/auth/register", command, CancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Result<AuthTokens>>(CancellationToken);
        return result!.Data!;
    }

    protected async Task<CurrentUserDto> GetMeAsync(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new("Bearer", accessToken);

        var response = await Client.SendAsync(request, CancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Result<CurrentUserDto>>(CancellationToken);
        return result!.Data!;
    }

    protected async Task<CurrentUserDto> RegisterAndGetUserAsync(string slug, string email)
    {
        var tokens = await RegisterAsync(slug, email);
        return await GetMeAsync(tokens.AccessToken);
    }

    // kayıt olur ve o işletmenin sahibi olarak istek atan bir client döner
    protected async Task<HttpClient> CreateOwnerClientAsync(string slug, string email)
    {
        var tokens = await RegisterAsync(slug, email);
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", tokens.AccessToken);
        return client;
    }

    // HTTP isteği olmadan, verilen kullanıcı giriş yapmış gibi bir scope açar
    protected IServiceScope CreateScopeAs(CurrentUserDto user)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimNames.TenantId, user.TenantId.ToString())
        ];

        var scope = Factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims))
        };
        return scope;
    }
}
