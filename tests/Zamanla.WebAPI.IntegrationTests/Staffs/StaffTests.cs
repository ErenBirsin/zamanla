using System.Net;
using System.Net.Http.Json;
using System.Text;
using Shouldly;
using TS.Result;
using Zamanla.Application.SalonServices;
using Zamanla.Application.Staffs;
using Zamanla.Domain.Staffs.ValueObjects;
using Zamanla.WebAPI.IntegrationTests.Infrastructure;

namespace Zamanla.WebAPI.IntegrationTests.Staffs;

[Collection(IntegrationTestCollection.Name)]
public sealed class StaffTests(ZamanlaApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Create_WithValidData_AppearsInList()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var serviceId = await CreateServiceAsync(client, "Sac kesimi");

        var response = await client.PostAsJsonAsync("/api/staff", NewStaff(serviceId), CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var staff = (await GetStaffAsync(client)).ShouldHaveSingleItem();
        staff.FirstName.ShouldBe("Ayse");
        staff.Color.ShouldBe("peach");
        staff.WorkingHours.Count.ShouldBe(2);
        staff.WorkingHours[0].StartTime.ShouldBe(new TimeOnly(9, 0));
        staff.SalonServiceIds.ShouldHaveSingleItem().ShouldBe(serviceId);
        staff.CreatedFullName.ShouldBe("Test Kullanıcı");
    }

    [Fact]
    public async Task Create_WithOverlappingHours_ReturnsUnprocessableEntity()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");

        var command = NewStaff() with
        {
            WorkingHours =
            [
                new WorkingHour(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0)),
                new WorkingHour(DayOfWeek.Monday, new TimeOnly(11, 0), new TimeOnly(14, 0))
            ]
        };
        var response = await client.PostAsJsonAsync("/api/staff", command, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Update_ReplacesWorkingHoursAndServices()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var haircutId = await CreateServiceAsync(client, "Sac kesimi");
        var beardId = await CreateServiceAsync(client, "Sakal");
        var id = await CreateStaffAsync(client, NewStaff(haircutId));

        var command = new StaffUpdateCommand(
            id,
            "Ayse",
            "Sik",
            "sage",
            [new WorkingHour(DayOfWeek.Tuesday, new TimeOnly(10, 0), new TimeOnly(19, 0))],
            [beardId],
            IsActive: false);
        var response = await client.PutAsJsonAsync("/api/staff", command, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await client.GetFromJsonAsync<Result<StaffDto>>($"/api/staff/{id}", CancellationToken);
        var staff = result!.Data!;
        var hour = staff.WorkingHours.ShouldHaveSingleItem();
        hour.Day.ShouldBe((int)DayOfWeek.Tuesday);
        hour.StartTime.ShouldBe(new TimeOnly(10, 0));
        staff.SalonServiceIds.ShouldHaveSingleItem().ShouldBe(beardId);
        staff.Color.ShouldBe("sage");
        staff.IsActive.ShouldBeFalse();
        staff.UpdatedFullName.ShouldBe("Test Kullanıcı");
    }

    [Fact]
    public async Task Delete_RemovesStaffFromList()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var id = await CreateStaffAsync(client, NewStaff());

        var response = await client.DeleteAsync($"/api/staff/{id}", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await GetStaffAsync(client)).ShouldBeEmpty();
    }

    [Fact]
    public async Task Create_WithOtherTenantService_ReturnsNotFound()
    {
        using var ownerA = await CreateOwnerClientAsync("salon-a", "a@test.com");
        using var ownerB = await CreateOwnerClientAsync("salon-b", "b@test.com");
        var serviceOfA = await CreateServiceAsync(ownerA, "Sac kesimi");

        var response = await ownerB.PostAsJsonAsync("/api/staff", NewStaff(serviceOfA), CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await GetStaffAsync(ownerB)).ShouldBeEmpty();
    }

    [Fact]
    public async Task OtherTenantStaff_IsNotAccessible()
    {
        using var ownerA = await CreateOwnerClientAsync("salon-a", "a@test.com");
        using var ownerB = await CreateOwnerClientAsync("salon-b", "b@test.com");
        var id = await CreateStaffAsync(ownerA, NewStaff());

        var get = await ownerB.GetAsync($"/api/staff/{id}", CancellationToken);
        var update = await ownerB.PutAsJsonAsync(
            "/api/staff",
            new StaffUpdateCommand(id, "Ele", "Gecirildi", "sky", [], [], true),
            CancellationToken);
        var delete = await ownerB.DeleteAsync($"/api/staff/{id}", CancellationToken);

        get.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        update.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        delete.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await GetStaffAsync(ownerB)).ShouldBeEmpty();
        (await GetStaffAsync(ownerA)).ShouldHaveSingleItem().FirstName.ShouldBe("Ayse");
    }

    [Fact]
    public async Task Create_WithMalformedRequest_ReturnsBadRequest()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");

        const string json = """
            { "firstName": "Ayse", "lastName": "Sik", "color": "peach", "workingHours": [], "salonServiceIds": ["bozuk-id"] }
            """;
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/staff", content, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private static StaffCreateCommand NewStaff(params Guid[] salonServiceIds) => new(
        "Ayse",
        "Sik",
        "peach",
        [
            new WorkingHour(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0)),
            new WorkingHour(DayOfWeek.Monday, new TimeOnly(13, 0), new TimeOnly(18, 0))
        ],
        [.. salonServiceIds]);

    private async Task<List<StaffDto>> GetStaffAsync(HttpClient client)
    {
        var response = await client.GetFromJsonAsync<ODataResponse<StaffDto>>("/api/odata/staff", CancellationToken);
        return response!.Value;
    }

    private async Task<Guid> CreateStaffAsync(HttpClient client, StaffCreateCommand command)
    {
        var response = await client.PostAsJsonAsync("/api/staff", command, CancellationToken);
        response.EnsureSuccessStatusCode();
        return (await GetStaffAsync(client)).Single().Id;
    }

    private async Task<Guid> CreateServiceAsync(HttpClient client, string name)
    {
        var command = new SalonServiceCreateCommand(name, "Sac", 45, 10, 400);
        var response = await client.PostAsJsonAsync("/api/services", command, CancellationToken);
        response.EnsureSuccessStatusCode();

        var services = await client.GetFromJsonAsync<ODataResponse<SalonServiceDto>>("/api/odata/services", CancellationToken);
        return services!.Value.Single(s => s.Name == name).Id;
    }
}
