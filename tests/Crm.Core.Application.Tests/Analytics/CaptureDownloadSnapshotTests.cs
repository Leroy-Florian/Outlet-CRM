using Crm.Core.Application.Analytics;
using Crm.Core.Application.Tests.Fakes;
using Crm.Kernel.Shared;
using Xunit;

namespace Crm.Core.Application.Tests.Analytics;

public sealed class CaptureDownloadSnapshotTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 9, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeDownloadSnapshotRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    [Fact]
    public async Task Should_StoreSnapshot_When_NuGetAnswers()
    {
        var useCase = new CaptureDownloadSnapshot(
            new FakeNuGetStatsClient(Result.Success(1234L)), _repository, _unitOfWork, new FakeClock(Now));

        var result = await useCase.HandleAsync(new CaptureDownloadSnapshotCommand("Outlet.Cli"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1234, result.Value);
        var snapshot = Assert.Single(_repository.Items);
        Assert.Equal("outlet.cli", snapshot.PackageId.Value);
        Assert.Equal(Now, snapshot.CapturedAt);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Should_PropagateError_When_NuGetFails()
    {
        var error = new Error("NuGetStats.HttpError", "boom");
        var useCase = new CaptureDownloadSnapshot(
            new FakeNuGetStatsClient(Result.Failure<long>(error)), _repository, _unitOfWork, new FakeClock(Now));

        var result = await useCase.HandleAsync(new CaptureDownloadSnapshotCommand("outlet.cli"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.Empty(_repository.Items);
    }

    [Fact]
    public async Task Should_Fail_When_PackageIdIsEmpty()
    {
        var useCase = new CaptureDownloadSnapshot(
            new FakeNuGetStatsClient(Result.Success(1L)), _repository, _unitOfWork, new FakeClock(Now));

        var result = await useCase.HandleAsync(new CaptureDownloadSnapshotCommand("  "), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PackageId.Empty", result.Error.Code);
    }
}
