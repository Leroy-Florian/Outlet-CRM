using Crm.Kernel.Shared;

namespace Crm.Core.Domain.Prospects;

public sealed class Prospect : AggregateRoot<ProspectId>
{
    private readonly List<Interaction> _interactions = [];

    private Prospect(ProspectId id, string name, Email email, string? company, DateTimeOffset createdAt)
        : base(id)
    {
        Name = name;
        Email = email;
        Company = company;
        CreatedAt = createdAt;
        Stage = ProspectStage.New;
    }

    public string Name { get; }

    public Email Email { get; }

    public string? Company { get; }

    public ProspectStage Stage { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public IReadOnlyList<Interaction> Interactions => _interactions;

    public static Result<Prospect> Create(string name, Email email, string? company, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Prospect>(ProspectErrors.NameRequired);
        }

        return Result.Success(new Prospect(ProspectId.New(), name.Trim(), email, company, createdAt));
    }

    public Result Advance(ProspectStage target)
    {
        if (Stage is ProspectStage.Won or ProspectStage.Lost)
        {
            return Result.Failure(ProspectErrors.AlreadyClosed);
        }

        if (target != ProspectStage.Lost && target <= Stage)
        {
            return Result.Failure(ProspectErrors.InvalidTransition);
        }

        Stage = target;
        return Result.Success();
    }

    public void RecordInteraction(string channel, string notes, DateTimeOffset occurredAt) =>
        _interactions.Add(new Interaction(Guid.NewGuid(), channel, notes, occurredAt));
}
