using MediatR;
using Microsoft.Extensions.Logging;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Commands.Pizza.DeletePizza;

/// <summary>
/// Handles soft deletion of pizzas by administrators
/// </summary>
public class DeletePizzaCommandHandler : IRequestHandler<DeletePizzaCommand, DeletePizzaResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeletePizzaCommandHandler> _logger;

    public DeletePizzaCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<DeletePizzaCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<DeletePizzaResponse> Handle(DeletePizzaCommand request, CancellationToken cancellationToken)
    {
        // Verify admin role
        if (!_currentUserService.IsInRole("Admin"))
            throw new UnauthorizedException("Only administrators can delete pizzas");

        _logger.LogInformation("Admin attempting to delete pizza {PizzaId}", request.Id);

        // Find the pizza
        var pizza = await _unitOfWork.Pizzas.GetByIdAsync(request.Id);
        
        if (pizza == null)
        {
            _logger.LogWarning("Admin attempted to delete non-existent pizza {PizzaId}", request.Id);
            throw new NotFoundException($"Pizza with ID '{request.Id}' not found.");
        }

        // Soft delete - set IsAvailable to false
        pizza.IsAvailable = false;

        // Save changes (entity is already tracked by EF Core, changes will be saved automatically)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Pizza {PizzaId} '{PizzaName}' successfully deleted (marked as unavailable)", 
            request.Id, pizza.Name);

        return new DeletePizzaResponse
        {
            Message = "Pizza has been successfully deleted (marked as unavailable)."
        };
    }
}
