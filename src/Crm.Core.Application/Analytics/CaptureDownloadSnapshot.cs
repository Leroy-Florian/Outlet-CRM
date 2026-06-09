using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Analytics;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.Analytics;

public sealed record CaptureDownloadSnapshotCommand(string PackageId);

public sealed class CaptureDownloadSnapshot(
    INuGetStatsClient nuGetStats,
    IDownloadSnapshotRepository snapshots,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<Result<long>> HandleAsync(CaptureDownloadSnapshotCommand command, CancellationToken cancellationToken)
    {
        var packageId = PackageId.Create(command.PackageId);
        if (packageId.IsFailure)
        {
            return Result.Failure<long>(packageId.Error);
        }

        var totalDownloads = await nuGetStats.GetTotalDownloadsAsync(packageId.Value, cancellationToken);
        if (totalDownloads.IsFailure)
        {
            return totalDownloads;
        }

        var snapshot = DownloadSnapshot.Create(packageId.Value, totalDownloads.Value, clock.UtcNow);
        if (snapshot.IsFailure)
        {
            return Result.Failure<long>(snapshot.Error);
        }

        await snapshots.AddAsync(snapshot.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(totalDownloads.Value);
    }
}
