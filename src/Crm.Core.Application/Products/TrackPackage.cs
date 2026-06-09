using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Analytics;
using Crm.Core.Domain.Products;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.Products;

public sealed record TrackPackageCommand(Guid ProductId, PackageRegistry Registry, string PackageId);

public sealed class TrackPackage(IProductRepository products, IUnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(TrackPackageCommand command, CancellationToken cancellationToken)
    {
        var productId = new ProductId(command.ProductId);
        var product = await products.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(productId));
        }

        var packageId = PackageId.Create(command.PackageId);
        if (packageId.IsFailure)
        {
            return Result.Failure(packageId.Error);
        }

        var tracked = product.TrackPackage(command.Registry, packageId.Value);
        if (tracked.IsFailure)
        {
            return tracked;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
