using Crm.Core.Domain.Organizations;
using Xunit;

namespace Crm.Core.Domain.Tests.Organizations;

public sealed class OrganizationTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 9, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Should_TrimNameAndWebsite_When_Created()
    {
        var result = Organization.Create("  Acme Corp ", " https://acme.example ", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("Acme Corp", result.Value.Name);
        Assert.Equal("https://acme.example", result.Value.Website);
    }

    [Fact]
    public void Should_Fail_When_NameIsBlank()
    {
        var result = Organization.Create("  ", null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal(OrganizationErrors.NameRequired, result.Error);
    }
}
