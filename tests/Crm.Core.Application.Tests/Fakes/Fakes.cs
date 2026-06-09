using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Analytics;
using Crm.Core.Domain.ApiMetrics;
using Crm.Core.Domain.Organizations;
using Crm.Core.Domain.Payments;
using Crm.Core.Domain.Products;
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

public sealed class FakeProductRepository : IProductRepository
{
    public List<Product> Items { get; } = [];

    public Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken) =>
        Task.FromResult(Items.FirstOrDefault(p => p.Id == id));

    public Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Product>>(Items);

    public Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        Items.Add(product);
        return Task.CompletedTask;
    }
}

public sealed class FakeOrganizationRepository : IOrganizationRepository
{
    public List<Organization> Items { get; } = [];

    public Task<Organization?> GetByIdAsync(OrganizationId id, CancellationToken cancellationToken) =>
        Task.FromResult(Items.FirstOrDefault(o => o.Id == id));

    public Task<IReadOnlyList<Organization>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Organization>>(Items);

    public Task AddAsync(Organization organization, CancellationToken cancellationToken)
    {
        Items.Add(organization);
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

    public Task<IReadOnlyList<DownloadSnapshot>> ListByPackageAsync(
        ProductId productId,
        PackageRegistry registry,
        PackageId packageId,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<DownloadSnapshot>>(
            [.. Items.Where(s => s.ProductId == productId && s.Registry == registry && s.PackageId == packageId)]);

    public Task AddAsync(DownloadSnapshot snapshot, CancellationToken cancellationToken)
    {
        Items.Add(snapshot);
        return Task.CompletedTask;
    }
}

public sealed class FakePackageStatsClient(Result<long> result) : IPackageStatsClient
{
    public List<(PackageRegistry Registry, string PackageId)> Calls { get; } = [];

    public Task<Result<long>> GetTotalDownloadsAsync(PackageRegistry registry, PackageId packageId, CancellationToken cancellationToken)
    {
        Calls.Add((registry, packageId.Value));
        return Task.FromResult(result);
    }
}

public sealed class FakeRepoStatsClient(Result<RepoStats> result) : IRepoStatsClient
{
    public List<string> Calls { get; } = [];

    public Task<Result<RepoStats>> GetRepositoryStatsAsync(RepositoryName repository, CancellationToken cancellationToken)
    {
        Calls.Add(repository.FullName);
        return Task.FromResult(result);
    }
}

public sealed class FakeRepositorySnapshotRepository : IRepositorySnapshotRepository
{
    public List<RepositorySnapshot> Items { get; } = [];

    public Task<IReadOnlyList<RepositorySnapshot>> ListByRepositoryAsync(
        ProductId productId,
        RepositoryName repository,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<RepositorySnapshot>>(
            [.. Items.Where(s => s.ProductId == productId && s.Repository == repository)]);

    public Task AddAsync(RepositorySnapshot snapshot, CancellationToken cancellationToken)
    {
        Items.Add(snapshot);
        return Task.CompletedTask;
    }
}

public sealed class FakeApiMetricRepository : IApiMetricRepository
{
    public List<ApiMetricSample> Items { get; } = [];

    public Task<IReadOnlyList<ApiMetricSample>> ListSinceAsync(ProductId productId, DateTimeOffset since, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ApiMetricSample>>(
            [.. Items.Where(s => s.ProductId == productId && s.OccurredAt >= since)]);

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
