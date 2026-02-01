using FluentAssertions;
using Moq;
using PizzaStore.Application.Features.Cart.Commands.AddPizzaToCart;
using PizzaStore.Application.Features.Cart.Commands.IncreaseCartItemQuantity;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;
using ValidationException = PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException;

namespace PizzaStore.Application.Tests.Features.Cart.Commands;

public class IncreaseCartItemQuantityCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly IncreaseCartItemQuantityCommandHandler _handler;

    public IncreaseCartItemQuantityCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cartRepositoryMock = new Mock<ICartRepository>();
        _cartItemRepositoryMock = new Mock<ICartItemRepository>();
        _currentUserServiceMock = MockCurrentUserServiceHelper.CreateAuthenticatedUser();
        
        _unitOfWorkMock.Setup(x => x.Carts).Returns(_cartRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.CartItems).Returns(_cartItemRepositoryMock.Object);
        
        _handler = new IncreaseCartItemQuantityCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAuthenticatedAndValid_IncreasesQuantity()
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
            .WithQuantity(3)
            .WithPizzaVariant(variant)
            .Build();

        var command = new IncreaseCartItemQuantityCommand(cartItemId, 2);

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
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        var unauthenticatedMock = MockCurrentUserServiceHelper.CreateUnauthenticatedUser();
        var handler = new IncreaseCartItemQuantityCommandHandler(
            _unitOfWorkMock.Object,
            unauthenticatedMock.Object);

        var command = new IncreaseCartItemQuantityCommand("cart-item-123", 1);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated");

        _cartItemRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<string>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAmountIsZero_ThrowsValidationException()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var command = new IncreaseCartItemQuantityCommand("cart-item-123", 0);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Amount must be greater than 0");

        _cartItemRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<string>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAmountIsNegative_ThrowsValidationException()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var command = new IncreaseCartItemQuantityCommand("cart-item-123", -5);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Amount must be greater than 0");

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

        var command = new IncreaseCartItemQuantityCommand(cartItemId, 1);

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
            .WithQuantity(2)
            .Build();

        var command = new IncreaseCartItemQuantityCommand(cartItemId, 3);

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
    public async Task Handle_WhenIncreasingByOne_IncrementsQuantityByOne()
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
            .WithQuantity(5)
            .WithPizzaVariant(variant)
            .Build();

        var command = new IncreaseCartItemQuantityCommand(cartItemId, 1);

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
        cartItem.Quantity.Should().Be(6);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenIncreasingByLargeAmount_AddsCorrectly()
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

        var command = new IncreaseCartItemQuantityCommand(cartItemId, 99);

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
        cartItem.Quantity.Should().Be(100);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
