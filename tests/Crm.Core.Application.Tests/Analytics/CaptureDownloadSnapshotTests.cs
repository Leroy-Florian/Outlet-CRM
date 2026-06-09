using Crm.Core.Application.Analytics;
using Crm.Core.Application.Tests.Fakes;
using Crm.Core.Domain.Analytics;
using Crm.Core.Domain.Products;
using Crm.Kernel.Shared;
using Xunit;

namespace Crm.Core.Application.Tests.Analytics;

public sealed class CaptureDownloadSnapshotTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 9, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeDownloadSnapshotRepository _repository = new();
    private readonly FakeProductRepository _products = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Product _product = Product.Create("Outlet", null, Now).Value;

    public CaptureDownloadSnapshotTests()
    {
        _product.TrackPackage(PackageRegistry.NuGet, PackageId.Create("outlet.cli").Value);
        _products.Items.Add(_product);
    }

    private CaptureDownloadSnapshot UseCase(Result<long> statsResult) =>
        new(new FakePackageStatsClient(statsResult), _products, _repository, _unitOfWork, new FakeClock(Now));

    [Fact]
    public async Task Should_StoreSnapshot_When_PackageIsTracked()
    {
        var result = await UseCase(Result.Success(1234L)).HandleAsync(
            new CaptureDownloadSnapshotCommand(_product.Id.Value, PackageRegistry.NuGet, "Outlet.Cli"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1234, result.Value);
        var snapshot = Assert.Single(_repository.Items);
        Assert.Equal("outlet.cli", snapshot.PackageId.Value);
        Assert.Equal(_product.Id, snapshot.ProductId);
        Assert.Equal(Now, snapshot.CapturedAt);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Should_Fail_When_PackageIsNotTracked()
    {
        var result = await UseCase(Result.Success(1L)).HandleAsync(
            new CaptureDownloadSnapshotCommand(_product.Id.Value, PackageRegistry.Npm, "outlet.cli"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Product.PackageNotTracked", result.Error.Code);
        Assert.Empty(_repository.Items);
    }

    [Fact]
    public async Task Should_Fail_When_ProductDoesNotExist()
    {
        var result = await UseCase(Result.Success(1L)).HandleAsync(
            new CaptureDownloadSnapshotCommand(Guid.NewGuid(), PackageRegistry.NuGet, "outlet.cli"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Product.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Should_PropagateError_When_RegistryFails()
    {
        var error = new Error("NuGetStats.HttpError", "boom");

        var result = await UseCase(Result.Failure<long>(error)).HandleAsync(
            new CaptureDownloadSnapshotCommand(_product.Id.Value, PackageRegistry.NuGet, "outlet.cli"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.Empty(_repository.Items);
    }
}
