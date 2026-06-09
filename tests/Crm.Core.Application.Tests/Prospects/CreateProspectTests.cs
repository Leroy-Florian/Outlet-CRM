using Crm.Core.Application.Prospects;
using Crm.Core.Application.Tests.Fakes;
using Crm.Core.Domain.Organizations;
using Crm.Core.Domain.Products;
using Crm.Core.Domain.Prospects;
using Xunit;

namespace Crm.Core.Application.Tests.Prospects;

public sealed class CreateProspectTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 9, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeProspectRepository _repository = new();
    private readonly FakeProductRepository _products = new();
    private readonly FakeOrganizationRepository _organizations = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Product _product = Product.Create("Outlet", null, Now).Value;

    public CreateProspectTests() => _products.Items.Add(_product);

    private CreateProspect UseCase => new(_repository, _products, _organizations, _unitOfWork, new FakeClock(Now));

    [Fact]
    public async Task Should_PersistProspect_When_CommandIsValid()
    {
        var result = await UseCase.HandleAsync(
            new CreateProspectCommand(_product.Id.Value, null, "Ada", "ada@example.com", "AE"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stored = Assert.Single(_repository.Items);
        Assert.Equal(result.Value, stored.Id);
        Assert.Equal(_product.Id, stored.ProductId);
        Assert.Null(stored.OrganizationId);
        Assert.Equal(Now, stored.CreatedAt);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Should_LinkOrganization_When_ItExists()
    {
        var organization = Organization.Create("Acme", null, Now).Value;
        _organizations.Items.Add(organization);

        var result = await UseCase.HandleAsync(
            new CreateProspectCommand(_product.Id.Value, organization.Id.Value, "Ada", "ada@example.com", null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(organization.Id, Assert.Single(_repository.Items).OrganizationId);
    }

    [Fact]
    public async Task Should_Fail_When_OrganizationDoesNotExist()
    {
        var result = await UseCase.HandleAsync(
            new CreateProspectCommand(_product.Id.Value, Guid.NewGuid(), "Ada", "ada@example.com", null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Organization.NotFound", result.Error.Code);
        Assert.Empty(_repository.Items);
    }

    [Fact]
    public async Task Should_Fail_When_ProductDoesNotExist()
    {
        var result = await UseCase.HandleAsync(
            new CreateProspectCommand(Guid.NewGuid(), null, "Ada", "ada@example.com", null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Product.NotFound", result.Error.Code);
        Assert.Empty(_repository.Items);
    }

    [Fact]
    public async Task Should_Fail_When_EmailIsInvalid()
    {
        var result = await UseCase.HandleAsync(
            new CreateProspectCommand(_product.Id.Value, null, "Ada", "not-an-email", null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Email.Invalid", result.Error.Code);
        Assert.Empty(_repository.Items);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Should_Fail_When_NameIsBlank()
    {
        var result = await UseCase.HandleAsync(
            new CreateProspectCommand(_product.Id.Value, null, " ", "ada@example.com", null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ProspectErrors.NameRequired, result.Error);
    }
}
