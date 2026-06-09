using Crm.Core.Domain.Analytics;
using Crm.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Core.Infrastructure.Persistence.Configurations;

public sealed class DownloadSnapshotConfiguration : IEntityTypeConfiguration<DownloadSnapshot>
{
    public void Configure(EntityTypeBuilder<DownloadSnapshot> builder)
    {
        builder.ToTable("download_snapshots");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.ProductId).HasConversion(id => id.Value, value => new ProductId(value));
        builder.Property(s => s.Registry).HasConversion<string>().HasMaxLength(20);
        builder.ComplexProperty(s => s.PackageId, packageId =>
            packageId.Property(p => p.Value).HasColumnName("package_id").HasMaxLength(100));
        builder.Ignore(s => s.DomainEvents);
    }
}
