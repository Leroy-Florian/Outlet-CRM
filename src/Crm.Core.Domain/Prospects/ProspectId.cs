namespace Crm.Core.Domain.Prospects;

public readonly record struct ProspectId(Guid Value)
{
    public static ProspectId New() => new(Guid.NewGuid());
}
