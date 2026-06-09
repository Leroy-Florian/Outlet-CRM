using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Products;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.Products;

public sealed record CreateProductCommand(string Name, string? Description);

public sealed class CreateProduct(IProductRepository products, IUnitOfWork unitOfWork, IClock clock)
{
    public async Task<Result<ProductId>> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = Product.Create(command.Name, command.Description, clock.UtcNow);
        if (product.IsFailure)
        {
            return Result.Failure<ProductId>(product.Error);
        }

        await products.AddAsync(product.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(product.Value.Id);
    }
}
