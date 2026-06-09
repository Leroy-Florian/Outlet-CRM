using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.ApiMetrics;
using Crm.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Crm.Core.Infrastructure.Persistence.Repositories;

public sealed class ApiMetricRepository(CrmDbContext dbContext) : IApiMetricRepository
{
    public async Task<IReadOnlyList<ApiMetricSample>> ListSinceAsync(
        ProductId productId,
        DateTimeOffset since,
        CancellationToken cancellationToken) =>
        await dbContext.ApiMetricSamples
            .AsNoTracking()
            .Where(s => s.ProductId == productId && s.OccurredAt >= since)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(ApiMetricSample sample, CancellationToken cancellationToken) =>
        await dbContext.ApiMetricSamples.AddAsync(sample, cancellationToken);
}
