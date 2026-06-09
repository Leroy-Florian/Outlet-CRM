using Crm.Kernel.Shared;

namespace Crm.Core.Domain.Products;

public sealed class TrackedRepository(Guid id, RepositoryName repository) : Entity<Guid>(id)
{
    public RepositoryName Repository { get; } = repository;
}
