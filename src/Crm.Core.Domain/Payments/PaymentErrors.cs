using Crm.Kernel.Shared;

namespace Crm.Core.Domain.Payments;

public static class PaymentErrors
{
    public static readonly Error SourceRequired =
        Error.Validation("Payment.SourceRequired", "A payment requires a source (provider) identifier.");

    public static readonly Error NotPending =
        Error.Conflict("Payment.NotPending", "Only a pending payment can be settled or failed.");

    public static readonly Error NotSettled =
        Error.Conflict("Payment.NotSettled", "Only a settled payment can be refunded.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("Payment.NotFound", $"Payment '{id}' was not found.");
}
