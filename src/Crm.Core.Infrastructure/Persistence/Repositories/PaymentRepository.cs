using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace Crm.Core.Infrastructure.Persistence.Repositories;

public sealed class PaymentRepository(CrmDbContext dbContext) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Payments.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Payment>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Payments.AsNoTracking().OrderByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken) =>
        await dbContext.Payments.AddAsync(payment, cancellationToken);
}
