using FluentAssertions;
using Moq;
using PizzaStore.Application.Features.Commands.Cart.DecreaseCartItemQuantity;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;
using ValidationException = PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException;

namespace PizzaStore.Application.Tests.Features.Cart;

public class DecreaseCartItemQuantityCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly DecreaseCartItemQuantityCommandHandler _handler;

    public DecreaseCartItemQuantityCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cartRepositoryMock = new Mock<ICartRepository>();
        _cartItemRepositoryMock = new Mock<ICartItemRepository>();
        _currentUserServiceMock = MockCurrentUserServiceHelper.CreateAuthenticatedUser();
        
        _unitOfWorkMock.Setup(x => x.Carts).Returns(_cartRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.CartItems).Returns(_cartItemRepositoryMock.Object);
        
        _handler = new DecreaseCartItemQuantityCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAuthenticatedAndValidAndResultGreaterThanZero_DecreasesQuantity()
    {
        // Arrange
        var userId = "test-user-id";
        var cartItemId = "cart-item-123";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var cartItem = TestDataBuilder.CartItem()
            .WithId(cartItemId)
            .WithQuantity(5)
            .Build();

        var command = new DecreaseCartItemQuantityCommand(cartItemId, 2);

        _cartItemRepositoryMock
            .Setup(x => x.GetByIdAsync(cartItemId))
            .ReturnsAsync(cartItem);

        _cartRepositoryMock
            .Setup(x => x.IsCartItemOwnedByUserAsync(cartItemId, userId))
            .ReturnsAsync(true);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ItemRemoved.Should().BeFalse();
        result.Message.Should().Contain("decreased successfully");

        cartItem.Quantity.Should().Be(3);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cartItemRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<CartItem>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenResultQuantityWouldBeZero_RemovesCartItem()
    {
        // Arrange
        var userId = "test-user-id";
        var cartItemId = "cart-item-123";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var cartItem = TestDataBuilder.CartItem()
            .WithId(cartItemId)
            .WithQuantity(3)
            .Build();

        var command = new DecreaseCartItemQuantityCommand(cartItemId, 3);

        _cartItemRepositoryMock
            .Setup(x => x.GetByIdAsync(cartItemId))
            .ReturnsAsync(cartItem);

        _cartRepositoryMock
            .Setup(x => x.IsCartItemOwnedByUserAsync(cartItemId, userId))
            .ReturnsAsync(true);

        _cartItemRepositoryMock
            .Setup(x => x.DeleteAsync(cartItem))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ItemRemoved.Should().BeTrue();
        result.Message.Should().Contain("removed");
        result.Message.Should().Contain("zero or negative");

        _cartItemRepositoryMock.Verify(x => x.DeleteAsync(cartItem), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenResultQuantityWouldBeNegative_RemovesCartItem()
    {
        // Arrange
        var userId = "test-user-id";
        var cartItemId = "cart-item-123";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var cartItem = TestDataBuilder.CartItem()
            .WithId(cartItemId)
            .WithQuantity(2)
            .Build();

        var command = new DecreaseCartItemQuantityCommand(cartItemId, 5);

        _cartItemRepositoryMock
            .Setup(x => x.GetByIdAsync(cartItemId))
            .ReturnsAsync(cartItem);

        _cartRepositoryMock
            .Setup(x => x.IsCartItemOwnedByUserAsync(cartItemId, userId))
            .ReturnsAsync(true);

        _cartItemRepositoryMock
            .Setup(x => x.DeleteAsync(cartItem))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ItemRemoved.Should().BeTrue();
        _cartItemRepositoryMock.Verify(x => x.DeleteAsync(cartItem), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        var unauthenticatedMock = MockCurrentUserServiceHelper.CreateUnauthenticatedUser();
        var handler = new DecreaseCartItemQuantityCommandHandler(
            _unitOfWorkMock.Object,
            unauthenticatedMock.Object);

        var command = new DecreaseCartItemQuantityCommand("cart-item-123", 1);

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

        var command = new DecreaseCartItemQuantityCommand("cart-item-123", 0);

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

        var command = new DecreaseCartItemQuantityCommand("cart-item-123", -3);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Amount must be greater than 0");
    }

    [Fact]
    public async Task Handle_WhenCartItemNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-id";
        var cartItemId = "non-existent-item";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var command = new DecreaseCartItemQuantityCommand(cartItemId, 1);

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
            .WithQuantity(5)
            .Build();

        var command = new DecreaseCartItemQuantityCommand(cartItemId, 1);

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
    public async Task Handle_WhenDecreasingByOne_DecrementsQuantityByOne()
    {
        // Arrange
        var userId = "test-user-id";
        var cartItemId = "cart-item-123";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var cartItem = TestDataBuilder.CartItem()
            .WithId(cartItemId)
            .WithQuantity(10)
            .Build();

        var command = new DecreaseCartItemQuantityCommand(cartItemId, 1);

        _cartItemRepositoryMock
            .Setup(x => x.GetByIdAsync(cartItemId))
            .ReturnsAsync(cartItem);

        _cartRepositoryMock
            .Setup(x => x.IsCartItemOwnedByUserAsync(cartItemId, userId))
            .ReturnsAsync(true);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        cartItem.Quantity.Should().Be(9);
        result.ItemRemoved.Should().BeFalse();
    }
}
