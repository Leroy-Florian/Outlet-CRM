using Crm.Core.Domain.Prospects;
using Xunit;

namespace Crm.Core.Domain.Tests.Prospects;

public sealed class EmailTests
{
    [Theory]
    [InlineData("ada@example.com")]
    [InlineData("  ADA@Example.COM  ")]
    public void Should_NormalizeValue_When_EmailIsValid(string raw)
    {
        var result = Email.Create(raw);

        Assert.True(result.IsSuccess);
        Assert.Equal("ada@example.com", result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("no-at-sign")]
    [InlineData("@example.com")]
    [InlineData("ada@")]
    [InlineData("ada lovelace@example.com")]
    public void Should_Fail_When_EmailIsInvalid(string raw)
    {
        var result = Email.Create(raw);

        Assert.True(result.IsFailure);
        Assert.Equal("Email.Invalid", result.Error.Code);
    }
}
