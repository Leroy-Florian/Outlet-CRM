using Crm.Core.Domain.Analytics;
using Crm.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Core.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasConversion(id => id.Value, value => new ProductId(value));
        builder.Property(p => p.Name).HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(2000);

        builder.OwnsMany(p => p.Packages, packages =>
        {
            packages.ToTable("product_packages");
            packages.WithOwner().HasForeignKey("product_id");
            packages.HasKey(t => t.Id);
            packages.Property(t => t.Registry).HasConversion<string>().HasMaxLength(20);
            packages.Property(t => t.PackageId)
                .HasConversion(p => p.Value, value => PackageId.Create(value).Value)
                .HasColumnName("package_id")
                .HasMaxLength(100);
        });

        builder.OwnsMany(p => p.Repositories, repositories =>
        {
            repositories.ToTable("product_repositories");
            repositories.WithOwner().HasForeignKey("product_id");
            repositories.HasKey(t => t.Id);
            repositories.Property(t => t.Repository)
                .HasConversion(r => r.FullName, value => RepositoryName.Create(value).Value)
                .HasColumnName("repository")
                .HasMaxLength(200);
        });

        builder.Ignore(p => p.DomainEvents);
    }
}
