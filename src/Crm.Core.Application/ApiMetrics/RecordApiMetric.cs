using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.ApiMetrics;
using Crm.Core.Domain.Products;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.ApiMetrics;

public sealed record RecordApiMetricCommand(Guid ProductId, string Endpoint, int StatusCode, double DurationMs);

public sealed class RecordApiMetric(
    IApiMetricRepository metrics,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<Result> HandleAsync(RecordApiMetricCommand command, CancellationToken cancellationToken)
    {
        var productId = new ProductId(command.ProductId);
        if (await products.GetByIdAsync(productId, cancellationToken) is null)
        {
            return Result.Failure(ProductErrors.NotFound(productId));
        }

        var sample = ApiMetricSample.Create(productId, command.Endpoint, command.StatusCode, command.DurationMs, clock.UtcNow);
        if (sample.IsFailure)
        {
            return sample;
        }

        await metrics.AddAsync(sample.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
