using Crm.Core.Application.Prospects;
using Crm.Core.Application.Tests.Fakes;
using Crm.Core.Domain.Products;
using Crm.Core.Domain.Prospects;
using Xunit;

namespace Crm.Core.Application.Tests.Prospects;

public sealed class AdvanceProspectStageTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 9, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeProspectRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    [Fact]
    public async Task Should_AdvanceStage_When_ProspectExists()
    {
        var prospect = Prospect.Create(ProductId.New(), null, "Ada", Email.Create("ada@example.com").Value, null, Now).Value;
        _repository.Items.Add(prospect);
        var useCase = new AdvanceProspectStage(_repository, _unitOfWork);

        var result = await useCase.HandleAsync(
            new AdvanceProspectStageCommand(prospect.Id, ProspectStage.Contacted), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ProspectStage.Contacted, prospect.Stage);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Should_Fail_When_ProspectIsMissing()
    {
        var useCase = new AdvanceProspectStage(_repository, _unitOfWork);

        var result = await useCase.HandleAsync(
            new AdvanceProspectStageCommand(ProspectId.New(), ProspectStage.Contacted), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Prospect.NotFound", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
