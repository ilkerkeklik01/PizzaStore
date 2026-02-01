using FluentValidation;
using MediatR;
using PizzaStore.Application.Extensions;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Commands.Cart.AddPizzaToCart;

/// <summary>
/// Handles adding a pizza with optional toppings to the user's cart
/// </summary>
public class AddPizzaToCartCommandHandler : IRequestHandler<AddPizzaToCartCommand, CartItemDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<AddPizzaToCartDto> _validator;
    private readonly ICurrentUserService _currentUserService;

    public AddPizzaToCartCommandHandler(
        IUnitOfWork unitOfWork,
        IValidator<AddPizzaToCartDto> validator,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _currentUserService = currentUserService;
    }

    public async Task<CartItemDto> Handle(AddPizzaToCartCommand request, CancellationToken cancellationToken)
    {
        // Check authentication
        var userId = _currentUserService.GetAuthenticatedUserId();

        // Validate DTO
        var validationResult = await _validator.ValidateAsync(request.Dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException(errorMessage);
        }

        // Validate pizza variant exists and is available
        var pizzaVariant = await _unitOfWork.PizzaVariants.GetByIdAsync(request.Dto.PizzaVariantId);
        if (pizzaVariant == null)
            throw new NotFoundException($"Pizza variant with ID '{request.Dto.PizzaVariantId}' not found");

        if (!pizzaVariant.IsAvailable)
            throw new PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException($"Pizza variant '{pizzaVariant.Size}' is not available");

        // Validate toppings exist and are available
        var toppingIds = request.Dto.ToppingIds.Where(id => !string.IsNullOrEmpty(id)).ToList();
        if (toppingIds.Count > 0)
        {
            var toppings = await _unitOfWork.Toppings.GetAllAsync();
            var toppingDict = toppings.ToDictionary(t => t.Id);

            foreach (var toppingId in toppingIds)
            {
                if (!toppingDict.TryGetValue(toppingId, out var topping))
                    throw new NotFoundException($"Topping with ID '{toppingId}' not found");

                if (!topping.IsAvailable)
                    throw new PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException($"Topping '{topping.Name}' is not available");
            }
        }

        // Get or create cart for user
        var cart = await _unitOfWork.Carts.GetOrCreateCartForUserAsync(userId);

        // Issue #9: Validate max cart size (20 items limit)
        if (cart.CartItems.Count >= 20)
        {
            throw new PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException("Cart cannot contain more than 20 items. Please remove some items before adding more.");
        }

        // Create new cart item (no auto-merge)
        var cartItem = new Domain.Entities.CartItem
        {
            Id = Guid.NewGuid().ToString(),
            CartId = cart.Id,
            PizzaVariantId = request.Dto.PizzaVariantId,
            Quantity = request.Dto.Quantity,
            SpecialInstructions = request.Dto.SpecialInstructions ?? string.Empty
        };

        // Add cart item to context (not saved yet)
        await _unitOfWork.CartItems.AddAsync(cartItem);

        // Add toppings to cart item if any
        if (toppingIds.Count > 0)
        {
            var cartItemToppings = toppingIds.Select(toppingId => new Domain.Entities.CartItemTopping
            {
                CartItemId = cartItem.Id,
                ToppingId = toppingId
            }).ToList();

            foreach (var topping in cartItemToppings)
            {
                await _unitOfWork.CartItemToppings.AddAsync(topping);
            }
        }
        
        // Save everything atomically (cart item + toppings in single transaction)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload cart item with related data
        var savedCartItem = await _unitOfWork.CartItems.GetCartItemWithDetailsAsync(cartItem.Id);
        if (savedCartItem == null)
            throw new NotFoundException($"Cart item with ID '{cartItem.Id}' not found after creation");

        return CartItemDto.FromEntity(savedCartItem);
    }
}
