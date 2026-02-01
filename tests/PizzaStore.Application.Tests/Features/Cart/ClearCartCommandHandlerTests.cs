using FluentAssertions;
using Moq;
using PizzaStore.Application.Features.Commands.Cart.ClearCart;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Tests.Features.Cart;

public class ClearCartCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly ClearCartCommandHandler _handler;

    public ClearCartCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cartItemRepositoryMock = new Mock<ICartItemRepository>();
        _cartRepositoryMock = new Mock<ICartRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _unitOfWorkMock.Setup(x => x.CartItems).Returns(_cartItemRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Carts).Returns(_cartRepositoryMock.Object);
        
        _handler = new ClearCartCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticated_AndCartHasItems_ClearsAllItemsAndReturnsSuccess()
    {
        // Arrange
        var userId = "user-123";
        var cartId = "cart-456";
        
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);

        var cartItem1 = TestDataBuilder.CartItem().WithId("item-1").WithCartId(cartId).Build();
        var cartItem2 = TestDataBuilder.CartItem().WithId("item-2").WithCartId(cartId).Build();
        var cartItem3 = TestDataBuilder.CartItem().WithId("item-3").WithCartId(cartId).Build();

        var cart = TestDataBuilder.Cart()
            .WithId(cartId)
            .WithUserId(userId)
            .WithCartItems(cartItem1, cartItem2, cartItem3)
            .Build();

        var command = new ClearCartCommand();

        _cartRepositoryMock
            .Setup(x => x.GetCartWithItemsByUserIdAsync(userId))
            .ReturnsAsync(cart);

        _cartItemRepositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<CartItem>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ItemsRemoved.Should().Be(3);
        result.Message.Should().Contain("Cart cleared successfully");
        result.Message.Should().Contain("3 item(s) removed");

        _cartItemRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<CartItem>()), Times.Exactly(3));
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns((string?)null);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(false);

        var command = new ClearCartCommand();

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated");

        _cartRepositoryMock.Verify(
            x => x.GetCartWithItemsByUserIdAsync(It.IsAny<string>()), 
            Times.Never);
        _cartItemRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<CartItem>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCartIsEmpty_ReturnsSuccessWithZeroItemsRemoved()
    {
        // Arrange
        var userId = "user-123";
        
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);

        var command = new ClearCartCommand();

        _cartRepositoryMock
            .Setup(x => x.GetCartWithItemsByUserIdAsync(userId))
            .ReturnsAsync((Domain.Entities.Cart?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ItemsRemoved.Should().Be(0);
        result.Message.Should().Be("Cart is already empty");

        _cartItemRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<CartItem>()), 
            Times.Never);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCartExistsButHasNoItems_ReturnsSuccessWithZeroItemsRemoved()
    {
        // Arrange
        var userId = "user-123";
        var cartId = "cart-456";
        
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);

        var cart = TestDataBuilder.Cart()
            .WithId(cartId)
            .WithUserId(userId)
            .WithCartItems()
            .Build();

        var command = new ClearCartCommand();

        _cartRepositoryMock
            .Setup(x => x.GetCartWithItemsByUserIdAsync(userId))
            .ReturnsAsync(cart);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ItemsRemoved.Should().Be(0);
        result.Message.Should().Contain("0 item(s) removed");

        _cartItemRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<CartItem>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCartHasSingleItem_ClearsSuccessfully()
    {
        // Arrange
        var userId = "user-123";
        var cartId = "cart-456";
        
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);

        var cartItem = TestDataBuilder.CartItem().WithId("item-1").WithCartId(cartId).Build();

        var cart = TestDataBuilder.Cart()
            .WithId(cartId)
            .WithUserId(userId)
            .WithCartItems(cartItem)
            .Build();

        var command = new ClearCartCommand();

        _cartRepositoryMock
            .Setup(x => x.GetCartWithItemsByUserIdAsync(userId))
            .ReturnsAsync(cart);

        _cartItemRepositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<CartItem>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ItemsRemoved.Should().Be(1);
        result.Message.Should().Contain("1 item(s) removed");
        _cartItemRepositoryMock.Verify(x => x.DeleteAsync(cartItem), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCartHasItemsWithToppings_ClearsAllSuccessfully()
    {
        // Arrange
        var userId = "user-123";
        var cartId = "cart-456";
        
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);

        var topping1 = TestDataBuilder.Topping().WithName("Extra Cheese").Build();
        var topping2 = TestDataBuilder.Topping().WithName("Pepperoni").Build();

        var cartItem1 = TestDataBuilder.CartItem()
            .WithId("item-1")
            .WithCartId(cartId)
            .WithToppings(topping1, topping2)
            .Build();

        var cartItem2 = TestDataBuilder.CartItem()
            .WithId("item-2")
            .WithCartId(cartId)
            .WithToppings(topping1)
            .Build();

        var cart = TestDataBuilder.Cart()
            .WithId(cartId)
            .WithUserId(userId)
            .WithCartItems(cartItem1, cartItem2)
            .Build();

        var command = new ClearCartCommand();

        _cartRepositoryMock
            .Setup(x => x.GetCartWithItemsByUserIdAsync(userId))
            .ReturnsAsync(cart);

        _cartItemRepositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<CartItem>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ItemsRemoved.Should().Be(2);
        result.Success.Should().BeTrue();
        _cartItemRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<CartItem>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WhenClearingMultipleTimes_EachTimeIsHandledCorrectly()
    {
        // Arrange
        var userId = "user-123";
        var cartId = "cart-456";
        
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);

        var cartItem = TestDataBuilder.CartItem().WithId("item-1").WithCartId(cartId).Build();

        var cart = TestDataBuilder.Cart()
            .WithId(cartId)
            .WithUserId(userId)
            .WithCartItems(cartItem)
            .Build();

        var command1 = new ClearCartCommand();
        var command2 = new ClearCartCommand();

        // First call: cart has items
        _cartRepositoryMock
            .Setup(x => x.GetCartWithItemsByUserIdAsync(userId))
            .ReturnsAsync(cart);

        _cartItemRepositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<CartItem>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act - First clear
        var result1 = await _handler.Handle(command1, CancellationToken.None);

        // Setup for second call: cart is now empty
        _cartRepositoryMock
            .Setup(x => x.GetCartWithItemsByUserIdAsync(userId))
            .ReturnsAsync((Domain.Entities.Cart?)null);

        // Act - Second clear
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.ItemsRemoved.Should().Be(1);
        result1.Success.Should().BeTrue();
        
        result2.ItemsRemoved.Should().Be(0);
        result2.Success.Should().BeTrue();
        result2.Message.Should().Be("Cart is already empty");
    }
}
