using System.Text.Json;
using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Analytics;
using Crm.Kernel.Shared;

namespace Crm.Core.Infrastructure.NuGet;

/// <summary>Adapter over the NuGet azuresearch query API.</summary>
public sealed class NuGetStatsHttpClient(HttpClient httpClient) : INuGetStatsClient
{
    public async Task<Result<long>> GetTotalDownloadsAsync(PackageId packageId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            $"query?q=packageid:{Uri.EscapeDataString(packageId.Value)}&prerelease=true",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure<long>(new Error(
                "NuGetStats.HttpError",
                $"NuGet search API answered {(int)response.StatusCode} for '{packageId.Value}'."));
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        foreach (var item in document.RootElement.GetProperty("data").EnumerateArray())
        {
            if (string.Equals(item.GetProperty("id").GetString(), packageId.Value, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Success(item.GetProperty("totalDownloads").GetInt64());
            }
        }

        return Result.Failure<long>(Error.NotFound(
            "NuGetStats.PackageNotFound",
            $"Package '{packageId.Value}' was not found on NuGet.org."));
    }
}
