using Crm.Core.Domain.Analytics;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.Abstractions;

/// <summary>Port over the NuGet search API; the HTTP adapter lives in Infrastructure.</summary>
public interface INuGetStatsClient
{
    Task<Result<long>> GetTotalDownloadsAsync(PackageId packageId, CancellationToken cancellationToken);
}
