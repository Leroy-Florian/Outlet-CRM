using Crm.Core.Application.Payments;
using Crm.Core.Application.Tests.Fakes;
using Crm.Core.Domain.Payments;
using Xunit;

namespace Crm.Core.Application.Tests.Payments;

public sealed class RecordPaymentTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 9, 12, 0, 0, TimeSpan.Zero);

    private readonly FakePaymentRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private RecordPayment UseCase => new(_repository, _unitOfWork, new FakeClock(Now));

    [Fact]
    public async Task Should_PersistPendingPayment_When_CommandIsValid()
    {
        var result = await UseCase.HandleAsync(
            new RecordPaymentCommand(49.99m, "eur", "stripe", "pi_123"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var payment = Assert.Single(_repository.Items);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal("EUR", payment.Amount.Currency);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Should_Fail_When_CurrencyIsInvalid()
    {
        var result = await UseCase.HandleAsync(
            new RecordPaymentCommand(10m, "EURO", "stripe", "pi_123"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Money.InvalidCurrency", result.Error.Code);
        Assert.Empty(_repository.Items);
    }

    [Fact]
    public async Task Should_SettlePayment_When_ItExists()
    {
        await UseCase.HandleAsync(new RecordPaymentCommand(10m, "EUR", "stripe", "pi_1"), CancellationToken.None);
        var settle = new SettlePayment(_repository, _unitOfWork);

        var result = await settle.HandleAsync(new SettlePaymentCommand(_repository.Items[0].Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Settled, _repository.Items[0].Status);
    }

    [Fact]
    public async Task Should_Fail_When_SettlingUnknownPayment()
    {
        var settle = new SettlePayment(_repository, _unitOfWork);

        var result = await settle.HandleAsync(new SettlePaymentCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Payment.NotFound", result.Error.Code);
    }
}
