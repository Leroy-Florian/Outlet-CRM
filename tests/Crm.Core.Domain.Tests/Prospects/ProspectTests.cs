using Crm.Core.Domain.Prospects;
using Xunit;

namespace Crm.Core.Domain.Tests.Prospects;

public sealed class ProspectTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 9, 12, 0, 0, TimeSpan.Zero);

    private static Prospect CreateProspect() =>
        Prospect.Create("Ada Lovelace", Email.Create("ada@example.com").Value, "Analytical Engines", Now).Value;

    [Fact]
    public void Should_StartInNewStage_When_Created()
    {
        var prospect = CreateProspect();

        Assert.Equal(ProspectStage.New, prospect.Stage);
    }

    [Fact]
    public void Should_Fail_When_NameIsBlank()
    {
        var result = Prospect.Create("  ", Email.Create("ada@example.com").Value, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal(ProspectErrors.NameRequired, result.Error);
    }

    [Fact]
    public void Should_AdvanceStage_When_TargetIsForward()
    {
        var prospect = CreateProspect();

        var result = prospect.Advance(ProspectStage.Contacted);

        Assert.True(result.IsSuccess);
        Assert.Equal(ProspectStage.Contacted, prospect.Stage);
    }

    [Fact]
    public void Should_Fail_When_AdvancingBackward()
    {
        var prospect = CreateProspect();
        prospect.Advance(ProspectStage.Qualified);

        var result = prospect.Advance(ProspectStage.Contacted);

        Assert.True(result.IsFailure);
        Assert.Equal(ProspectErrors.InvalidTransition, result.Error);
    }

    [Fact]
    public void Should_AllowMarkingLost_When_FromAnyOpenStage()
    {
        var prospect = CreateProspect();

        var result = prospect.Advance(ProspectStage.Lost);

        Assert.True(result.IsSuccess);
        Assert.Equal(ProspectStage.Lost, prospect.Stage);
    }

    [Fact]
    public void Should_Fail_When_AdvancingAClosedProspect()
    {
        var prospect = CreateProspect();
        prospect.Advance(ProspectStage.Won);

        var result = prospect.Advance(ProspectStage.Lost);

        Assert.True(result.IsFailure);
        Assert.Equal(ProspectErrors.AlreadyClosed, result.Error);
    }

    [Fact]
    public void Should_AppendInteraction_When_Recorded()
    {
        var prospect = CreateProspect();

        prospect.RecordInteraction("email", "Intro call scheduled", Now);

        var interaction = Assert.Single(prospect.Interactions);
        Assert.Equal("email", interaction.Channel);
    }
}
