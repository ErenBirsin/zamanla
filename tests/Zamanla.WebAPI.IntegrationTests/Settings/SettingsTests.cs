using System.Net;
using System.Net.Http.Json;
using Shouldly;
using TS.Result;
using Zamanla.Application.Public;
using Zamanla.Application.Settings;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Tenants.ValueObjects;
using Zamanla.WebAPI.IntegrationTests.Infrastructure;

namespace Zamanla.WebAPI.IntegrationTests.Settings;

[Collection(IntegrationTestCollection.Name)]
public sealed class SettingsTests(ZamanlaApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Get_NewSalon_ReturnsDefaultRules()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");

        var settings = await GetSettingsAsync(client);

        settings.Name.ShouldBe("Test Salon");
        settings.Slug.ShouldBe("salon-a");
        settings.Address.ShouldBeNull();
        settings.PhoneNumber.ShouldBeNull();
        settings.BookingPolicy.ShouldBe(BookingPolicy.Default);
    }

    [Fact]
    public async Task Update_WithValidData_SavesContactAndRules()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");

        var command = new SettingsUpdateCommand(
            " Makas Studio ",
            new Address(" İstanbul ", "Kadıköy", "Moda Cad. No: 12"),
            "0216 000 00 00",
            new BookingPolicy(30, 2, 3));
        var response = await client.PutAsJsonAsync("/api/settings", command, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var settings = await GetSettingsAsync(client);
        settings.Name.ShouldBe("Makas Studio");
        settings.Address.ShouldBe(new Address("İstanbul", "Kadıköy", "Moda Cad. No: 12"));
        settings.PhoneNumber.ShouldBe("+902160000000");
        settings.BookingPolicy.ShouldBe(new BookingPolicy(30, 2, 3));
        // link değişmez
        settings.Slug.ShouldBe("salon-a");
    }

    [Fact]
    public async Task Update_WithInvalidValues_ReturnsUnprocessableEntity()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");

        var command = new SettingsUpdateCommand("Test Salon", null, "123", new BookingPolicy(0, 2, 3));
        var response = await client.PutAsJsonAsync("/api/settings", command, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await GetSettingsAsync(client)).BookingPolicy.ShouldBe(BookingPolicy.Default);
    }

    [Fact]
    public async Task Update_DoesNotChangeOtherSalon()
    {
        using var clientA = await CreateOwnerClientAsync("salon-a", "a@test.com");
        using var clientB = await CreateOwnerClientAsync("salon-b", "b@test.com");

        var command = new SettingsUpdateCommand("Salon A", null, null, new BookingPolicy(7, 1, 2));
        (await clientA.PutAsJsonAsync("/api/settings", command, CancellationToken)).EnsureSuccessStatusCode();

        var settingsB = await GetSettingsAsync(clientB);
        settingsB.Name.ShouldBe("Test Salon");
        settingsB.BookingPolicy.ShouldBe(BookingPolicy.Default);
    }

    [Fact]
    public async Task PublicPage_UsesSalonRulesAndContact()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var command = new SettingsUpdateCommand(
            "Test Salon",
            new Address("İstanbul", "Kadıköy", "Moda Cad. No: 12"),
            "0216 000 00 00",
            new BookingPolicy(7, 2, 5));
        (await client.PutAsJsonAsync("/api/settings", command, CancellationToken)).EnsureSuccessStatusCode();

        var salon = await Client.GetFromJsonAsync<Result<PublicSalonDto>>("/api/public/salon-a", CancellationToken);
        salon!.Data!.BookingDaysAhead.ShouldBe(7);
        salon.Data.CancelHoursBefore.ShouldBe(2);
        salon.Data.PhoneNumber.ShouldBe("+902160000000");
        salon.Data.Address!.District.ShouldBe("Kadıköy");

        // varsayılan 60 günün içinde ama salonun 7 günlük sınırının dışında
        var date = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-dd");
        var response = await Client.GetAsync(
            $"/api/public/salon-a/availability?date={date}&salonServiceIds={Guid.NewGuid()}", CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private async Task<SettingsDto> GetSettingsAsync(HttpClient client)
    {
        var result = await client.GetFromJsonAsync<Result<SettingsDto>>("/api/settings", CancellationToken);
        return result!.Data!;
    }
}
