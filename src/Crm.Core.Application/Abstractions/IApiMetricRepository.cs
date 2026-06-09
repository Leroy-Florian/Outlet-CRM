using Crm.Core.Domain.ApiMetrics;
using Crm.Core.Domain.Products;

namespace Crm.Core.Application.Abstractions;

public interface IApiMetricRepository
{
    Task<IReadOnlyList<ApiMetricSample>> ListSinceAsync(ProductId productId, DateTimeOffset since, CancellationToken cancellationToken);

    Task AddAsync(ApiMetricSample sample, CancellationToken cancellationToken);
}
