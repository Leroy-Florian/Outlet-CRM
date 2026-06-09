using Crm.Core.Domain.Products;
using Crm.Kernel.Shared;

namespace Crm.Core.Domain.ApiMetrics;

public sealed class ApiMetricSample : AggregateRoot<Guid>
{
    private ApiMetricSample(Guid id, ProductId productId, string endpoint, int statusCode, double durationMs, DateTimeOffset occurredAt)
        : base(id)
    {
        ProductId = productId;
        Endpoint = endpoint;
        StatusCode = statusCode;
        DurationMs = durationMs;
        OccurredAt = occurredAt;
    }

    public ProductId ProductId { get; }

    public string Endpoint { get; }

    public int StatusCode { get; }

    public double DurationMs { get; }

    public DateTimeOffset OccurredAt { get; }

    public static Result<ApiMetricSample> Create(
        ProductId productId,
        string endpoint,
        int statusCode,
        double durationMs,
        DateTimeOffset occurredAt)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return Result.Failure<ApiMetricSample>(
                Error.Validation("ApiMetric.EndpointRequired", "An endpoint is required."));
        }

        if (durationMs < 0)
        {
            return Result.Failure<ApiMetricSample>(
                Error.Validation("ApiMetric.NegativeDuration", "A duration cannot be negative."));
        }

        return Result.Success(new ApiMetricSample(Guid.NewGuid(), productId, endpoint.Trim(), statusCode, durationMs, occurredAt));
    }
}
