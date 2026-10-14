using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TS.Result;
using Zamanla.Application.Customers;
using Zamanla.Domain.Customers;
using Zamanla.WebAPI.IntegrationTests.Infrastructure;

namespace Zamanla.WebAPI.IntegrationTests.Customers;

[Collection(IntegrationTestCollection.Name)]
public sealed class CustomerTests(ZamanlaApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Create_WithValidData_AppearsInList()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");

        var command = NewCustomer("Elif") with { Email = "  Elif.Yilmaz@Gmail.com " };
        var response = await client.PostAsJsonAsync("/api/customers", command, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var customer = (await GetCustomersAsync(client)).ShouldHaveSingleItem();
        customer.FullName.ShouldBe("Elif Yilmaz");
        customer.PhoneNumber.ShouldBe("+905321234567");
        customer.Email.ShouldBe("elif.yilmaz@gmail.com");
        customer.Note.ShouldBe("Boya 6/1");
        customer.CreatedFullName.ShouldBe("Test Kullanıcı");
    }

    [Fact]
    public async Task Create_WithSameNameAndPhone_ReturnsConflict()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        await CreateCustomerAsync(client, NewCustomer("Elif"));

        // aynı numara farklı yazılmış
        var command = NewCustomer("Elif") with { PhoneNumber = "+90 532 123 4567" };
        var response = await client.PostAsJsonAsync("/api/customers", command, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_WithSamePhoneDifferentName_AppearsInPhoneSearch()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        await CreateCustomerAsync(client, NewCustomer("Elif"));

        var response = await client.PostAsJsonAsync("/api/customers", NewCustomer("Ayse"), CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var found = await client.GetFromJsonAsync<ODataResponse<CustomerDto>>(
            "/api/odata/customers?$filter=contains(phoneNumber,'5321234')",
            CancellationToken);
        found!.Value.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Create_WithInvalidPhone_ReturnsUnprocessableEntity()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");

        var command = NewCustomer("Elif") with { PhoneNumber = "123" };
        var response = await client.PostAsJsonAsync("/api/customers", command, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Update_WithEmptyEmail_ClearsEmail()
    {
        using var client = await CreateOwnerClientAsync("salon-a", "a@test.com");
        var id = await CreateCustomerAsync(client, NewCustomer("Elif") with { Email = "elif@test.com" });

        var command = new CustomerUpdateCommand(id, "Elif", "Kaya", "0532 765 43 21", "", "Yeni not");
        var response = await client.PutAsJsonAsync("/api/customers", command, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await client.GetFromJsonAsync<Result<CustomerDto>>($"/api/customers/{id}", CancellationToken);
        var customer = result!.Data!;
        customer.LastName.ShouldBe("Kaya");
        customer.PhoneNumber.ShouldBe("+905327654321");
        customer.Email.ShouldBeNull();
        customer.Note.ShouldBe("Yeni not");
        customer.UpdatedFullName.ShouldBe("Test Kullanıcı");
    }

    [Fact]
    public async Task Delete_AnonymizesCustomer()
    {
        var tokens = await RegisterAsync("salon-a", "a@test.com");
        var owner = await GetMeAsync(tokens.AccessToken);
        using var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", tokens.AccessToken);
        var id = await CreateCustomerAsync(client, NewCustomer("Elif") with { Email = "elif@test.com" });

        var response = await client.DeleteAsync($"/api/customers/{id}", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await GetCustomersAsync(client)).ShouldBeEmpty();
        (await client.GetAsync($"/api/customers/{id}", CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // satır silinmemiş, yalnızca kişisel bilgiler temizlenmiş olmalı 
        using var scope = CreateScopeAs(owner);
        var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
        var customer = await repository.FirstOrDefaultAsync(c => c.Id == id, CancellationToken);
        customer.ShouldNotBeNull();
        customer.IsAnonymized.ShouldBeTrue();
        customer.FullName.ShouldBe("Silinmiş Müşteri");
        customer.PhoneNumber.ShouldBeNull();
        customer.Email.ShouldBeNull();
        customer.Note.ShouldBeNull();
    }

    [Fact]
    public async Task OtherTenantCustomer_IsNotAccessible()
    {
        using var ownerA = await CreateOwnerClientAsync("salon-a", "a@test.com");
        using var ownerB = await CreateOwnerClientAsync("salon-b", "b@test.com");
        var id = await CreateCustomerAsync(ownerA, NewCustomer("Elif"));

        var get = await ownerB.GetAsync($"/api/customers/{id}", CancellationToken);
        var update = await ownerB.PutAsJsonAsync(
            "/api/customers",
            new CustomerUpdateCommand(id, "Ele", "Gecirildi", "0532 000 00 00", null, null),
            CancellationToken);
        var delete = await ownerB.DeleteAsync($"/api/customers/{id}", CancellationToken);

        get.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        update.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        delete.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await GetCustomersAsync(ownerB)).ShouldBeEmpty();
        (await GetCustomersAsync(ownerA)).ShouldHaveSingleItem().FirstName.ShouldBe("Elif");

        // kopya kontrolü salon bazında: B aynı ad ve telefonla kendi müşterisini açabilir
        var sameCustomer = await ownerB.PostAsJsonAsync("/api/customers", NewCustomer("Elif"), CancellationToken);
        sameCustomer.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private static CustomerCreateCommand NewCustomer(string firstName) =>
        new(firstName, "Yilmaz", "0532 123 45 67", null, "Boya 6/1");

    private async Task<List<CustomerDto>> GetCustomersAsync(HttpClient client)
    {
        var response = await client.GetFromJsonAsync<ODataResponse<CustomerDto>>("/api/odata/customers", CancellationToken);
        return response!.Value;
    }

    private async Task<Guid> CreateCustomerAsync(HttpClient client, CustomerCreateCommand command)
    {
        var response = await client.PostAsJsonAsync("/api/customers", command, CancellationToken);
        response.EnsureSuccessStatusCode();
        return (await GetCustomersAsync(client)).Single(c => c.FirstName == command.FirstName).Id;
    }
}
