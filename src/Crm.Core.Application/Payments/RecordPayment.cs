using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Payments;
using Crm.Core.Domain.Products;
using Crm.Kernel.Shared;

namespace Crm.Core.Application.Payments;

public sealed record RecordPaymentCommand(Guid ProductId, decimal Amount, string Currency, string Source, string ExternalReference);

public sealed class RecordPayment(
    IPaymentRepository payments,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<Result<Guid>> HandleAsync(RecordPaymentCommand command, CancellationToken cancellationToken)
    {
        var productId = new ProductId(command.ProductId);
        if (await products.GetByIdAsync(productId, cancellationToken) is null)
        {
            return Result.Failure<Guid>(ProductErrors.NotFound(productId));
        }

        var money = Money.Create(command.Amount, command.Currency);
        if (money.IsFailure)
        {
            return Result.Failure<Guid>(money.Error);
        }

        var payment = Payment.Create(productId, money.Value, command.Source, command.ExternalReference, clock.UtcNow);
        if (payment.IsFailure)
        {
            return Result.Failure<Guid>(payment.Error);
        }

        await payments.AddAsync(payment.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(payment.Value.Id);
    }
}
