using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Zamanla.Application.Appointments;
using Zamanla.Application.Customers;
using Zamanla.Application.SalonServices;
using Zamanla.Application.Staffs;
using Zamanla.Domain.Staffs.ValueObjects;
using Zamanla.WebAPI.IntegrationTests.Infrastructure;

namespace Zamanla.WebAPI.IntegrationTests.Appointments;

[Collection(IntegrationTestCollection.Name)]
public sealed class AppointmentTests(ZamanlaApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Create_WithParallelRequestsForSameSlot_OnlyOneSucceeds()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var serviceId = await CreateServiceAsync(client, "Sac kesimi");
        var staffId = await CreateStaffAsync(client, serviceId);
        var customerId = await CreateCustomerAsync(client);
        var command = new AppointmentCreateCommand(customerId, staffId, [serviceId], NextMonday(10), null);

        // iki müşteri aynı anda aynı saate basıyormuş gibi 20 istek
        var responses = await Task.WhenAll(Enumerable.Range(0, 20)
            .Select(_ => client.PostAsJsonAsync("/api/appointments", command, CancellationToken)));

        responses.Count(r => r.StatusCode == HttpStatusCode.OK).ShouldBe(1);
        responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).ShouldBe(19);
        (await GetAppointmentsAsync(client)).ShouldHaveSingleItem();
    }

    // testin çalıştığı günden bağımsız, her zaman ileri bir pazartesi (İstanbul saati)
    private static DateTimeOffset NextMonday(int hour)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var days = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        var monday = today.AddDays(days == 0 ? 7 : days);
        return new DateTimeOffset(monday.ToDateTime(new TimeOnly(hour, 0)), TimeSpan.FromHours(3));
    }

    private async Task<List<AppointmentDto>> GetAppointmentsAsync(HttpClient client)
    {
        var response = await client.GetFromJsonAsync<ODataResponse<AppointmentDto>>("/api/odata/appointments", CancellationToken);
        return response!.Value;
    }

    private async Task<Guid> CreateServiceAsync(HttpClient client, string name)
    {
        var command = new SalonServiceCreateCommand(name, "Sac", 45, 10, 400);
        var response = await client.PostAsJsonAsync("/api/services", command, CancellationToken);
        response.EnsureSuccessStatusCode();

        var services = await client.GetFromJsonAsync<ODataResponse<SalonServiceDto>>("/api/odata/services", CancellationToken);
        return services!.Value.Single(s => s.Name == name).Id;
    }

    private async Task<Guid> CreateStaffAsync(HttpClient client, params Guid[] salonServiceIds)
    {
        var command = new StaffCreateCommand(
            "Ayse",
            "Sik",
            "peach",
            [new WorkingHour(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(18, 0))],
            [.. salonServiceIds]);
        var response = await client.PostAsJsonAsync("/api/staff", command, CancellationToken);
        response.EnsureSuccessStatusCode();

        var staff = await client.GetFromJsonAsync<ODataResponse<StaffDto>>("/api/odata/staff", CancellationToken);
        return staff!.Value.Single().Id;
    }

    private async Task<Guid> CreateCustomerAsync(HttpClient client)
    {
        var command = new CustomerCreateCommand("Elif", "Yilmaz", "0532 123 45 67", null, null);
        var response = await client.PostAsJsonAsync("/api/customers", command, CancellationToken);
        response.EnsureSuccessStatusCode();

        var customers = await client.GetFromJsonAsync<ODataResponse<CustomerDto>>("/api/odata/customers", CancellationToken);
        return customers!.Value.Single().Id;
    }
}
