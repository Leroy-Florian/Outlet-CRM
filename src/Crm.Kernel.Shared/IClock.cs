namespace Crm.Kernel.Shared;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
