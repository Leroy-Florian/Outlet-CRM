using Crm.Core.Domain.ApiMetrics;

namespace Crm.Core.Application.Abstractions;

public interface IApiMetricRepository
{
    Task<IReadOnlyList<ApiMetricSample>> ListSinceAsync(DateTimeOffset since, CancellationToken cancellationToken);

    Task AddAsync(ApiMetricSample sample, CancellationToken cancellationToken);
}
