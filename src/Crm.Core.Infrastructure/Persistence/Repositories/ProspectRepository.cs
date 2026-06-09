using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Prospects;
using Microsoft.EntityFrameworkCore;

namespace Crm.Core.Infrastructure.Persistence.Repositories;

public sealed class ProspectRepository(CrmDbContext dbContext) : IProspectRepository
{
    public Task<Prospect?> GetByIdAsync(ProspectId id, CancellationToken cancellationToken) =>
        dbContext.Prospects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Prospect>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Prospects.AsNoTracking().OrderByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);

    public async Task AddAsync(Prospect prospect, CancellationToken cancellationToken) =>
        await dbContext.Prospects.AddAsync(prospect, cancellationToken);
}
