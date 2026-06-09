using Crm.Core.Application.ApiMetrics;
using Crm.Core.Application.Tests.Fakes;
using Crm.Core.Domain.Products;
using Xunit;

namespace Crm.Core.Application.Tests.ApiMetrics;

public sealed class RecordApiMetricTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 9, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeApiMetricRepository _repository = new();
    private readonly FakeProductRepository _products = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Product _product = Product.Create("Outlet", null, Now).Value;

    public RecordApiMetricTests() => _products.Items.Add(_product);

    private RecordApiMetric UseCase => new(_repository, _products, _unitOfWork, new FakeClock(Now));

    [Fact]
    public async Task Should_StoreSample_When_CommandIsValid()
    {
        var result = await UseCase.HandleAsync(
            new RecordApiMetricCommand(_product.Id.Value, "/api/items", 200, 12.5), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var sample = Assert.Single(_repository.Items);
        Assert.Equal(Now, sample.OccurredAt);
        Assert.Equal(_product.Id, sample.ProductId);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Should_Fail_When_ProductDoesNotExist()
    {
        var result = await UseCase.HandleAsync(
            new RecordApiMetricCommand(Guid.NewGuid(), "/api/items", 200, 1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Product.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Should_Fail_When_DurationIsNegative()
    {
        var result = await UseCase.HandleAsync(
            new RecordApiMetricCommand(_product.Id.Value, "/api/items", 200, -1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Empty(_repository.Items);
    }

    [Fact]
    public async Task Should_ComputeStatisticsForProductWindow_When_Queried()
    {
        await UseCase.HandleAsync(new RecordApiMetricCommand(_product.Id.Value, "/api/items", 200, 10), CancellationToken.None);
        await UseCase.HandleAsync(new RecordApiMetricCommand(_product.Id.Value, "/api/items", 500, 30), CancellationToken.None);
        var query = new GetEndpointStatistics(_repository);

        var result = await query.HandleAsync(
            new GetEndpointStatisticsQuery(_product.Id.Value, Now.AddHours(-1)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stats = Assert.Single(result.Value);
        Assert.Equal(2, stats.RequestCount);
        Assert.Equal(1, stats.ErrorCount);
    }
}
