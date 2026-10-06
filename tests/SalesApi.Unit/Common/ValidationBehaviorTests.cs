using SalesApi.Application.Sales.CancelSale;
using SalesApi.Common.Validation;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace SalesApi.Unit.Common;

/// <summary>
/// Contains unit tests for the <see cref="ValidationBehavior{TRequest, TResponse}"/>,
/// the MediatR pipeline step that runs every validator before the handler.
/// </summary>
public class ValidationBehaviorTests
{
    private readonly ValidationBehavior<CancelSaleCommand, CancelSaleResponse> _behavior =
        new(new IValidator<CancelSaleCommand>[] { new CancelSaleValidator() });

    [Fact(DisplayName = "Given an invalid request When it goes through the pipeline Then throws ValidationException and the handler never runs")]
    public async Task Handle_InvalidRequest_ThrowsAndSkipsHandler()
    {
        // Given
        var handlerRan = false;

        // When
        var act = () => _behavior.Handle(new CancelSaleCommand(Guid.Empty), _ =>
        {
            handlerRan = true;
            return Task.FromResult(new CancelSaleResponse());
        }, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<ValidationException>();
        handlerRan.Should().BeFalse();
    }

    [Fact(DisplayName = "Given a valid request When it goes through the pipeline Then the handler runs and its response is returned")]
    public async Task Handle_ValidRequest_RunsHandler()
    {
        // Given
        var expected = new CancelSaleResponse();

        // When
        var response = await _behavior.Handle(new CancelSaleCommand(Guid.NewGuid()), _ => Task.FromResult(expected), CancellationToken.None);

        // Then
        response.Should().BeSameAs(expected);
    }
}
