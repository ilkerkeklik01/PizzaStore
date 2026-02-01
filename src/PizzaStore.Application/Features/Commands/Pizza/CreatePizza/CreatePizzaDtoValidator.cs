using FluentValidation;

namespace PizzaStore.Application.Features.Commands.Pizza.CreatePizza;

public class CreatePizzaDtoValidator : AbstractValidator<CreatePizzaDto>
{
    public CreatePizzaDtoValidator()
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

        RuleFor(x => x.Variants)
            .NotEmpty().WithMessage("At least one pizza variant is required")
            .Must(variants => variants.Distinct(new PizzaSizeComparer()).Count() == variants.Count)
            .WithMessage("Pizza cannot have duplicate sizes");

        RuleForEach(x => x.Variants)
            .ChildRules(variant =>
            {
                variant.RuleFor(v => v.Size)
                    .IsInEnum().WithMessage("Pizza size must be a valid enum value");

                variant.RuleFor(v => v.Price)
                    .GreaterThan(0).WithMessage("Pizza price must be greater than 0");
            });
    }

    private class PizzaSizeComparer : IEqualityComparer<PizzaVariantDto>
    {
        public bool Equals(PizzaVariantDto? x, PizzaVariantDto? y)
        {
            return x?.Size == y?.Size;
        }

        public int GetHashCode(PizzaVariantDto obj)
        {
            return obj.Size.GetHashCode();
        }
    }
}
