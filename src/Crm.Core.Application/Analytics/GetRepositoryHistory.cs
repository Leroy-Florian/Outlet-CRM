using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Analytics;
using Crm.Core.Domain.Products;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.Analytics;

public sealed record GetRepositoryHistoryQuery(Guid ProductId, string Repository);

public sealed class GetRepositoryHistory(IRepositorySnapshotRepository snapshots)
{
    public async Task<Result<IReadOnlyList<RepositorySnapshot>>> HandleAsync(
        GetRepositoryHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var repository = RepositoryName.Create(query.Repository);
        if (repository.IsFailure)
        {
            return Result.Failure<IReadOnlyList<RepositorySnapshot>>(repository.Error);
        }

        var history = await snapshots.ListByRepositoryAsync(
            new ProductId(query.ProductId), repository.Value, cancellationToken);

        return Result.Success(history);
    }
}
