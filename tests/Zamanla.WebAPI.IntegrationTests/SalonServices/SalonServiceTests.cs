using System.Net;
using System.Net.Http.Json;
using Shouldly;
using TS.Result;
using Zamanla.Application.SalonServices;
using Zamanla.WebAPI.IntegrationTests.Infrastructure;

namespace Zamanla.WebAPI.IntegrationTests.SalonServices;

[Collection(IntegrationTestCollection.Name)]
public sealed class SalonServiceTests(ZamanlaApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Create_WithValidData_AppearsInList()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");

        var response = await client.PostAsJsonAsync("/api/services", NewService("Sac kesimi"), CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var service = (await GetServicesAsync(client)).ShouldHaveSingleItem();
        service.Name.ShouldBe("Sac kesimi");
        service.CreatedFullName.ShouldBe("Test Kullanıcı");
    }

    [Fact]
    public async Task Create_WithExistingName_ReturnsConflict()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        await CreateServiceAsync(client, "Sac kesimi");

        var response = await client.PostAsJsonAsync("/api/services", NewService("Sac kesimi"), CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_WithInvalidDuration_ReturnsUnprocessableEntity()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");

        var command = NewService("Sac kesimi") with { DurationMinutes = 0 };
        var response = await client.PostAsJsonAsync("/api/services", command, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Update_WithValidData_UpdatesService()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var id = await CreateServiceAsync(client, "Sac kesimi");

        var command = new SalonServiceUpdateCommand(id, "Sac kesimi", "Sac", 45, 10, 450, IsActive: false);
        var response = await client.PutAsJsonAsync("/api/services", command, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await client.GetFromJsonAsync<Result<SalonServiceDto>>($"/api/services/{id}", CancellationToken);
        result!.Data!.Price.ShouldBe(450m);
        result.Data.IsActive.ShouldBeFalse();
        result.Data.UpdatedFullName.ShouldBe("Test Kullanıcı");
    }

    [Fact]
    public async Task Delete_RemovesServiceFromList()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var id = await CreateServiceAsync(client, "Sac kesimi");

        var response = await client.DeleteAsync($"/api/services/{id}", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await GetServicesAsync(client)).ShouldBeEmpty();

        // silinen hizmetin adı tekrar kullanılabilmeli
        var recreate = await client.PostAsJsonAsync("/api/services", NewService("Sac kesimi"), CancellationToken);
        recreate.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OtherTenantService_IsNotAccessible()
    {
        using var ownerA = await CreateOwnerClientAsync("salon-a", "a@test.com");
        using var ownerB = await CreateOwnerClientAsync("salon-b", "b@test.com");
        var id = await CreateServiceAsync(ownerA, "Sac kesimi");

        var get = await ownerB.GetAsync($"/api/services/{id}", CancellationToken);
        var update = await ownerB.PutAsJsonAsync(
            "/api/services",
            new SalonServiceUpdateCommand(id, "Ele gecirildi", "Sac", 45, 10, 1, true),
            CancellationToken);
        var delete = await ownerB.DeleteAsync($"/api/services/{id}", CancellationToken);

        get.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        update.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        delete.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await GetServicesAsync(ownerB)).ShouldBeEmpty();
        (await GetServicesAsync(ownerA)).ShouldHaveSingleItem().Name.ShouldBe("Sac kesimi");

        // isim benzersizliği işletme bazında: B aynı adı kullanabilir
        var sameName = await ownerB.PostAsJsonAsync("/api/services", NewService("Sac kesimi"), CancellationToken);
        sameName.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task List_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/odata/services", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private static SalonServiceCreateCommand NewService(string name) => new(name, "Sac", 45, 10, 400);

    private async Task<List<SalonServiceDto>> GetServicesAsync(HttpClient client)
    {
        var response = await client.GetFromJsonAsync<ODataResponse<SalonServiceDto>>("/api/odata/services", CancellationToken);
        return response!.Value;
    }

    private async Task<Guid> CreateServiceAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/services", NewService(name), CancellationToken);
        response.EnsureSuccessStatusCode();
        return (await GetServicesAsync(client)).Single(s => s.Name == name).Id;
    }
}
