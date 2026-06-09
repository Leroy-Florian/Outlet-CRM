using Crm.Core.Domain.Payments;
using Crm.Core.Domain.Products;
using Xunit;

namespace Crm.Core.Domain.Tests.Payments;

public sealed class PaymentTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 9, 12, 0, 0, TimeSpan.Zero);

    private static Payment CreatePayment() =>
        Payment.Create(ProductId.New(), null, Money.Create(49.99m, "eur").Value, "stripe", "pi_123", Now).Value;

    [Fact]
    public void Should_StartPending_When_Created()
    {
        Assert.Equal(PaymentStatus.Pending, CreatePayment().Status);
    }

    [Fact]
    public void Should_Fail_When_SourceIsBlank()
    {
        var result = Payment.Create(ProductId.New(), null, Money.Create(1m, "EUR").Value, " ", "ref", Now);

        Assert.True(result.IsFailure);
        Assert.Equal(PaymentErrors.SourceRequired, result.Error);
    }

    [Fact]
    public void Should_Settle_When_Pending()
    {
        var payment = CreatePayment();

        var result = payment.Settle();

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Settled, payment.Status);
    }

    [Fact]
    public void Should_Fail_When_SettlingTwice()
    {
        var payment = CreatePayment();
        payment.Settle();

        var result = payment.Settle();

        Assert.True(result.IsFailure);
        Assert.Equal(PaymentErrors.NotPending, result.Error);
    }

    [Fact]
    public void Should_Refund_When_Settled()
    {
        var payment = CreatePayment();
        payment.Settle();

        var result = payment.Refund();

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
    }

    [Fact]
    public void Should_Fail_When_RefundingAPendingPayment()
    {
        var result = CreatePayment().Refund();

        Assert.True(result.IsFailure);
        Assert.Equal(PaymentErrors.NotSettled, result.Error);
    }

    [Fact]
    public void Should_NormalizeCurrency_When_CreatingMoney()
    {
        var money = Money.Create(10m, " eur ");

        Assert.True(money.IsSuccess);
        Assert.Equal("EUR", money.Value.Currency);
    }

    [Theory]
    [InlineData(-1, "EUR", "Money.Negative")]
    [InlineData(1, "EU", "Money.InvalidCurrency")]
    public void Should_Fail_When_MoneyIsInvalid(decimal amount, string currency, string expectedCode)
    {
        var result = Money.Create(amount, currency);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedCode, result.Error.Code);
    }
}
