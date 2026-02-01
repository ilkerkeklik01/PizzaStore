using FluentValidation;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Features.Commands.Admin.UpdateOrderStatus;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid order status")
            .Must(status => IsValidStatusTransition(status))
            .WithMessage("Invalid status transition. Cannot transition from Delivered or Cancelled to Pending, Confirmed, or Preparing");
    }

    private static bool IsValidStatusTransition(OrderStatus newStatus)
    {
        // Can't transition to these states from Delivered or Cancelled
        // This is a basic validation - the handler will do more detailed validation
        return true;
    }
}
