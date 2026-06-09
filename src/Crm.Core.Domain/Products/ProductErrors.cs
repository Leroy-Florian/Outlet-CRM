using Crm.Core.Domain.Analytics;
using Crm.Kernel.Shared;

namespace Crm.Core.Domain.Products;

public static class ProductErrors
{
    public static readonly Error NameRequired =
        Error.Validation("Product.NameRequired", "A product requires a non-empty name.");

    public static Error NotFound(ProductId id) =>
        Error.NotFound("Product.NotFound", $"Product '{id.Value}' was not found.");

    public static Error PackageAlreadyTracked(PackageRegistry registry, PackageId packageId) =>
        Error.Conflict("Product.PackageAlreadyTracked", $"Package '{packageId.Value}' ({registry}) is already tracked.");

    public static Error PackageNotTracked(PackageRegistry registry, PackageId packageId) =>
        Error.NotFound("Product.PackageNotTracked", $"Package '{packageId.Value}' ({registry}) is not tracked by this product.");

    public static Error RepositoryAlreadyTracked(RepositoryName repository) =>
        Error.Conflict("Product.RepositoryAlreadyTracked", $"Repository '{repository.FullName}' is already tracked.");
}
