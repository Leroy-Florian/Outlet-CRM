using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Organizations;
using Crm.Core.Domain.Products;
using Crm.Core.Domain.Prospects;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.Prospects;

public sealed record CreateProspectCommand(Guid ProductId, Guid? OrganizationId, string Name, string Email, string? Company);

public sealed class CreateProspect(
    IProspectRepository prospects,
    IProductRepository products,
    IOrganizationRepository organizations,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<Result<ProspectId>> HandleAsync(CreateProspectCommand command, CancellationToken cancellationToken)
    {
        var productId = new ProductId(command.ProductId);
        if (await products.GetByIdAsync(productId, cancellationToken) is null)
        {
            return Result.Failure<ProspectId>(ProductErrors.NotFound(productId));
        }

        OrganizationId? organizationId = null;
        if (command.OrganizationId is { } rawOrganizationId)
        {
            organizationId = new OrganizationId(rawOrganizationId);
            if (await organizations.GetByIdAsync(organizationId.Value, cancellationToken) is null)
            {
                return Result.Failure<ProspectId>(OrganizationErrors.NotFound(organizationId.Value));
            }
        }

        var email = Email.Create(command.Email);
        if (email.IsFailure)
        {
            return Result.Failure<ProspectId>(email.Error);
        }

        var prospect = Prospect.Create(productId, organizationId, command.Name, email.Value, command.Company, clock.UtcNow);
        if (prospect.IsFailure)
        {
            return Result.Failure<ProspectId>(prospect.Error);
        }

        await prospects.AddAsync(prospect.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(prospect.Value.Id);
    }
}
