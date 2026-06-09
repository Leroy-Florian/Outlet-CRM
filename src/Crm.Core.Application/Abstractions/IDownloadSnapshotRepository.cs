using Crm.Core.Domain.Analytics;

namespace Crm.Core.Application.Abstractions;

public interface IDownloadSnapshotRepository
{
    Task<IReadOnlyList<DownloadSnapshot>> ListByPackageAsync(PackageId packageId, CancellationToken cancellationToken);

    Task AddAsync(DownloadSnapshot snapshot, CancellationToken cancellationToken);
}
