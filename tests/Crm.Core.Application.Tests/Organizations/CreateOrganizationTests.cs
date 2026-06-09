using Crm.Core.Application.Organizations;
using Crm.Core.Application.Tests.Fakes;
using Crm.Core.Domain.Organizations;
using Xunit;

namespace Crm.Core.Application.Tests.Organizations;

public sealed class CreateOrganizationTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 9, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeOrganizationRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CreateOrganization UseCase => new(_repository, _unitOfWork, new FakeClock(Now));

    [Fact]
    public async Task Should_PersistOrganization_When_CommandIsValid()
    {
        var result = await UseCase.HandleAsync(
            new CreateOrganizationCommand("Acme Corp", "https://acme.example"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stored = Assert.Single(_repository.Items);
        Assert.Equal(result.Value, stored.Id);
        Assert.Equal("Acme Corp", stored.Name);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Should_Fail_When_NameIsBlank()
    {
        var result = await UseCase.HandleAsync(new CreateOrganizationCommand("  ", null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(OrganizationErrors.NameRequired, result.Error);
        Assert.Empty(_repository.Items);
    }
}
