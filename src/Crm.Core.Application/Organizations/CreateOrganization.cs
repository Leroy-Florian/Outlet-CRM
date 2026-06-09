using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Organizations;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.Organizations;

public sealed record CreateOrganizationCommand(string Name, string? Website);

public sealed class CreateOrganization(IOrganizationRepository organizations, IUnitOfWork unitOfWork, IClock clock)
{
    public async Task<Result<OrganizationId>> HandleAsync(CreateOrganizationCommand command, CancellationToken cancellationToken)
    {
        var organization = Organization.Create(command.Name, command.Website, clock.UtcNow);
        if (organization.IsFailure)
        {
            return Result.Failure<OrganizationId>(organization.Error);
        }

        await organizations.AddAsync(organization.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(organization.Value.Id);
    }
}
