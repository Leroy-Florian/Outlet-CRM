using System.Reflection;
using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Prospects;
using Crm.Core.Infrastructure.Persistence;
using Crm.Kernel.Shared;
using NetArchTest.Rules;
using Xunit;

namespace Crm.Architecture.Tests;

public sealed class LayerDependencyTests
{
    private static readonly Assembly Kernel = typeof(Result).Assembly;
    private static readonly Assembly Domain = typeof(Prospect).Assembly;
    private static readonly Assembly Application = typeof(IUnitOfWork).Assembly;
    private static readonly Assembly Infrastructure = typeof(CrmDbContext).Assembly;

    [Fact]
    public void Should_HaveNoProjectDependencies_When_AssemblyIsKernel()
    {
        var references = Kernel.GetReferencedAssemblies().Select(a => a.Name);

        Assert.DoesNotContain(references, name => name!.StartsWith("Crm.", StringComparison.Ordinal));
    }

    [Fact]
    public void Should_NotDependOnApplication_When_TypeIsInDomain()
    {
        var result = Types.InAssembly(Domain)
            .ShouldNot().HaveDependencyOn("Crm.Core.Application")
            .GetResult();

        Assert.True(result.IsSuccessful, FailingTypes(result));
    }

    [Fact]
    public void Should_NotDependOnInfrastructure_When_TypeIsInDomain()
    {
        var result = Types.InAssembly(Domain)
            .ShouldNot().HaveDependencyOn("Crm.Core.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FailingTypes(result));
    }

    [Fact]
    public void Should_NotDependOnInfrastructure_When_TypeIsInApplication()
    {
        var result = Types.InAssembly(Application)
            .ShouldNot().HaveDependencyOn("Crm.Core.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FailingTypes(result));
    }

    [Theory]
    [InlineData("Microsoft.EntityFrameworkCore")]
    [InlineData("System.Net.Http")]
    [InlineData("System.Text.Json")]
    public void Should_KeepExternalConcernsOutOfDomain_When_CheckingDependencies(string forbidden)
    {
        var result = Types.InAssembly(Domain)
            .ShouldNot().HaveDependencyOn(forbidden)
            .GetResult();

        Assert.True(result.IsSuccessful, FailingTypes(result));
    }

    [Theory]
    [InlineData("Microsoft.EntityFrameworkCore")]
    [InlineData("System.Net.Http")]
    public void Should_KeepExternalConcernsOutOfApplication_When_CheckingDependencies(string forbidden)
    {
        var result = Types.InAssembly(Application)
            .ShouldNot().HaveDependencyOn(forbidden)
            .GetResult();

        Assert.True(result.IsSuccessful, FailingTypes(result));
    }

    [Fact]
    public void Should_OnlyInfrastructureReferenceEfCore_When_CheckingSolution()
    {
        var references = Infrastructure.GetReferencedAssemblies().Select(a => a.Name);

        Assert.Contains("Microsoft.EntityFrameworkCore", references);
    }

    private static string FailingTypes(TestResult result) =>
        string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []);
}
