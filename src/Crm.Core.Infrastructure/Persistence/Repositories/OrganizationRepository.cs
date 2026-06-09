using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Organizations;
using Microsoft.EntityFrameworkCore;

namespace Crm.Core.Infrastructure.Persistence.Repositories;

public sealed class OrganizationRepository(CrmDbContext dbContext) : IOrganizationRepository
{
    public Task<Organization?> GetByIdAsync(OrganizationId id, CancellationToken cancellationToken) =>
        dbContext.Organizations.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Organization>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Organizations.AsNoTracking().OrderBy(o => o.Name).ToListAsync(cancellationToken);

    public async Task AddAsync(Organization organization, CancellationToken cancellationToken) =>
        await dbContext.Organizations.AddAsync(organization, cancellationToken);
}
