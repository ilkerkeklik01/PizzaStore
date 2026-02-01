using MediatR;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Commands.PizzaVariant.DeletePizzaVariant;

public class DeletePizzaVariantCommandHandler : IRequestHandler<DeletePizzaVariantCommand, DeletePizzaVariantResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeletePizzaVariantCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<DeletePizzaVariantResponse> Handle(DeletePizzaVariantCommand request, CancellationToken cancellationToken)
    {
        // Verify admin role
        if (!_currentUserService.IsInRole("Admin"))
            throw new UnauthorizedException("Only administrators can delete pizza variants");

        // Find the pizza variant
        var variant = await _unitOfWork.PizzaVariants.GetByIdAsync(request.Id);
        
        if (variant == null)
        {
            throw new NotFoundException($"Pizza variant with ID '{request.Id}' not found.");
        }

        // Soft delete - set IsAvailable to false
        variant.IsAvailable = false;

        // Save changes (entity is already tracked by EF Core, changes will be saved automatically)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeletePizzaVariantResponse
        {
            Message = "Pizza variant has been successfully deleted (marked as unavailable)."
        };
    }
}
