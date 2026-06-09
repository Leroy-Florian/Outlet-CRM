using Crm.Core.Domain.Analytics;
using Crm.Core.Domain.Products;
using Xunit;

namespace Crm.Core.Domain.Tests.Products;

public sealed class ProductTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 9, 12, 0, 0, TimeSpan.Zero);

    private static Product CreateProduct() => Product.Create("Accordent", "Packages npm", Now).Value;

    [Fact]
    public void Should_Fail_When_NameIsBlank()
    {
        var result = Product.Create("  ", null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NameRequired, result.Error);
    }

    [Fact]
    public void Should_TrackPackage_When_NotAlreadyTracked()
    {
        var product = CreateProduct();

        var result = product.TrackPackage(PackageRegistry.Npm, PackageId.Create("@accordent/core").Value);

        Assert.True(result.IsSuccess);
        var tracked = Assert.Single(product.Packages);
        Assert.Equal(PackageRegistry.Npm, tracked.Registry);
    }

    [Fact]
    public void Should_Fail_When_TrackingSamePackageTwice()
    {
        var product = CreateProduct();
        var packageId = PackageId.Create("@accordent/core").Value;
        product.TrackPackage(PackageRegistry.Npm, packageId);

        var result = product.TrackPackage(PackageRegistry.Npm, packageId);

        Assert.True(result.IsFailure);
        Assert.Equal("Product.PackageAlreadyTracked", result.Error.Code);
    }

    [Fact]
    public void Should_AllowSamePackageId_When_RegistriesDiffer()
    {
        var product = CreateProduct();
        var packageId = PackageId.Create("accordent").Value;
        product.TrackPackage(PackageRegistry.Npm, packageId);

        var result = product.TrackPackage(PackageRegistry.NuGet, packageId);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, product.Packages.Count);
    }

    [Fact]
    public void Should_TrackRepository_When_NotAlreadyTracked()
    {
        var product = CreateProduct();

        var result = product.TrackRepository(RepositoryName.Create("Leroy-Florian/Accordent").Value);

        Assert.True(result.IsSuccess);
        Assert.Single(product.Repositories);
    }

    [Fact]
    public void Should_Fail_When_TrackingSameRepositoryTwice()
    {
        var product = CreateProduct();
        var repository = RepositoryName.Create("Leroy-Florian/Accordent").Value;
        product.TrackRepository(repository);

        var result = product.TrackRepository(repository);

        Assert.True(result.IsFailure);
        Assert.Equal("Product.RepositoryAlreadyTracked", result.Error.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("no-slash")]
    [InlineData("/name")]
    [InlineData("owner/")]
    [InlineData("a/b/c")]
    public void Should_RejectRepositoryName_When_NotOwnerSlashName(string raw)
    {
        var result = RepositoryName.Create(raw);

        Assert.True(result.IsFailure);
        Assert.Equal("RepositoryName.Invalid", result.Error.Code);
    }

    [Fact]
    public void Should_RejectNegativeCounters_When_CreatingRepositorySnapshot()
    {
        var result = RepositorySnapshot.Create(
            ProductId.New(), RepositoryName.Create("a/b").Value, -1, 0, 0, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("RepositorySnapshot.NegativeCount", result.Error.Code);
    }
}
