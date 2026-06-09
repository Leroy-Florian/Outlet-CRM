using System.Reflection;
using Crm.Core.Application.Abstractions;
using Crm.Core.Domain.Prospects;
using Crm.Core.Infrastructure.Persistence;
using Crm.Kernel.Shared;
using NetArchTest.Rules;
using Xunit;

namespace Crm.Architecture.Tests;

public sealed class ConventionTests
{
    private static readonly Assembly Domain = typeof(Prospect).Assembly;
    private static readonly Assembly Application = typeof(IUnitOfWork).Assembly;
    private static readonly Assembly Infrastructure = typeof(CrmDbContext).Assembly;

    [Fact]
    public void Should_BeSealed_When_TypeIsAnAggregateRoot()
    {
        var result = Types.InAssembly(Domain)
            .That().Inherit(typeof(AggregateRoot<>))
            .Should().BeSealed()
            .GetResult();

        Assert.True(result.IsSuccessful, FailingTypes(result));
    }

    [Fact]
    public void Should_BeSealed_When_TypeIsAUseCase()
    {
        var result = Types.InAssembly(Application)
            .That().AreClasses().And().DoNotResideInNamespace("Crm.Core.Application.Abstractions")
            .Should().BeSealed()
            .GetResult();

        Assert.True(result.IsSuccessful, FailingTypes(result));
    }

    [Fact]
    public void Should_OnlyExposeInterfaces_When_NamespaceIsApplicationAbstractions()
    {
        var result = Types.InAssembly(Application)
            .That().ResideInNamespace("Crm.Core.Application.Abstractions")
            .Should().BeInterfaces()
            .GetResult();

        Assert.True(result.IsSuccessful, FailingTypes(result));
    }

    [Fact]
    public void Should_ReturnResult_When_MethodIsAUseCaseHandler()
    {
        var handlers = Application.GetTypes()
            .Where(t => t is { IsClass: true, IsSealed: true } && !t.Namespace!.EndsWith(".Abstractions", StringComparison.Ordinal))
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(m => m.Name == "HandleAsync")
            .ToList();

        Assert.NotEmpty(handlers);
        Assert.All(handlers, method =>
        {
            var returned = method.ReturnType;
            Assert.True(returned.IsGenericType && returned.GetGenericTypeDefinition() == typeof(Task<>));
            var inner = returned.GetGenericArguments()[0];
            Assert.True(typeof(Result).IsAssignableFrom(inner), $"{method.DeclaringType!.Name}.HandleAsync must return a Result.");
        });
    }

    [Fact]
    public void Should_ImplementAnApplicationPort_When_TypeIsARepository()
    {
        var result = Types.InAssembly(Infrastructure)
            .That().HaveNameEndingWith("Repository")
            .Should().ImplementInterface(typeof(IUnitOfWork)).Or().BeClasses()
            .GetResult();

        var repositories = Infrastructure.GetTypes().Where(t => t.Name.EndsWith("Repository", StringComparison.Ordinal));

        Assert.All(repositories, repository =>
            Assert.Contains(repository.GetInterfaces(), i => i.Namespace == "Crm.Core.Application.Abstractions"));
    }

    [Fact]
    public void Should_NotThrowFromDomainFactories_When_InputIsInvalid()
    {
        var factories = Domain.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(m => m.Name == "Create" && m.DeclaringType!.Namespace!.StartsWith("Crm.Core.Domain", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(factories);
        Assert.All(factories, factory =>
            Assert.True(typeof(Result).IsAssignableFrom(factory.ReturnType), $"{factory.DeclaringType!.Name}.Create must return a Result."));
    }

    private static string FailingTypes(TestResult result) =>
        string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []);
}
