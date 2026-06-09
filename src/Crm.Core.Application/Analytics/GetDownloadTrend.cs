using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Analytics;
using Crm.Core.Domain.Products;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.Analytics;

public sealed record GetDownloadTrendQuery(Guid ProductId, PackageRegistry Registry, string PackageId);

public sealed class GetDownloadTrend(IDownloadSnapshotRepository snapshots)
{
    public async Task<Result<IReadOnlyList<DownloadTrendPoint>>> HandleAsync(
        GetDownloadTrendQuery query,
        CancellationToken cancellationToken)
    {
        var packageId = PackageId.Create(query.PackageId);
        if (packageId.IsFailure)
        {
            return Result.Failure<IReadOnlyList<DownloadTrendPoint>>(packageId.Error);
        }

        var history = await snapshots.ListByPackageAsync(
            new ProductId(query.ProductId), query.Registry, packageId.Value, cancellationToken);

        return Result.Success(DownloadTrend.FromSnapshots(history));
    }
}
