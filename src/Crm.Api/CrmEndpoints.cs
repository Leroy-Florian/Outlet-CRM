using Crm.Core.Application.Abstractions;
using Crm.Core.Application.Analytics;
using Crm.Core.Application.ApiMetrics;
using Crm.Core.Application.Payments;
using Crm.Core.Application.Prospects;
using Crm.Core.Domain.Prospects;
using Crm.Kernel.Shared;

namespace Crm.Api;

public static class CrmEndpoints
{
    public static void MapCrmEndpoints(this IEndpointRouteBuilder app, bool requireAuthorization)
    {
        var api = app.MapGroup("/api");
        if (requireAuthorization)
        {
            api.RequireAuthorization();
        }

        var prospects = api.MapGroup("/prospects");

        prospects.MapGet("/", async (IProspectRepository repository, CancellationToken ct) =>
            Results.Ok((await repository.ListAsync(ct)).Select(p => new
            {
                id = p.Id.Value,
                p.Name,
                email = p.Email.Value,
                p.Company,
                stage = p.Stage.ToString(),
                p.CreatedAt,
            })));

        prospects.MapPost("/", async (CreateProspectCommand command, CreateProspect useCase, CancellationToken ct) =>
            ToHttp(await useCase.HandleAsync(command, ct), id => Results.Created($"/api/prospects/{id.Value}", new { id = id.Value })));

        prospects.MapPost("/{id:guid}/stage", async (Guid id, ProspectStage target, AdvanceProspectStage useCase, CancellationToken ct) =>
            ToHttp(await useCase.HandleAsync(new AdvanceProspectStageCommand(new ProspectId(id), target), ct)));

        var analytics = api.MapGroup("/analytics");

        analytics.MapPost("/packages/{packageId}/snapshot", async (string packageId, CaptureDownloadSnapshot useCase, CancellationToken ct) =>
            ToHttp(await useCase.HandleAsync(new CaptureDownloadSnapshotCommand(packageId), ct), total => Results.Ok(new { totalDownloads = total })));

        analytics.MapGet("/packages/{packageId}/trend", async (string packageId, GetDownloadTrend useCase, CancellationToken ct) =>
            ToHttp(await useCase.HandleAsync(new GetDownloadTrendQuery(packageId), ct), Results.Ok));

        var metrics = api.MapGroup("/metrics");

        metrics.MapPost("/", async (RecordApiMetricCommand command, RecordApiMetric useCase, CancellationToken ct) =>
            ToHttp(await useCase.HandleAsync(command, ct)));

        metrics.MapGet("/statistics", async (DateTimeOffset since, GetEndpointStatistics useCase, CancellationToken ct) =>
            ToHttp(await useCase.HandleAsync(new GetEndpointStatisticsQuery(since), ct), Results.Ok));

        var payments = api.MapGroup("/payments");

        payments.MapGet("/", async (IPaymentRepository repository, CancellationToken ct) =>
            Results.Ok((await repository.ListAsync(ct)).Select(p => new
            {
                p.Id,
                amount = p.Amount.Amount,
                currency = p.Amount.Currency,
                p.Source,
                p.ExternalReference,
                status = p.Status.ToString(),
                p.CreatedAt,
            })));

        payments.MapPost("/", async (RecordPaymentCommand command, RecordPayment useCase, CancellationToken ct) =>
            ToHttp(await useCase.HandleAsync(command, ct), id => Results.Created($"/api/payments/{id}", new { id })));

        payments.MapPost("/{id:guid}/settle", async (Guid id, SettlePayment useCase, CancellationToken ct) =>
            ToHttp(await useCase.HandleAsync(new SettlePaymentCommand(id), ct)));
    }

    private static IResult ToHttp(Result result) =>
        result.IsSuccess ? Results.NoContent() : ToProblem(result.Error);

    private static IResult ToHttp<TValue>(Result<TValue> result, Func<TValue, IResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : ToProblem(result.Error);

    private static IResult ToProblem(Error error) =>
        Results.Problem(title: error.Code, detail: error.Message, statusCode: error.Code.EndsWith(".NotFound", StringComparison.Ordinal) ? 404 : 400);
}
