using MediatR;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Topping.Commands.DeleteTopping;

public class DeleteToppingCommandHandler : IRequestHandler<DeleteToppingCommand, DeleteToppingResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteToppingCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteToppingResponse> Handle(DeleteToppingCommand request, CancellationToken cancellationToken)
    {
        // Find the topping
        var topping = await _unitOfWork.Toppings.GetByIdAsync(request.Id);

        if (topping == null)
        {
            throw new NotFoundException($"Topping with ID '{request.Id}' not found.");
        }

        // Soft delete - set IsAvailable to false
        topping.IsAvailable = false;

        // Save changes (entity is already tracked by EF Core, changes will be saved automatically)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteToppingResponse
        {
            Message = "Topping has been successfully deleted (marked as unavailable)."
        };
    }
}
