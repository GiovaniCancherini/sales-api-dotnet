using SalesApi.Application.Sales.AddSaleItem;
using SalesApi.Application.Sales.CancelSale;
using SalesApi.Application.Sales.CancelSaleItem;
using SalesApi.Application.Sales.CreateSale;
using SalesApi.Application.Sales.GetBySaleId;
using SalesApi.Application.Sales.UpdateSale;
using SalesApi.Application.Users.CreateUser;
using SalesApi.Application.Users.UpdateUser;
using FluentValidation.TestHelper;
using Xunit;

namespace SalesApi.Unit.Application;

/// <summary>
/// Validation runs in the MediatR pipeline (ValidationBehavior), before any handler,
/// so invalid input is tested against each command/query validator directly.
/// </summary>
public class ValidatorsTests
{
    [Fact(DisplayName = "Given item with zero quantity When validating AddSaleItemCommand Then fails on the quantity")]
    public void AddSaleItem_ZeroQuantity_Fails()
    {
        var command = new AddSaleItemCommand
        {
            SaleId = Guid.NewGuid(),
            Item = new CreateSaleItemCommand
            {
                ProductId = Guid.NewGuid().ToString(),
                ProductName = "Refrigerante",
                ProductSku = "SKU-002",
                Quantity = 0,
                UnitPrice = 10m
            }
        };

        new AddSaleItemValidator().TestValidate(command).ShouldHaveValidationErrorFor(c => c.Item.Quantity);
    }

    [Fact(DisplayName = "Given empty sale id When validating CancelSaleCommand Then fails on the id")]
    public void CancelSale_EmptyId_Fails()
    {
        new CancelSaleValidator().TestValidate(new CancelSaleCommand(Guid.Empty)).ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact(DisplayName = "Given empty item id When validating CancelSaleItemCommand Then fails on the item id")]
    public void CancelSaleItem_EmptyItemId_Fails()
    {
        new CancelSaleItemValidator().TestValidate(new CancelSaleItemCommand(Guid.NewGuid(), Guid.Empty))
            .ShouldHaveValidationErrorFor(c => c.ItemId);
    }

    [Fact(DisplayName = "Given empty command When validating CreateSaleCommand Then fails on the required fields")]
    public void CreateSale_EmptyCommand_Fails()
    {
        var result = new CreateSaleCommandValidator().TestValidate(new CreateSaleCommand());

        result.ShouldHaveValidationErrorFor(c => c.SaleNumber);
        result.ShouldHaveValidationErrorFor(c => c.Items);
    }

    [Fact(DisplayName = "Given empty command When validating CreateUserCommand Then fails on the required fields")]
    public void CreateUser_EmptyCommand_Fails()
    {
        var result = new CreateUserCommandValidator().TestValidate(new CreateUserCommand());

        result.ShouldHaveValidationErrorFor(c => c.Username);
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact(DisplayName = "Given empty id When validating GetBySaleIdQuery Then fails on the id")]
    public void GetBySaleId_EmptyId_Fails()
    {
        new GetBySaleIdValidator().TestValidate(new GetBySaleIdQuery(Guid.Empty)).ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact(DisplayName = "Given command without items When validating UpdateSaleCommand Then fails on the items")]
    public void UpdateSale_NoItems_Fails()
    {
        var command = new UpdateSaleCommand { Id = Guid.NewGuid(), Items = new List<CreateSaleItemCommand>() };

        new UpdateSaleValidator().TestValidate(command).ShouldHaveValidationErrorFor(c => c.Items);
    }

    [Fact(DisplayName = "Given empty id When validating UpdateUserCommand Then fails on the id")]
    public void UpdateUser_EmptyId_Fails()
    {
        new UpdateUserCommandValidator().TestValidate(new UpdateUserCommand()).ShouldHaveValidationErrorFor(c => c.Id);
    }
}
