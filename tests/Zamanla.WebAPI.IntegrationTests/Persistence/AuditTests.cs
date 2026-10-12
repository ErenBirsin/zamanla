using GenericRepository;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Users;
using Zamanla.WebAPI.IntegrationTests.Infrastructure;

namespace Zamanla.WebAPI.IntegrationTests.Persistence;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuditTests(ZamanlaApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task SaveChanges_WhenEntityModified_SetsUpdatedFields()
    {
        var owner = await RegisterAndGetUserAsync("salon-a", "a@test.com");

        using var scope = CreateScopeAs(owner);
        var repository = scope.ServiceProvider.GetRequiredService<IAppUserRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var user = await repository.FirstOrDefaultAsync(u => u.Id == owner.Id, CancellationToken);
        user.SetFirstName(new FirstName("Yeni"));
        repository.Update(user);
        await unitOfWork.SaveChangesAsync(CancellationToken);

        user.UpdatedBy.ShouldBe(owner.Id);
        user.UpdatedAt.ShouldNotBeNull();
    }
}
