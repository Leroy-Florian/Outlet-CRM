using Crm.Core.Domain.ApiMetrics;
using Crm.Core.Domain.Products;
using Xunit;

namespace Crm.Core.Domain.Tests.ApiMetrics;

public sealed class EndpointStatisticsTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 9, 12, 0, 0, TimeSpan.Zero);

    private static readonly ProductId Product = ProductId.New();

    private static ApiMetricSample Sample(string endpoint, int status, double durationMs) =>
        ApiMetricSample.Create(Product, endpoint, status, durationMs, Now).Value;

    [Fact]
    public void Should_GroupByEndpoint_When_Computing()
    {
        var stats = EndpointStatisticsCalculator.Compute(
        [
            Sample("/api/a", 200, 10),
            Sample("/api/b", 200, 20),
            Sample("/api/a", 500, 30),
        ]);

        Assert.Equal(2, stats.Count);
        var a = stats.Single(s => s.Endpoint == "/api/a");
        Assert.Equal(2, a.RequestCount);
        Assert.Equal(1, a.ErrorCount);
        Assert.Equal(20, a.AverageDurationMs);
    }

    [Fact]
    public void Should_ComputeP95_When_ManySamples()
    {
        var samples = Enumerable.Range(1, 100).Select(i => Sample("/api/a", 200, i));

        var stats = EndpointStatisticsCalculator.Compute(samples);

        Assert.Equal(95, stats.Single().P95DurationMs);
    }

    [Fact]
    public void Should_RejectNegativeDuration_When_CreatingSample()
    {
        var result = ApiMetricSample.Create(Product, "/api/a", 200, -1, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ApiMetric.NegativeDuration", result.Error.Code);
    }

    [Fact]
    public void Should_RejectBlankEndpoint_When_CreatingSample()
    {
        var result = ApiMetricSample.Create(Product, " ", 200, 1, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ApiMetric.EndpointRequired", result.Error.Code);
    }
}
