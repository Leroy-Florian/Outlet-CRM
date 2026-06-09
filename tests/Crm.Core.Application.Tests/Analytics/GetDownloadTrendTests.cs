using Crm.Core.Application.Analytics;
using Crm.Core.Application.Tests.Fakes;
using Crm.Core.Domain.Analytics;
using Xunit;

namespace Crm.Core.Application.Tests.Analytics;

public sealed class GetDownloadTrendTests
{
    private static readonly DateTimeOffset Day1 = new(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Should_ReturnTrendWithDeltas_When_HistoryExists()
    {
        var repository = new FakeDownloadSnapshotRepository();
        var packageId = PackageId.Create("outlet.cli").Value;
        repository.Items.Add(DownloadSnapshot.Create(packageId, 100, Day1).Value);
        repository.Items.Add(DownloadSnapshot.Create(packageId, 130, Day1.AddDays(1)).Value);
        var useCase = new GetDownloadTrend(repository);

        var result = await useCase.HandleAsync(new GetDownloadTrendQuery("outlet.cli"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal([0, 30], result.Value.Select(p => p.Delta));
    }

    [Fact]
    public async Task Should_Fail_When_PackageIdIsEmpty()
    {
        var useCase = new GetDownloadTrend(new FakeDownloadSnapshotRepository());

        var result = await useCase.HandleAsync(new GetDownloadTrendQuery(""), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PackageId.Empty", result.Error.Code);
    }
}
