using Crm.Core.Domain.Analytics;
using Crm.Kernel.Shared;

namespace Crm.Core.Domain.Products;

public sealed class TrackedPackage(Guid id, PackageRegistry registry, PackageId packageId) : Entity<Guid>(id)
{
    public PackageRegistry Registry { get; } = registry;

    public PackageId PackageId { get; } = packageId;
}
