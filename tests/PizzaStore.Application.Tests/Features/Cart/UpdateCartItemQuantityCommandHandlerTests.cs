using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using PizzaStore.Application.Features.Commands.Cart.AddPizzaToCart;
using PizzaStore.Application.Features.Commands.Cart.UpdateCartItemQuantity;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;
using ValidationException = PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException;

namespace PizzaStore.Application.Tests.Features.Cart;

public class UpdateCartItemQuantityCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;
    private readonly Mock<IValidator<UpdateCartItemQuantityDto>> _validatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdateCartItemQuantityCommandHandler _handler;

    public UpdateCartItemQuantityCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cartRepositoryMock = new Mock<ICartRepository>();
        _cartItemRepositoryMock = new Mock<ICartItemRepository>();
        _validatorMock = new Mock<IValidator<UpdateCartItemQuantityDto>>();
        _currentUserServiceMock = MockCurrentUserServiceHelper.CreateAuthenticatedUser();
        
        _unitOfWorkMock.Setup(x => x.Carts).Returns(_cartRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.CartItems).Returns(_cartItemRepositoryMock.Object);
        
        _handler = new UpdateCartItemQuantityCommandHandler(
            _unitOfWorkMock.Object,
            _validatorMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAuthenticatedAndValid_UpdatesQuantity()
    {
        // Arrange
        var userId = "test-user-id";
        var cartItemId = "cart-item-123";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var pizza = TestDataBuilder.Pizza()
            .WithName("Margherita")
            .Build();

        var variant = TestDataBuilder.PizzaVariant()
            .WithSize(PizzaSize.Medium)
            .WithPrice(12.99m)
            .WithPizza(pizza)
            .Build();

        var cartItem = TestDataBuilder.CartItem()
            .WithId(cartItemId)
            .WithQuantity(1)
            .WithPizzaVariant(variant)
            .Build();

        var dto = new UpdateCartItemQuantityDto
        {
            CartItemId = cartItemId,
            Quantity = 5,
            SpecialInstructions = "Extra cheese"
        };
        var command = new UpdateCartItemQuantityCommand(dto);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _cartItemRepositoryMock
            .Setup(x => x.GetByIdAsync(cartItemId))
            .ReturnsAsync(cartItem);

        _cartRepositoryMock
            .Setup(x => x.IsCartItemOwnedByUserAsync(cartItemId, userId))
            .ReturnsAsync(true);

        _cartItemRepositoryMock
            .Setup(x => x.GetCartItemWithDetailsAsync(cartItemId))
            .ReturnsAsync(cartItem);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        cartItem.Quantity.Should().Be(5);
        cartItem.SpecialInstructions.Should().Be("Extra cheese");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        var unauthenticatedMock = MockCurrentUserServiceHelper.CreateUnauthenticatedUser();
        var handler = new UpdateCartItemQuantityCommandHandler(
            _unitOfWorkMock.Object,
            _validatorMock.Object,
            unauthenticatedMock.Object);

        var dto = new UpdateCartItemQuantityDto
        {
            CartItemId = "cart-item-123",
            Quantity = 5
        };
        var command = new UpdateCartItemQuantityCommand(dto);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated");

        _validatorMock.Verify(
            x => x.ValidateAsync(It.IsAny<UpdateCartItemQuantityDto>(), It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ThrowsValidationException()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var dto = new UpdateCartItemQuantityDto
        {
            CartItemId = "",
            Quantity = 0
        };
        var command = new UpdateCartItemQuantityCommand(dto);

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("CartItemId", "Cart item ID is required"),
            new ValidationFailure("Quantity", "Quantity must be greater than 0")
        };
        var validationResult = new ValidationResult(validationFailures);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Cart item ID is required*Quantity must be greater than 0*");

        _cartItemRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<string>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCartItemNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-id";
        var cartItemId = "non-existent-item";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var dto = new UpdateCartItemQuantityDto
        {
            CartItemId = cartItemId,
            Quantity = 5
        };
        var command = new UpdateCartItemQuantityCommand(dto);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _cartItemRepositoryMock
            .Setup(x => x.GetByIdAsync(cartItemId))
            .ReturnsAsync((CartItem?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Cart item with ID '{cartItemId}' not found");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNotOwner_ThrowsUnauthorizedException()
    {
        // Arrange
        var userId = "test-user-id";
        var cartItemId = "cart-item-123";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var cartItem = TestDataBuilder.CartItem()
            .WithId(cartItemId)
            .WithQuantity(1)
            .Build();

        var dto = new UpdateCartItemQuantityDto
        {
            CartItemId = cartItemId,
            Quantity = 5
        };
        var command = new UpdateCartItemQuantityCommand(dto);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _cartItemRepositoryMock
            .Setup(x => x.GetByIdAsync(cartItemId))
            .ReturnsAsync(cartItem);

        _cartRepositoryMock
            .Setup(x => x.IsCartItemOwnedByUserAsync(cartItemId, userId))
            .ReturnsAsync(false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("You do not have permission to update this cart item");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUpdatingWithoutSpecialInstructions_OnlyUpdatesQuantity()
    {
        // Arrange
        var userId = "test-user-id";
        var cartItemId = "cart-item-123";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var pizza = TestDataBuilder.Pizza()
            .WithName("Margherita")
            .Build();

        var variant = TestDataBuilder.PizzaVariant()
            .WithSize(PizzaSize.Medium)
            .WithPrice(12.99m)
            .WithPizza(pizza)
            .Build();

        var cartItem = TestDataBuilder.CartItem()
            .WithId(cartItemId)
            .WithQuantity(2)
            .WithSpecialInstructions("Old instructions")
            .WithPizzaVariant(variant)
            .Build();

        var dto = new UpdateCartItemQuantityDto
        {
            CartItemId = cartItemId,
            Quantity = 10,
            SpecialInstructions = null
        };
        var command = new UpdateCartItemQuantityCommand(dto);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _cartItemRepositoryMock
            .Setup(x => x.GetByIdAsync(cartItemId))
            .ReturnsAsync(cartItem);

        _cartRepositoryMock
            .Setup(x => x.IsCartItemOwnedByUserAsync(cartItemId, userId))
            .ReturnsAsync(true);

        _cartItemRepositoryMock
            .Setup(x => x.GetCartItemWithDetailsAsync(cartItemId))
            .ReturnsAsync(cartItem);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        cartItem.Quantity.Should().Be(10);
        cartItem.SpecialInstructions.Should().Be("Old instructions");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
