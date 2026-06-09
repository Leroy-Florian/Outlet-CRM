using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.ApiMetrics;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.ApiMetrics;

public sealed record RecordApiMetricCommand(string Endpoint, int StatusCode, double DurationMs);

public sealed class RecordApiMetric(IApiMetricRepository metrics, IUnitOfWork unitOfWork, IClock clock)
{
    public async Task<Result> HandleAsync(RecordApiMetricCommand command, CancellationToken cancellationToken)
    {
        var sample = ApiMetricSample.Create(command.Endpoint, command.StatusCode, command.DurationMs, clock.UtcNow);
        if (sample.IsFailure)
        {
            return sample;
        }

        await metrics.AddAsync(sample.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
