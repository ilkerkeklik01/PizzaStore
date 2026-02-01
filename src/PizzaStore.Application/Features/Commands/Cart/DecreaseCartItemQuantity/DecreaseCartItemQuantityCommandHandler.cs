using MediatR;
using PizzaStore.Application.Extensions;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Commands.Cart.DecreaseCartItemQuantity;

public class DecreaseCartItemQuantityCommandHandler : IRequestHandler<DecreaseCartItemQuantityCommand, DecreaseCartItemQuantityResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DecreaseCartItemQuantityCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<DecreaseCartItemQuantityResponse> Handle(DecreaseCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        // Check authentication
        var userId = _currentUserService.GetAuthenticatedUserId();

        // Validate amount
        if (request.Amount <= 0)
            throw new PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException("Amount must be greater than 0");

        // Verify cart item exists and belongs to user
        var cartItem = await _unitOfWork.CartItems.GetByIdAsync(request.CartItemId);
        if (cartItem == null)
            throw new NotFoundException($"Cart item with ID '{request.CartItemId}' not found");

        var isOwned = await _unitOfWork.Carts.IsCartItemOwnedByUserAsync(request.CartItemId, userId);
        if (!isOwned)
            throw new UnauthorizedException("You do not have permission to update this cart item");

        // Calculate new quantity
        var newQuantity = cartItem.Quantity - request.Amount;

        // If quantity would be <= 0, remove the item
        if (newQuantity <= 0)
        {
            await _unitOfWork.CartItems.DeleteAsync(cartItem);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new DecreaseCartItemQuantityResponse
            {
                Message = "Cart item removed because quantity would be zero or negative",
                Success = true,
                ItemRemoved = true
            };
        }

        // Otherwise, update the quantity
        cartItem.Quantity = newQuantity;
        // Save changes - EF Core tracks changes automatically
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DecreaseCartItemQuantityResponse
        {
            Message = "Cart item quantity decreased successfully",
            Success = true,
            ItemRemoved = false
        };
    }
}
