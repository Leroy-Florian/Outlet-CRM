using Crm.Core.Domain.Analytics;
using Crm.Core.Domain.Products;
using Xunit;

namespace Crm.Core.Domain.Tests.Analytics;

public sealed class DownloadTrendTests
{
    private static readonly DateTimeOffset Day1 = new(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

    private static readonly ProductId Product = ProductId.New();

    private static DownloadSnapshot Snapshot(long total, DateTimeOffset at) =>
        DownloadSnapshot.Create(Product, PackageRegistry.NuGet, PackageId.Create("outlet.cli").Value, total, at).Value;

    [Fact]
    public void Should_ReturnEmpty_When_NoSnapshots()
    {
        var points = DownloadTrend.FromSnapshots([]);

        Assert.Empty(points);
    }

    [Fact]
    public void Should_ComputeDeltas_When_SnapshotsAreOutOfOrder()
    {
        var points = DownloadTrend.FromSnapshots(
        [
            Snapshot(150, Day1.AddDays(2)),
            Snapshot(100, Day1),
            Snapshot(120, Day1.AddDays(1)),
        ]);

        Assert.Equal([0, 20, 30], points.Select(p => p.Delta));
        Assert.Equal([100, 120, 150], points.Select(p => p.TotalDownloads));
    }

    [Fact]
    public void Should_RejectNegativeCount_When_CreatingSnapshot()
    {
        var result = DownloadSnapshot.Create(Product, PackageRegistry.NuGet, PackageId.Create("outlet.cli").Value, -1, Day1);

        Assert.True(result.IsFailure);
        Assert.Equal("DownloadSnapshot.NegativeCount", result.Error.Code);
    }

    [Fact]
    public void Should_NormalizePackageId_When_Created()
    {
        var result = PackageId.Create("  Outlet.CLI ");

        Assert.True(result.IsSuccess);
        Assert.Equal("outlet.cli", result.Value.Value);
    }

    [Fact]
    public void Should_RejectEmptyPackageId_When_Created()
    {
        var result = PackageId.Create("   ");

        Assert.True(result.IsFailure);
        Assert.Equal("PackageId.Empty", result.Error.Code);
    }
}
