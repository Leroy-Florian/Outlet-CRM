using Crm.Core.Domain.Analytics;
using Crm.Core.Domain.Products;

namespace Crm.Core.Application.Abstractions;

public interface IRepositorySnapshotRepository
{
    Task<IReadOnlyList<RepositorySnapshot>> ListByRepositoryAsync(
        ProductId productId,
        RepositoryName repository,
        CancellationToken cancellationToken);

    Task AddAsync(RepositorySnapshot snapshot, CancellationToken cancellationToken);
}
