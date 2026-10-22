using System.Reflection;
using Shouldly;
using Zamanla.Application.Behaviors;
using Zamanla.Application.Permissions;

namespace Zamanla.Application.Tests.Permissions;

public sealed class PermissionCatalogTests
{
    [Fact]
    public void EveryPermissionAttribute_UsesCatalogValue()
    {
        var used = typeof(PermissionCatalog).Assembly.GetTypes()
            .Select(t => t.GetCustomAttribute<PermissionAttribute>()?.Permission)
            .OfType<string>()
            .Distinct()
            .ToList();

        used.ShouldNotBeEmpty();
        used.ShouldAllBe(p => PermissionCatalog.Exists(p));
    }

    [Fact]
    public void Catalog_HasUniqueValuesAndLabels()
    {
        PermissionCatalog.All.Select(p => p.Value).ShouldBeUnique();
        PermissionCatalog.All.ShouldAllBe(p => !string.IsNullOrWhiteSpace(p.Label));
    }
}
