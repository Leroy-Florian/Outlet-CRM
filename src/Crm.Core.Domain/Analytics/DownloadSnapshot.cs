using Crm.Kernel.Shared;

namespace Crm.Core.Domain.Analytics;

public sealed class DownloadSnapshot : AggregateRoot<Guid>
{
    private DownloadSnapshot(Guid id, PackageId packageId, long totalDownloads, DateTimeOffset capturedAt)
        : base(id)
    {
        PackageId = packageId;
        TotalDownloads = totalDownloads;
        CapturedAt = capturedAt;
    }

    public PackageId PackageId { get; }

    public long TotalDownloads { get; }

    public DateTimeOffset CapturedAt { get; }

    public static Result<DownloadSnapshot> Create(PackageId packageId, long totalDownloads, DateTimeOffset capturedAt)
    {
        if (totalDownloads < 0)
        {
            return Result.Failure<DownloadSnapshot>(
                Error.Validation("DownloadSnapshot.NegativeCount", "A download count cannot be negative."));
        }

        return Result.Success(new DownloadSnapshot(Guid.NewGuid(), packageId, totalDownloads, capturedAt));
    }
}
