using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Prospects;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.Prospects;

public sealed record AdvanceProspectStageCommand(ProspectId ProspectId, ProspectStage Target);

public sealed class AdvanceProspectStage(IProspectRepository prospects, IUnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(AdvanceProspectStageCommand command, CancellationToken cancellationToken)
    {
        var prospect = await prospects.GetByIdAsync(command.ProspectId, cancellationToken);
        if (prospect is null)
        {
            return Result.Failure(ProspectErrors.NotFound(command.ProspectId));
        }

        var advanced = prospect.Advance(command.Target);
        if (advanced.IsFailure)
        {
            return advanced;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
