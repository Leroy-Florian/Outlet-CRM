using Crm.Core.Domain.Payments;

namespace Crm.Core.Application.Abstractions;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Payment>> ListAsync(CancellationToken cancellationToken);

    Task AddAsync(Payment payment, CancellationToken cancellationToken);
}
