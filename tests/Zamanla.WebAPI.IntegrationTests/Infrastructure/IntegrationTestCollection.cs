namespace Zamanla.WebAPI.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<ZamanlaApiFactory>
{
    public const string Name = "Integration";
}
