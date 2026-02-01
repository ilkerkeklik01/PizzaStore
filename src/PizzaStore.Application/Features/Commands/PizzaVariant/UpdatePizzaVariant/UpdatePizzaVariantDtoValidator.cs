using FluentValidation;

namespace PizzaStore.Application.Features.Commands.PizzaVariant.UpdatePizzaVariant;

public class UpdatePizzaVariantDtoValidator : AbstractValidator<UpdatePizzaVariantDto>
{
    public UpdatePizzaVariantDtoValidator()
    {
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Pizza price must be greater than 0");

        RuleFor(x => x.IsAvailable)
            .NotNull().WithMessage("IsAvailable must be specified");
    }
}
