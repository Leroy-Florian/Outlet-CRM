using Crm.Core.Application.Abstractions;
using Crm.Core.Application.Analytics;
using Crm.Core.Application.ApiMetrics;
using Crm.Core.Application.Payments;
using Crm.Core.Application.Products;
using Crm.Core.Application.Prospects;
using Crm.Core.Domain.Analytics;
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

        var products = api.MapGroup("/products");

        products.MapGet("/", async (IProductRepository repository, CancellationToken ct) =>
            Results.Ok((await repository.ListAsync(ct)).Select(p => new
            {
                id = p.Id.Value,
                p.Name,
                p.Description,
                packages = p.Packages.Select(t => new { registry = t.Registry.ToString(), packageId = t.PackageId.Value }),
                repositories = p.Repositories.Select(r => r.Repository.FullName),
                p.CreatedAt,
            })));

        products.MapPost("/", async (CreateProductCommand command, CreateProduct useCase, CancellationToken ct) =>
            ToHttp(await useCase.HandleAsync(command, ct), id => Results.Created($"/api/products/{id.Value}", new { id = id.Value })));

        products.MapPost("/{productId:guid}/packages", async (Guid productId, TrackPackageRequest request, TrackPackage useCase, CancellationToken ct) =>
            ToHttp(await useCase.HandleAsync(new TrackPackageCommand(productId, request.Registry, request.PackageId), ct)));

        products.MapPost("/{productId:guid}/repositories", async (Guid productId, TrackRepositoryRequest request, TrackRepository useCase, CancellationToken ct) =>
            ToHttp(await useCase.HandleAsync(new TrackRepositoryCommand(productId, request.Repository), ct)));

        products.MapPost("/{productId:guid}/snapshots", async (Guid productId, CaptureProductSnapshots useCase, CancellationToken ct) =>
            ToHttp(await useCase.HandleAsync(new CaptureProductSnapshotsCommand(productId), ct), Results.Ok));

        products.MapGet("/{productId:guid}/packages/{registry}/{packageId}/trend",
            async (Guid productId, PackageRegistry registry, string packageId, GetDownloadTrend useCase, CancellationToken ct) =>
                ToHttp(await useCase.HandleAsync(new GetDownloadTrendQuery(productId, registry, packageId), ct), Results.Ok));

        products.MapGet("/{productId:guid}/repositories/{owner}/{name}/history",
            async (Guid productId, string owner, string name, GetRepositoryHistory useCase, CancellationToken ct) =>
                ToHttp(await useCase.HandleAsync(new GetRepositoryHistoryQuery(productId, $"{owner}/{name}"), ct), history =>
                    Results.Ok(history.Select(s => new
                    {
                        repository = s.Repository.FullName,
                        s.OpenIssues,
                        s.Stars,
                        s.Forks,
                        s.CapturedAt,
                    }))));

        products.MapGet("/{productId:guid}/metrics/statistics",
            async (Guid productId, DateTimeOffset since, GetEndpointStatistics useCase, CancellationToken ct) =>
                ToHttp(await useCase.HandleAsync(new GetEndpointStatisticsQuery(productId, since), ct), Results.Ok));

        var prospects = api.MapGroup("/prospects");

        prospects.MapGet("/", async (IProspectRepository repository, CancellationToken ct) =>
            Results.Ok((await repository.ListAsync(ct)).Select(p => new
            {
                id = p.Id.Value,
                productId = p.ProductId.Value,
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

        var metrics = api.MapGroup("/metrics");

        metrics.MapPost("/", async (RecordApiMetricCommand command, RecordApiMetric useCase, CancellationToken ct) =>
            ToHttp(await useCase.HandleAsync(command, ct)));

        var payments = api.MapGroup("/payments");

        payments.MapGet("/", async (IPaymentRepository repository, CancellationToken ct) =>
            Results.Ok((await repository.ListAsync(ct)).Select(p => new
            {
                p.Id,
                productId = p.ProductId.Value,
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

    private sealed record TrackPackageRequest(PackageRegistry Registry, string PackageId);

    private sealed record TrackRepositoryRequest(string Repository);

    private static IResult ToHttp(Result result) =>
        result.IsSuccess ? Results.NoContent() : ToProblem(result.Error);

    private static IResult ToHttp<TValue>(Result<TValue> result, Func<TValue, IResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : ToProblem(result.Error);

    private static IResult ToProblem(Error error) =>
        Results.Problem(
            title: error.Code,
            detail: error.Message,
            statusCode: error.Code.Contains("NotFound", StringComparison.Ordinal) || error.Code.Contains("NotTracked", StringComparison.Ordinal) ? 404 : 400);
}
