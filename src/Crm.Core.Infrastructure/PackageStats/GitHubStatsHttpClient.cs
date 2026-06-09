using System.Text.Json;
using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Products;
using Crm.Kernel.Shared;

namespace Crm.Core.Infrastructure.PackageStats;

/// <summary>Adapter over the GitHub repository API (issues ouvertes, stars, forks).</summary>
public sealed class GitHubStatsHttpClient(HttpClient httpClient) : IRepoStatsClient
{
    public async Task<Result<RepoStats>> GetRepositoryStatsAsync(RepositoryName repository, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            $"repos/{Uri.EscapeDataString(repository.Owner)}/{Uri.EscapeDataString(repository.Name)}",
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result.Failure<RepoStats>(Error.NotFound(
                "GitHubStats.RepositoryNotFound", $"Repository '{repository.FullName}' was not found on GitHub."));
        }

        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure<RepoStats>(new Error(
                "GitHubStats.HttpError",
                $"GitHub API answered {(int)response.StatusCode} for '{repository.FullName}'."));
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;

        return Result.Success(new RepoStats(
            root.GetProperty("open_issues_count").GetInt32(),
            root.GetProperty("stargazers_count").GetInt32(),
            root.GetProperty("forks_count").GetInt32()));
    }
}
