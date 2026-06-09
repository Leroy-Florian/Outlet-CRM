using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Products;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.Products;

public sealed record TrackRepositoryCommand(Guid ProductId, string Repository);

public sealed class TrackRepository(IProductRepository products, IUnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(TrackRepositoryCommand command, CancellationToken cancellationToken)
    {
        var productId = new ProductId(command.ProductId);
        var product = await products.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(productId));
        }

        var repository = RepositoryName.Create(command.Repository);
        if (repository.IsFailure)
        {
            return Result.Failure(repository.Error);
        }

        var tracked = product.TrackRepository(repository.Value);
        if (tracked.IsFailure)
        {
            return tracked;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
