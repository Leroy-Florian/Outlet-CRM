using Crm.Core.Application.Analytics;
using Crm.Core.Application.Abstractions;
using Crm.Core.Application.Products;
using Crm.Core.Application.Tests.Fakes;
using Crm.Core.Domain.Analytics;
using Crm.Core.Domain.Products;
using Crm.Kernel.Shared;
using Xunit;

namespace Crm.Core.Application.Tests.Products;

public sealed class ProductUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 9, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeProductRepository _products = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    [Fact]
    public async Task Should_PersistProduct_When_CommandIsValid()
    {
        var useCase = new CreateProduct(_products, _unitOfWork, new FakeClock(Now));

        var result = await useCase.HandleAsync(new CreateProductCommand("Accordent", "7 packages npm"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var product = Assert.Single(_products.Items);
        Assert.Equal("Accordent", product.Name);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Should_TrackPackage_When_ProductExists()
    {
        var product = Product.Create("Accordent", null, Now).Value;
        _products.Items.Add(product);
        var useCase = new TrackPackage(_products, _unitOfWork);

        var result = await useCase.HandleAsync(
            new TrackPackageCommand(product.Id.Value, PackageRegistry.Npm, "@accordent/core"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(product.Packages);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Should_Fail_When_TrackingPackageOnUnknownProduct()
    {
        var useCase = new TrackPackage(_products, _unitOfWork);

        var result = await useCase.HandleAsync(
            new TrackPackageCommand(Guid.NewGuid(), PackageRegistry.Npm, "@accordent/core"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Product.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Should_TrackRepository_When_ProductExists()
    {
        var product = Product.Create("Outlet", null, Now).Value;
        _products.Items.Add(product);
        var useCase = new TrackRepository(_products, _unitOfWork);

        var result = await useCase.HandleAsync(
            new TrackRepositoryCommand(product.Id.Value, "Leroy-Florian/Outlet-CLI"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(product.Repositories);
    }

    [Fact]
    public async Task Should_Fail_When_RepositoryNameIsInvalid()
    {
        var product = Product.Create("Outlet", null, Now).Value;
        _products.Items.Add(product);
        var useCase = new TrackRepository(_products, _unitOfWork);

        var result = await useCase.HandleAsync(
            new TrackRepositoryCommand(product.Id.Value, "not-a-repo"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("RepositoryName.Invalid", result.Error.Code);
    }

    [Fact]
    public async Task Should_CaptureAllTrackedSources_When_ProductHasPackagesAndRepositories()
    {
        var product = Product.Create("Accordent", null, Now).Value;
        product.TrackPackage(PackageRegistry.Npm, PackageId.Create("@accordent/core").Value);
        product.TrackPackage(PackageRegistry.Npm, PackageId.Create("@accordent/react").Value);
        product.TrackRepository(RepositoryName.Create("Leroy-Florian/Accordent").Value);
        _products.Items.Add(product);

        var downloads = new FakeDownloadSnapshotRepository();
        var repos = new FakeRepositorySnapshotRepository();
        var useCase = new CaptureProductSnapshots(
            _products,
            new FakePackageStatsClient(Result.Success(500L)),
            new FakeRepoStatsClient(Result.Success(new RepoStats(3, 42, 7))),
            downloads,
            repos,
            _unitOfWork,
            new FakeClock(Now));

        var result = await useCase.HandleAsync(new CaptureProductSnapshotsCommand(product.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Count);
        Assert.All(result.Value, report => Assert.True(report.Succeeded));
        Assert.Equal(2, downloads.Items.Count);
        var repoSnapshot = Assert.Single(repos.Items);
        Assert.Equal(3, repoSnapshot.OpenIssues);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Should_ContinueCapturing_When_OneSourceFails()
    {
        var product = Product.Create("Accordent", null, Now).Value;
        product.TrackPackage(PackageRegistry.Npm, PackageId.Create("@accordent/core").Value);
        product.TrackRepository(RepositoryName.Create("Leroy-Florian/Accordent").Value);
        _products.Items.Add(product);

        var downloads = new FakeDownloadSnapshotRepository();
        var repos = new FakeRepositorySnapshotRepository();
        var useCase = new CaptureProductSnapshots(
            _products,
            new FakePackageStatsClient(Result.Failure<long>(new Error("NpmStats.HttpError", "boom"))),
            new FakeRepoStatsClient(Result.Success(new RepoStats(1, 2, 3))),
            downloads,
            repos,
            _unitOfWork,
            new FakeClock(Now));

        var result = await useCase.HandleAsync(new CaptureProductSnapshotsCommand(product.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var failed = Assert.Single(result.Value, r => !r.Succeeded);
        Assert.Equal("NpmStats.HttpError", failed.ErrorCode);
        Assert.Empty(downloads.Items);
        Assert.Single(repos.Items);
    }

    [Fact]
    public async Task Should_Fail_When_CapturingUnknownProduct()
    {
        var useCase = new CaptureProductSnapshots(
            _products,
            new FakePackageStatsClient(Result.Success(1L)),
            new FakeRepoStatsClient(Result.Success(new RepoStats(0, 0, 0))),
            new FakeDownloadSnapshotRepository(),
            new FakeRepositorySnapshotRepository(),
            _unitOfWork,
            new FakeClock(Now));

        var result = await useCase.HandleAsync(new CaptureProductSnapshotsCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Product.NotFound", result.Error.Code);
    }
}
