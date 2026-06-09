namespace Crm.Kernel.Shared;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
