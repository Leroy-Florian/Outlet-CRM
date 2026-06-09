using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Analytics;
using Crm.Core.Domain.Products;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.Analytics;

public sealed record CaptureDownloadSnapshotCommand(Guid ProductId, PackageRegistry Registry, string PackageId);

public sealed class CaptureDownloadSnapshot(
    IPackageStatsClient packageStats,
    IProductRepository products,
    IDownloadSnapshotRepository snapshots,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<Result<long>> HandleAsync(CaptureDownloadSnapshotCommand command, CancellationToken cancellationToken)
    {
        var productId = new ProductId(command.ProductId);
        var product = await products.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<long>(ProductErrors.NotFound(productId));
        }

        var packageId = PackageId.Create(command.PackageId);
        if (packageId.IsFailure)
        {
            return Result.Failure<long>(packageId.Error);
        }

        if (!product.IsTracking(command.Registry, packageId.Value))
        {
            return Result.Failure<long>(ProductErrors.PackageNotTracked(command.Registry, packageId.Value));
        }

        var totalDownloads = await packageStats.GetTotalDownloadsAsync(command.Registry, packageId.Value, cancellationToken);
        if (totalDownloads.IsFailure)
        {
            return totalDownloads;
        }

        var snapshot = DownloadSnapshot.Create(productId, command.Registry, packageId.Value, totalDownloads.Value, clock.UtcNow);
        if (snapshot.IsFailure)
        {
            return Result.Failure<long>(snapshot.Error);
        }

        await snapshots.AddAsync(snapshot.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(totalDownloads.Value);
    }
}
