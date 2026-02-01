using FluentValidation;

namespace PizzaStore.Application.Features.Commands.Topping.UpdateTopping;

public class UpdateToppingDtoValidator : AbstractValidator<UpdateToppingDto>
{
    public UpdateToppingDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Topping name is required")
            .MaximumLength(100).WithMessage("Topping name must not exceed 100 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Topping price must be greater than 0");

        RuleFor(x => x.IsAvailable)
            .NotNull().WithMessage("IsAvailable must be specified");
    }
}
