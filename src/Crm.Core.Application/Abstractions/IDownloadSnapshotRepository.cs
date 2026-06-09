using Crm.Core.Domain.Analytics;
using Crm.Core.Domain.Products;

namespace Crm.Core.Application.Abstractions;

public interface IDownloadSnapshotRepository
{
    Task<IReadOnlyList<DownloadSnapshot>> ListByPackageAsync(
        ProductId productId,
        PackageRegistry registry,
        PackageId packageId,
        CancellationToken cancellationToken);

    Task AddAsync(DownloadSnapshot snapshot, CancellationToken cancellationToken);
}
