namespace Crm.Core.Domain.Analytics;

public sealed record DownloadTrendPoint(DateTimeOffset CapturedAt, long TotalDownloads, long Delta);

public static class DownloadTrend
{
    /// <summary>Computes per-snapshot deltas; the first point has a delta of zero.</summary>
    public static IReadOnlyList<DownloadTrendPoint> FromSnapshots(IEnumerable<DownloadSnapshot> snapshots)
    {
        var ordered = snapshots.OrderBy(s => s.CapturedAt).ToList();
        List<DownloadTrendPoint> points = [];

        for (var i = 0; i < ordered.Count; i++)
        {
            var delta = i is 0 ? 0 : ordered[i].TotalDownloads - ordered[i - 1].TotalDownloads;
            points.Add(new DownloadTrendPoint(ordered[i].CapturedAt, ordered[i].TotalDownloads, delta));
        }

        return points;
    }
}
