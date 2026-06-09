using Crm.Core.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Core.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.ProductId).HasConversion(id => id.Value, value => new Crm.Core.Domain.Products.ProductId(value));
        builder.Property(p => p.OrganizationId).HasConversion<Guid?>(
            id => id == null ? null : id.Value.Value,
            value => value == null ? null : new Crm.Core.Domain.Organizations.OrganizationId(value.Value));
        builder.Property(p => p.Source).HasMaxLength(50);
        builder.Property(p => p.ExternalReference).HasMaxLength(200);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

        builder.ComplexProperty(p => p.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("amount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("currency").HasMaxLength(3);
        });

        builder.Ignore(p => p.DomainEvents);
    }
}
