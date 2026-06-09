using Crm.Core.Domain.ApiMetrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Core.Infrastructure.Persistence.Configurations;

public sealed class ApiMetricSampleConfiguration : IEntityTypeConfiguration<ApiMetricSample>
{
    public void Configure(EntityTypeBuilder<ApiMetricSample> builder)
    {
        builder.ToTable("api_metric_samples");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.ProductId).HasConversion(id => id.Value, value => new Crm.Core.Domain.Products.ProductId(value));
        builder.Property(s => s.Endpoint).HasMaxLength(300);
        builder.HasIndex(s => s.OccurredAt);
        builder.Ignore(s => s.DomainEvents);
    }
}
