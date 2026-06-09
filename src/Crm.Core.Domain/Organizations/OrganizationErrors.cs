using Crm.Kernel.Shared;

namespace Crm.Core.Domain.Organizations;

public static class OrganizationErrors
{
    public static readonly Error NameRequired =
        Error.Validation("Organization.NameRequired", "An organization requires a non-empty name.");

    public static Error NotFound(OrganizationId id) =>
        Error.NotFound("Organization.NotFound", $"Organization '{id.Value}' was not found.");
}
