using Crm.Core.Domain.Products;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.Abstractions;

public sealed record RepoStats(int OpenIssues, int Stars, int Forks);

/// <summary>Port over the GitHub repository API; the HTTP adapter lives in Infrastructure.</summary>
public interface IRepoStatsClient
{
    Task<Result<RepoStats>> GetRepositoryStatsAsync(RepositoryName repository, CancellationToken cancellationToken);
}
