using FluentAssertions;
using Moq;
using PizzaStore.Application.Features.Cart.Commands.RemoveCartItem;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Tests.Features.Cart.Commands;

public class RemoveCartItemCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly RemoveCartItemCommandHandler _handler;

    public RemoveCartItemCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cartItemRepositoryMock = new Mock<ICartItemRepository>();
        _cartRepositoryMock = new Mock<ICartRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _unitOfWorkMock.Setup(x => x.CartItems).Returns(_cartItemRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Carts).Returns(_cartRepositoryMock.Object);
        
        _handler = new RemoveCartItemCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticated_AndOwnsCartItem_RemovesAndReturnsSuccess()
    {
        // Arrange
        var userId = "user-123";
        var cartItemId = "cart-item-456";
        
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);

        var cartItem = TestDataBuilder.CartItem()
            .WithId(cartItemId)
            .WithQuantity(2)
            .Build();

        var command = new RemoveCartItemCommand(cartItemId);

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
        result.Message.Should().Contain("removed successfully");

        _cartItemRepositoryMock.Verify(x => x.DeleteAsync(cartItem), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        var cartItemId = "cart-item-456";
        
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns((string?)null);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(false);

        var command = new RemoveCartItemCommand(cartItemId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated");

        _cartItemRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<string>()), 
            Times.Never);
        _cartItemRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<CartItem>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCartItemDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "user-123";
        var cartItemId = "non-existent-item";
        
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);

        var command = new RemoveCartItemCommand(cartItemId);

        _cartItemRepositoryMock
            .Setup(x => x.GetByIdAsync(cartItemId))
            .ReturnsAsync((CartItem?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Cart item with ID '{cartItemId}' not found");

        _cartRepositoryMock.Verify(
            x => x.IsCartItemOwnedByUserAsync(It.IsAny<string>(), It.IsAny<string>()), 
            Times.Never);
        _cartItemRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<CartItem>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotOwnCartItem_ThrowsUnauthorizedException()
    {
        // Arrange
        var userId = "user-123";
        var cartItemId = "cart-item-789";
        
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);

        var cartItem = TestDataBuilder.CartItem()
            .WithId(cartItemId)
            .WithQuantity(1)
            .Build();

        var command = new RemoveCartItemCommand(cartItemId);

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
            .WithMessage("You do not have permission to remove this cart item");

        _cartItemRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<CartItem>()), 
            Times.Never);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRemovingItemWithToppings_SuccessfullyRemoves()
    {
        // Arrange
        var userId = "user-123";
        var cartItemId = "cart-item-456";
        
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);

        var topping1 = TestDataBuilder.Topping().WithName("Cheese").Build();
        var topping2 = TestDataBuilder.Topping().WithName("Pepperoni").Build();

        var cartItem = TestDataBuilder.CartItem()
            .WithId(cartItemId)
            .WithQuantity(1)
            .WithToppings(topping1, topping2)
            .Build();

        var command = new RemoveCartItemCommand(cartItemId);

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
        result.Success.Should().BeTrue();
        _cartItemRepositoryMock.Verify(x => x.DeleteAsync(cartItem), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRemovingMultipleItemsSequentially_EachIsHandledIndependently()
    {
        // Arrange
        var userId = "user-123";
        var cartItemId1 = "cart-item-1";
        var cartItemId2 = "cart-item-2";
        
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);

        var cartItem1 = TestDataBuilder.CartItem().WithId(cartItemId1).Build();
        var cartItem2 = TestDataBuilder.CartItem().WithId(cartItemId2).Build();

        _cartItemRepositoryMock
            .Setup(x => x.GetByIdAsync(cartItemId1))
            .ReturnsAsync(cartItem1);

        _cartItemRepositoryMock
            .Setup(x => x.GetByIdAsync(cartItemId2))
            .ReturnsAsync(cartItem2);

        _cartRepositoryMock
            .Setup(x => x.IsCartItemOwnedByUserAsync(It.IsAny<string>(), userId))
            .ReturnsAsync(true);

        _cartItemRepositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<CartItem>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command1 = new RemoveCartItemCommand(cartItemId1);
        var command2 = new RemoveCartItemCommand(cartItemId2);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.Success.Should().BeTrue();
        result2.Success.Should().BeTrue();
        _cartItemRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<CartItem>()), Times.Exactly(2));
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
