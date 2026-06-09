using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Payments;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.Payments;

public sealed record SettlePaymentCommand(Guid PaymentId);

public sealed class SettlePayment(IPaymentRepository payments, IUnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(SettlePaymentCommand command, CancellationToken cancellationToken)
    {
        var payment = await payments.GetByIdAsync(command.PaymentId, cancellationToken);
        if (payment is null)
        {
            return Result.Failure(PaymentErrors.NotFound(command.PaymentId));
        }

        var settled = payment.Settle();
        if (settled.IsFailure)
        {
            return settled;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
