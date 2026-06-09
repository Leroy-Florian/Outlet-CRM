using Crm.Core.Domain.Analytics;
using Crm.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Core.Infrastructure.Persistence.Configurations;

public sealed class RepositorySnapshotConfiguration : IEntityTypeConfiguration<RepositorySnapshot>
{
    public void Configure(EntityTypeBuilder<RepositorySnapshot> builder)
    {
        builder.ToTable("repository_snapshots");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.ProductId).HasConversion(id => id.Value, value => new ProductId(value));
        builder.ComplexProperty(s => s.Repository, repository =>
        {
            repository.Property(r => r.Owner).HasColumnName("repo_owner").HasMaxLength(100);
            repository.Property(r => r.Name).HasColumnName("repo_name").HasMaxLength(100);
        });
        builder.Ignore(s => s.DomainEvents);
    }
}
