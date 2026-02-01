using MediatR;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Commands.Topping.DeleteTopping;

public class DeleteToppingCommandHandler : IRequestHandler<DeleteToppingCommand, DeleteToppingResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteToppingCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<DeleteToppingResponse> Handle(DeleteToppingCommand request, CancellationToken cancellationToken)
    {
        // Verify admin role
        if (!_currentUserService.IsInRole("Admin"))
            throw new UnauthorizedException("Only administrators can delete toppings");

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
