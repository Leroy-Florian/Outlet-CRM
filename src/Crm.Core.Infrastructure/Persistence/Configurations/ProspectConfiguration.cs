using Crm.Core.Domain.Prospects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Core.Infrastructure.Persistence.Configurations;

public sealed class ProspectConfiguration : IEntityTypeConfiguration<Prospect>
{
    public void Configure(EntityTypeBuilder<Prospect> builder)
    {
        builder.ToTable("prospects");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasConversion(id => id.Value, value => new ProspectId(value));
        builder.Property(p => p.ProductId).HasConversion(id => id.Value, value => new Crm.Core.Domain.Products.ProductId(value));
        builder.Property(p => p.OrganizationId).HasConversion<Guid?>(
            id => id == null ? null : id.Value.Value,
            value => value == null ? null : new Crm.Core.Domain.Organizations.OrganizationId(value.Value));
        builder.Property(p => p.Name).HasMaxLength(200);
        builder.Property(p => p.Company).HasMaxLength(200);
        builder.Property(p => p.Stage).HasConversion<string>().HasMaxLength(20);
        builder.ComplexProperty(p => p.Email, email => email.Property(e => e.Value).HasColumnName("email").HasMaxLength(320));

        builder.OwnsMany(p => p.Interactions, interactions =>
        {
            interactions.ToTable("prospect_interactions");
            interactions.WithOwner().HasForeignKey("prospect_id");
            interactions.HasKey(i => i.Id);
            interactions.Property(i => i.Channel).HasMaxLength(50);
        });

        builder.Ignore(p => p.DomainEvents);
    }
}
