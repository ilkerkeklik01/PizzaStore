using FluentValidation;

namespace PizzaStore.Application.Features.Commands.Cart.UpdateCartItemQuantity;

public class UpdateCartItemQuantityDtoValidator : AbstractValidator<UpdateCartItemQuantityDto>
{
    public UpdateCartItemQuantityDtoValidator()
    {
        RuleFor(x => x.CartItemId)
            .NotEmpty().WithMessage("Cart item ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.SpecialInstructions)
            .MaximumLength(500).WithMessage("Special instructions must not exceed 500 characters");
    }
}
