using FluentValidation;

namespace PizzaStore.Application.Features.Commands.Cart.AddPizzaToCart;

public class AddPizzaToCartDtoValidator : AbstractValidator<AddPizzaToCartDto>
{
    public AddPizzaToCartDtoValidator()
    {
        RuleFor(x => x.PizzaVariantId)
            .NotEmpty().WithMessage("Pizza variant ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.SpecialInstructions)
            .MaximumLength(500).WithMessage("Special instructions must not exceed 500 characters");

        RuleFor(x => x.ToppingIds)
            .NotNull().WithMessage("Topping IDs list cannot be null");
    }
}
