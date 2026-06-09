using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Analytics;
using Crm.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Crm.Core.Infrastructure.Persistence.Repositories;

public sealed class RepositorySnapshotRepository(CrmDbContext dbContext) : IRepositorySnapshotRepository
{
    public async Task<IReadOnlyList<RepositorySnapshot>> ListByRepositoryAsync(
        ProductId productId,
        RepositoryName repository,
        CancellationToken cancellationToken) =>
        await dbContext.RepositorySnapshots
            .AsNoTracking()
            .Where(s => s.ProductId == productId
                && s.Repository.Owner == repository.Owner
                && s.Repository.Name == repository.Name)
            .OrderBy(s => s.CapturedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(RepositorySnapshot snapshot, CancellationToken cancellationToken) =>
        await dbContext.RepositorySnapshots.AddAsync(snapshot, cancellationToken);
}
