using Crm.Core.Domain.Analytics;
using Crm.Core.Domain.ApiMetrics;
using Crm.Core.Domain.Payments;
using Crm.Core.Domain.Prospects;
using Microsoft.EntityFrameworkCore;

namespace Crm.Core.Infrastructure.Persistence;

public sealed class CrmDbContext(DbContextOptions<CrmDbContext> options) : DbContext(options)
{
    public DbSet<Prospect> Prospects => Set<Prospect>();

    public DbSet<DownloadSnapshot> DownloadSnapshots => Set<DownloadSnapshot>();

    public DbSet<ApiMetricSample> ApiMetricSamples => Set<ApiMetricSample>();

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);
}
