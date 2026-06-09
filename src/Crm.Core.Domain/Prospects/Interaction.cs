using Crm.Kernel.Shared;

namespace Crm.Core.Domain.Prospects;

public sealed class Interaction(Guid id, string channel, string notes, DateTimeOffset occurredAt) : Entity<Guid>(id)
{
    public string Channel { get; } = channel;

    public string Notes { get; } = notes;

    public DateTimeOffset OccurredAt { get; } = occurredAt;
}
