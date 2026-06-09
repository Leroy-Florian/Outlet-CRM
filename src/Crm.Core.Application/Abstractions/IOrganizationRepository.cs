using Crm.Core.Domain.Organizations;

namespace Crm.Core.Application.Abstractions;

public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(OrganizationId id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Organization>> ListAsync(CancellationToken cancellationToken);

    Task AddAsync(Organization organization, CancellationToken cancellationToken);
}
