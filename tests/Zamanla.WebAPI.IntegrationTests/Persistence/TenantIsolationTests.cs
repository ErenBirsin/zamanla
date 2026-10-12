using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Users;
using Zamanla.WebAPI.IntegrationTests.Infrastructure;

namespace Zamanla.WebAPI.IntegrationTests.Persistence;

[Collection(IntegrationTestCollection.Name)]
public sealed class TenantIsolationTests(ZamanlaApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Query_ReturnsOnlyCurrentTenantUsers()
    {
        var ownerA = await RegisterAndGetUserAsync("salon-a", "a@test.com");
        await RegisterAndGetUserAsync("salon-b", "b@test.com");

        using var scope = CreateScopeAs(ownerA);
        var repository = scope.ServiceProvider.GetRequiredService<IAppUserRepository>();

        var users = await repository.GetAll().ToListAsync(CancellationToken);

        users.ShouldHaveSingleItem().Id.ShouldBe(ownerA.Id);
    }

    [Fact]
    public async Task SaveChanges_WhenEntityBelongsToAnotherTenant_Throws()
    {
        var ownerA = await RegisterAndGetUserAsync("salon-a", "a@test.com");
        var ownerB = await RegisterAndGetUserAsync("salon-b", "b@test.com");

        using var scope = CreateScopeAs(ownerA);
        var repository = scope.ServiceProvider.GetRequiredService<IAppUserRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // filtreyi bilerek atlayıp B'nin kullanıcısını değiştirmeyi deniyoruz
        var userB = await repository.GetByIdIgnoringTenantAsync(ownerB.Id, CancellationToken);
        userB!.SetFirstName(new FirstName("Değişti"));
        repository.Update(userB);

        await Should.ThrowAsync<InvalidOperationException>(() => unitOfWork.SaveChangesAsync(CancellationToken));
    }
}
