using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Analytics;
using Crm.Kernel.Shared;

namespace Crm.Core.Infrastructure.PackageStats;

/// <summary>Routes stats requests to the adapter matching the registry.</summary>
public sealed class PackageStatsClient(NuGetStatsHttpClient nuGet, NpmStatsHttpClient npm) : IPackageStatsClient
{
    public Task<Result<long>> GetTotalDownloadsAsync(PackageRegistry registry, PackageId packageId, CancellationToken cancellationToken) =>
        registry switch
        {
            PackageRegistry.NuGet => nuGet.GetTotalDownloadsAsync(packageId, cancellationToken),
            PackageRegistry.Npm => npm.GetTotalDownloadsAsync(packageId, cancellationToken),
            _ => Task.FromResult(Result.Failure<long>(Error.Validation(
                "PackageStats.UnknownRegistry", $"Unknown package registry '{registry}'."))),
        };
}
