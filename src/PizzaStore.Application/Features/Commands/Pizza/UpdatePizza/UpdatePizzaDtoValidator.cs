using FluentValidation;

namespace PizzaStore.Application.Features.Commands.Pizza.UpdatePizza;

public class UpdatePizzaDtoValidator : AbstractValidator<UpdatePizzaDto>
{
    public UpdatePizzaDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Pizza name is required")
            .MaximumLength(200).WithMessage("Pizza name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Pizza description is required")
            .MaximumLength(1000).WithMessage("Pizza description must not exceed 1000 characters");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Pizza type must be a valid enum value");

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("Image URL is required")
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters");

        RuleFor(x => x.IsAvailable)
            .NotNull().WithMessage("IsAvailable status is required");
    }
}
