using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Crm.Core.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(CrmDbContext dbContext) : IProductRepository
{
    public Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken) =>
        dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync(cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken) =>
        await dbContext.Products.AddAsync(product, cancellationToken);
}
