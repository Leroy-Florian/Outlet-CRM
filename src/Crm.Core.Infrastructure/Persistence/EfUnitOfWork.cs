using Crm.Core.Application.Abstractions;

namespace Crm.Core.Infrastructure.Persistence;

public sealed class EfUnitOfWork(CrmDbContext dbContext) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
