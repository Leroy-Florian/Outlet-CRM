using Crm.Core.Domain.Products;

namespace Crm.Core.Application.Abstractions;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken);

    Task AddAsync(Product product, CancellationToken cancellationToken);
}
