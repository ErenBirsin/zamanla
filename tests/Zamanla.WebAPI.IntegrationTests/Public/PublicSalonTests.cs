using System.Net;
using System.Net.Http.Json;
using Shouldly;
using TS.Result;
using Zamanla.Application.Public;
using Zamanla.Application.SalonServices;
using Zamanla.Application.Staffs;
using Zamanla.Domain.Staffs.ValueObjects;
using Zamanla.WebAPI.IntegrationTests.Infrastructure;

namespace Zamanla.WebAPI.IntegrationTests.Public;

[Collection(IntegrationTestCollection.Name)]
public sealed class PublicSalonTests(ZamanlaApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task UnknownSlug_ReturnsNotFound()
    {
        var response = await Client.GetAsync("/api/public/olmayan-salon", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_WithoutLogin_ReturnsOnlyBookableServices()
    {
        using var owner = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var haircutId = await CreateServiceAsync(owner, "Sac kesimi");
        await CreateServiceAsync(owner, "Personelsiz hizmet");
        await CreateStaffAsync(owner, haircutId);

        // Client oturum açmamış ziyaretçi
        var result = await Client.GetFromJsonAsync<Result<PublicSalonDto>>("/api/public/salon-a", CancellationToken);

        var salon = result!.Data!;
        salon.Name.ShouldBe("Test Salon");
        salon.Services.ShouldHaveSingleItem().Id.ShouldBe(haircutId);
        salon.Staff.ShouldHaveSingleItem().SalonServiceIds.ShouldHaveSingleItem().ShouldBe(haircutId);
    }

    [Fact]
    public async Task Get_WithOtherSalonsLogin_ReturnsSalonFromSlug()
    {
        using var ownerA = await CreateOwnerClientAsync("salon-a", "a@test.com");
        using var ownerB = await CreateOwnerClientAsync("salon-b", "b@test.com");
        var serviceB = await CreateServiceAsync(ownerB, "B hizmeti");
        await CreateStaffAsync(ownerB, serviceB);

        // A'nın oturumu açıkken B'nin sayfasında A'nın değil B'nin hizmetleri görünür
        var result = await ownerA.GetFromJsonAsync<Result<PublicSalonDto>>("/api/public/salon-b", CancellationToken);

        result!.Data!.Services.ShouldHaveSingleItem().Name.ShouldBe("B hizmeti");
    }

    private async Task<Guid> CreateServiceAsync(HttpClient client, string name)
    {
        var command = new SalonServiceCreateCommand(name, "Sac", 45, 10, 400);
        var response = await client.PostAsJsonAsync("/api/services", command, CancellationToken);
        response.EnsureSuccessStatusCode();

        var services = await client.GetFromJsonAsync<ODataResponse<SalonServiceDto>>("/api/odata/services", CancellationToken);
        return services!.Value.Single(s => s.Name == name).Id;
    }

    private async Task CreateStaffAsync(HttpClient client, Guid salonServiceId)
    {
        var command = new StaffCreateCommand(
            "Ayse",
            "Sik",
            "peach",
            [new WorkingHour(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(18, 0))],
            [salonServiceId]);
        var response = await client.PostAsJsonAsync("/api/staff", command, CancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
