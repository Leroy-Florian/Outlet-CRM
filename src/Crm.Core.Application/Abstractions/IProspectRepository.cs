using Crm.Core.Domain.Prospects;

namespace Crm.Core.Application.Abstractions;

public interface IProspectRepository
{
    Task<Prospect?> GetByIdAsync(ProspectId id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Prospect>> ListAsync(CancellationToken cancellationToken);

    Task AddAsync(Prospect prospect, CancellationToken cancellationToken);
}
