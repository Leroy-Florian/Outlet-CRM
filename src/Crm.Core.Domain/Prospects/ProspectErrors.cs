using Crm.Kernel.Shared;

namespace Crm.Core.Domain.Prospects;

public static class ProspectErrors
{
    public static readonly Error NameRequired =
        Error.Validation("Prospect.NameRequired", "A prospect requires a non-empty name.");

    public static readonly Error AlreadyClosed =
        Error.Conflict("Prospect.AlreadyClosed", "A won or lost prospect cannot change stage.");

    public static readonly Error InvalidTransition =
        Error.Conflict("Prospect.InvalidTransition", "Stages can only advance forward through the pipeline.");

    public static Error NotFound(ProspectId id) =>
        Error.NotFound("Prospect.NotFound", $"Prospect '{id.Value}' was not found.");
}
