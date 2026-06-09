using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Analytics;
using Crm.Core.Domain.Products;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.Analytics;

public sealed record CaptureProductSnapshotsCommand(Guid ProductId);

public sealed record SnapshotCaptureReport(string Target, bool Succeeded, string? ErrorCode);

/// <summary>
/// Capture en une passe les snapshots de tous les packages (NuGet/npm)
/// et repositories GitHub suivis par un produit. Best-effort : une source
/// en échec n'empêche pas les autres d'être capturées.
/// </summary>
public sealed class CaptureProductSnapshots(
    IProductRepository products,
    IPackageStatsClient packageStats,
    IRepoStatsClient repoStats,
    IDownloadSnapshotRepository downloadSnapshots,
    IRepositorySnapshotRepository repositorySnapshots,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<Result<IReadOnlyList<SnapshotCaptureReport>>> HandleAsync(
        CaptureProductSnapshotsCommand command,
        CancellationToken cancellationToken)
    {
        var productId = new ProductId(command.ProductId);
        var product = await products.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<IReadOnlyList<SnapshotCaptureReport>>(ProductErrors.NotFound(productId));
        }

        List<SnapshotCaptureReport> reports = [];

        foreach (var package in product.Packages)
        {
            var downloads = await packageStats.GetTotalDownloadsAsync(package.Registry, package.PackageId, cancellationToken);
            if (downloads.IsFailure)
            {
                reports.Add(new SnapshotCaptureReport($"{package.Registry}:{package.PackageId.Value}", false, downloads.Error.Code));
                continue;
            }

            var snapshot = DownloadSnapshot.Create(productId, package.Registry, package.PackageId, downloads.Value, clock.UtcNow);
            if (snapshot.IsFailure)
            {
                reports.Add(new SnapshotCaptureReport($"{package.Registry}:{package.PackageId.Value}", false, snapshot.Error.Code));
                continue;
            }

            await downloadSnapshots.AddAsync(snapshot.Value, cancellationToken);
            reports.Add(new SnapshotCaptureReport($"{package.Registry}:{package.PackageId.Value}", true, null));
        }

        foreach (var tracked in product.Repositories)
        {
            var stats = await repoStats.GetRepositoryStatsAsync(tracked.Repository, cancellationToken);
            if (stats.IsFailure)
            {
                reports.Add(new SnapshotCaptureReport($"github:{tracked.Repository.FullName}", false, stats.Error.Code));
                continue;
            }

            var snapshot = RepositorySnapshot.Create(
                productId, tracked.Repository, stats.Value.OpenIssues, stats.Value.Stars, stats.Value.Forks, clock.UtcNow);
            if (snapshot.IsFailure)
            {
                reports.Add(new SnapshotCaptureReport($"github:{tracked.Repository.FullName}", false, snapshot.Error.Code));
                continue;
            }

            await repositorySnapshots.AddAsync(snapshot.Value, cancellationToken);
            reports.Add(new SnapshotCaptureReport($"github:{tracked.Repository.FullName}", true, null));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success<IReadOnlyList<SnapshotCaptureReport>>(reports);
    }
}
