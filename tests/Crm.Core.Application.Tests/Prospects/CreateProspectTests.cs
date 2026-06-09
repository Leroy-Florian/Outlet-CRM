using Crm.Core.Application.Prospects;
using Crm.Core.Application.Tests.Fakes;
using Crm.Core.Domain.Prospects;
using Xunit;

namespace Crm.Core.Application.Tests.Prospects;

public sealed class CreateProspectTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 9, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeProspectRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CreateProspect UseCase => new(_repository, _unitOfWork, new FakeClock(Now));

    [Fact]
    public async Task Should_PersistProspect_When_CommandIsValid()
    {
        var result = await UseCase.HandleAsync(new CreateProspectCommand("Ada", "ada@example.com", "AE"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stored = Assert.Single(_repository.Items);
        Assert.Equal(result.Value, stored.Id);
        Assert.Equal(Now, stored.CreatedAt);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Should_Fail_When_EmailIsInvalid()
    {
        var result = await UseCase.HandleAsync(new CreateProspectCommand("Ada", "not-an-email", null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Email.Invalid", result.Error.Code);
        Assert.Empty(_repository.Items);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Should_Fail_When_NameIsBlank()
    {
        var result = await UseCase.HandleAsync(new CreateProspectCommand(" ", "ada@example.com", null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ProspectErrors.NameRequired, result.Error);
    }
}
