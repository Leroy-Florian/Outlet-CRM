using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Analytics;
using Crm.Core.Domain.ApiMetrics;
using Crm.Core.Domain.Payments;
using Crm.Core.Domain.Prospects;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.Tests.Fakes;

public sealed class FakeClock(DateTimeOffset utcNow) : IClock
{
    public DateTimeOffset UtcNow { get; } = utcNow;
}

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}

public sealed class FakeProspectRepository : IProspectRepository
{
    public List<Prospect> Items { get; } = [];

    public Task<Prospect?> GetByIdAsync(ProspectId id, CancellationToken cancellationToken) =>
        Task.FromResult(Items.FirstOrDefault(p => p.Id == id));

    public Task<IReadOnlyList<Prospect>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Prospect>>(Items);

    public Task AddAsync(Prospect prospect, CancellationToken cancellationToken)
    {
        Items.Add(prospect);
        return Task.CompletedTask;
    }
}

public sealed class FakeDownloadSnapshotRepository : IDownloadSnapshotRepository
{
    public List<DownloadSnapshot> Items { get; } = [];

    public Task<IReadOnlyList<DownloadSnapshot>> ListByPackageAsync(PackageId packageId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<DownloadSnapshot>>([.. Items.Where(s => s.PackageId == packageId)]);

    public Task AddAsync(DownloadSnapshot snapshot, CancellationToken cancellationToken)
    {
        Items.Add(snapshot);
        return Task.CompletedTask;
    }
}

public sealed class FakeNuGetStatsClient(Result<long> result) : INuGetStatsClient
{
    public Task<Result<long>> GetTotalDownloadsAsync(PackageId packageId, CancellationToken cancellationToken) =>
        Task.FromResult(result);
}

public sealed class FakeApiMetricRepository : IApiMetricRepository
{
    public List<ApiMetricSample> Items { get; } = [];

    public Task<IReadOnlyList<ApiMetricSample>> ListSinceAsync(DateTimeOffset since, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ApiMetricSample>>([.. Items.Where(s => s.OccurredAt >= since)]);

    public Task AddAsync(ApiMetricSample sample, CancellationToken cancellationToken)
    {
        Items.Add(sample);
        return Task.CompletedTask;
    }
}

public sealed class FakePaymentRepository : IPaymentRepository
{
    public List<Payment> Items { get; } = [];

    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Items.FirstOrDefault(p => p.Id == id));

    public Task<IReadOnlyList<Payment>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Payment>>(Items);

    public Task AddAsync(Payment payment, CancellationToken cancellationToken)
    {
        Items.Add(payment);
        return Task.CompletedTask;
    }
}
