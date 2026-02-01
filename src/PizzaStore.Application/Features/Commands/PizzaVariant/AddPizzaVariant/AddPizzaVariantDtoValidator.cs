using FluentValidation;

namespace PizzaStore.Application.Features.Commands.PizzaVariant.AddPizzaVariant;

public class AddPizzaVariantDtoValidator : AbstractValidator<AddPizzaVariantDto>
{
    public AddPizzaVariantDtoValidator()
    {
        RuleFor(x => x.PizzaId)
            .NotEmpty().WithMessage("Pizza ID is required");

        RuleFor(x => x.Size)
            .IsInEnum().WithMessage("Pizza size must be a valid enum value");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Pizza price must be greater than 0");
    }
}
