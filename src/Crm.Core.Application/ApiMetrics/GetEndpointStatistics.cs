using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.ApiMetrics;
using Crm.Core.Domain.Products;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.ApiMetrics;

public sealed record GetEndpointStatisticsQuery(Guid ProductId, DateTimeOffset Since);

public sealed class GetEndpointStatistics(IApiMetricRepository metrics)
{
    public async Task<Result<IReadOnlyList<EndpointStatistics>>> HandleAsync(
        GetEndpointStatisticsQuery query,
        CancellationToken cancellationToken)
    {
        var samples = await metrics.ListSinceAsync(new ProductId(query.ProductId), query.Since, cancellationToken);
        return Result.Success(EndpointStatisticsCalculator.Compute(samples));
    }
}
