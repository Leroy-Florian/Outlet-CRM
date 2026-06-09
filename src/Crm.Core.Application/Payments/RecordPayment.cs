using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Payments;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.Payments;

public sealed record RecordPaymentCommand(decimal Amount, string Currency, string Source, string ExternalReference);

public sealed class RecordPayment(IPaymentRepository payments, IUnitOfWork unitOfWork, IClock clock)
{
    public async Task<Result<Guid>> HandleAsync(RecordPaymentCommand command, CancellationToken cancellationToken)
    {
        var money = Money.Create(command.Amount, command.Currency);
        if (money.IsFailure)
        {
            return Result.Failure<Guid>(money.Error);
        }

        var payment = Payment.Create(money.Value, command.Source, command.ExternalReference, clock.UtcNow);
        if (payment.IsFailure)
        {
            return Result.Failure<Guid>(payment.Error);
        }

        await payments.AddAsync(payment.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(payment.Value.Id);
    }
}
