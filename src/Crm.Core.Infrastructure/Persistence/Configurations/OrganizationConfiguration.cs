using Crm.Core.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Core.Infrastructure.Persistence.Configurations;

public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasConversion(id => id.Value, value => new OrganizationId(value));
        builder.Property(o => o.Name).HasMaxLength(200);
        builder.Property(o => o.Website).HasMaxLength(500);
        builder.Ignore(o => o.DomainEvents);
    }
}
