using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Analytics;
using Crm.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Crm.Core.Infrastructure.Persistence.Repositories;

public sealed class DownloadSnapshotRepository(CrmDbContext dbContext) : IDownloadSnapshotRepository
{
    public async Task<IReadOnlyList<DownloadSnapshot>> ListByPackageAsync(
        ProductId productId,
        PackageRegistry registry,
        PackageId packageId,
        CancellationToken cancellationToken) =>
        await dbContext.DownloadSnapshots
            .AsNoTracking()
            .Where(s => s.ProductId == productId && s.Registry == registry && s.PackageId.Value == packageId.Value)
            .OrderBy(s => s.CapturedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(DownloadSnapshot snapshot, CancellationToken cancellationToken) =>
        await dbContext.DownloadSnapshots.AddAsync(snapshot, cancellationToken);
}
