using Crm.Core.Domain.Analytics;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.Abstractions;

/// <summary>
/// Port over package registry download statistics. One adapter per registry
/// (NuGet azuresearch, npm downloads API…) lives in Infrastructure.
/// Note : NuGet renvoie un cumul total ; npm renvoie un volume glissant
/// (30 derniers jours), la sémantique du chiffre dépend donc du registre.
/// </summary>
public interface IPackageStatsClient
{
    Task<Result<long>> GetTotalDownloadsAsync(PackageRegistry registry, PackageId packageId, CancellationToken cancellationToken);
}
