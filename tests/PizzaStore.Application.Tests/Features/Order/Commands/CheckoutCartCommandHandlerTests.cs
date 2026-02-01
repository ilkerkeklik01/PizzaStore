using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PizzaStore.Application.Features.Order.Commands.CheckoutCart;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Tests.Features.Order.Commands;

public class CheckoutCartCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<CheckoutCartCommandHandler>> _loggerMock;
    private readonly CheckoutCartCommandHandler _handler;

    public CheckoutCartCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cartRepositoryMock = new Mock<ICartRepository>();
        _cartItemRepositoryMock = new Mock<ICartItemRepository>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _currentUserServiceMock = MockCurrentUserServiceHelper.CreateAuthenticatedUser();
        _loggerMock = new Mock<ILogger<CheckoutCartCommandHandler>>();

        _unitOfWorkMock.Setup(x => x.Carts).Returns(_cartRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.CartItems).Returns(_cartItemRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Orders).Returns(_orderRepositoryMock.Object);

        _handler = new CheckoutCartCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidCart_CreatesOrderAndClearsCart()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var pizza = TestDataBuilder.Pizza().WithName("Margherita").Build();
        var pizzaVariant = TestDataBuilder.PizzaVariant()
            .WithId("variant-123")
            .WithPizza(pizza)
            .WithSize(PizzaSize.Medium)
            .WithPrice(10.00m)
            .IsAvailable(true)
            .Build();

        var cartItem = TestDataBuilder.CartItem()
            .WithId("cart-item-1")
            .WithPizzaVariant(pizzaVariant)
            .WithQuantity(1)
            .Build();

        var cart = TestDataBuilder.Cart()
            .WithUserId(userId)
            .WithCartItems(cartItem)
            .Build();

        var command = new CheckoutCartCommand();

        _cartRepositoryMock
            .Setup(x => x.GetCartWithItemsByUserIdAsync(userId))
            .ReturnsAsync(cart);

        Domain.Entities.Order? capturedOrder = null;
        _orderRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Order>()))
            .Callback<Domain.Entities.Order>(order => capturedOrder = order)
            .Returns(Task.CompletedTask);

        _orderRepositoryMock
            .Setup(x => x.GetOrderByIdWithDetailsAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => capturedOrder!);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>(null!));

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Status.Should().Be(OrderStatus.Pending);
        result.TotalPrice.Should().Be(10.00m);

        capturedOrder.Should().NotBeNull();
        capturedOrder!.UserId.Should().Be(userId);
        capturedOrder.TotalPrice.Should().Be(10.00m);
        capturedOrder.Status.Should().Be(OrderStatus.Pending);
        capturedOrder.OrderItems.Should().HaveCount(1);

        _cartItemRepositoryMock.Verify(x => x.DeleteAsync(cartItem), Times.Once);
        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMultipleItemsWithToppings_CreatesCompleteOrder()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var pizza1 = TestDataBuilder.Pizza().WithName("Pepperoni").Build();
        var pizzaVariant1 = TestDataBuilder.PizzaVariant()
            .WithId("variant-1")
            .WithPizza(pizza1)
            .WithSize(PizzaSize.Large)
            .WithPrice(12.00m)
            .IsAvailable(true)
            .Build();

        var pizza2 = TestDataBuilder.Pizza().WithName("Vegetarian").Build();
        var pizzaVariant2 = TestDataBuilder.PizzaVariant()
            .WithId("variant-2")
            .WithPizza(pizza2)
            .WithSize(PizzaSize.Medium)
            .WithPrice(8.00m)
            .IsAvailable(true)
            .Build();

        var topping1 = TestDataBuilder.Topping()
            .WithId("topping-1")
            .WithName("Mushrooms")
            .WithPrice(1.50m)
            .IsAvailable(true)
            .Build();

        var topping2 = TestDataBuilder.Topping()
            .WithId("topping-2")
            .WithName("Olives")
            .WithPrice(1.00m)
            .IsAvailable(true)
            .Build();

        var cartItem1 = TestDataBuilder.CartItem()
            .WithId("cart-item-1")
            .WithPizzaVariant(pizzaVariant1)
            .WithQuantity(2)
            .WithToppings(topping1, topping2)
            .Build();

        var cartItem2 = TestDataBuilder.CartItem()
            .WithId("cart-item-2")
            .WithPizzaVariant(pizzaVariant2)
            .WithQuantity(1)
            .WithToppings(topping1)
            .Build();

        var cart = TestDataBuilder.Cart()
            .WithUserId(userId)
            .WithCartItems(cartItem1, cartItem2)
            .Build();

        var command = new CheckoutCartCommand();

        _cartRepositoryMock
            .Setup(x => x.GetCartWithItemsByUserIdAsync(userId))
            .ReturnsAsync(cart);

        Domain.Entities.Order? capturedOrder = null;
        _orderRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Order>()))
            .Callback<Domain.Entities.Order>(order => capturedOrder = order)
            .Returns(Task.CompletedTask);

        _orderRepositoryMock
            .Setup(x => x.GetOrderByIdWithDetailsAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => capturedOrder!);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>(null!));

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        
        // Item 1: (12.00 + 1.50 + 1.00) * 2 = 29.00
        // Item 2: (8.00 + 1.50) * 1 = 9.50
        // Total: 38.50
        result.TotalPrice.Should().Be(38.50m);
        
        capturedOrder.Should().NotBeNull();
        capturedOrder!.OrderItems.Should().HaveCount(2);
        
        var orderItem1 = capturedOrder.OrderItems.First(oi => oi.PizzaVariantId == "variant-1");
        orderItem1.Quantity.Should().Be(2);
        orderItem1.SubtotalAtOrder.Should().Be(29.00m);
        orderItem1.OrderItemToppings.Should().HaveCount(2);

        var orderItem2 = capturedOrder.OrderItems.First(oi => oi.PizzaVariantId == "variant-2");
        orderItem2.Quantity.Should().Be(1);
        orderItem2.SubtotalAtOrder.Should().Be(9.50m);
        orderItem2.OrderItemToppings.Should().HaveCount(1);

        _cartItemRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<CartItem>()), Times.Exactly(2));
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns((string?)null);
        var command = new CheckoutCartCommand();

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated");
    }

    [Fact]
    public async Task Handle_WhenCartEmpty_ThrowsValidationException()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var cart = TestDataBuilder.Cart()
            .WithUserId(userId)
            .WithCartItems()
            .Build();

        var command = new CheckoutCartCommand();

        _cartRepositoryMock
            .Setup(x => x.GetCartWithItemsByUserIdAsync(userId))
            .ReturnsAsync(cart);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Cart is empty. Cannot proceed with checkout");
    }

    [Fact]
    public async Task Handle_WhenMinimumOrderNotMet_ThrowsValidationException()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var pizza = TestDataBuilder.Pizza().WithName("Cheap Pizza").Build();
        var pizzaVariant = TestDataBuilder.PizzaVariant()
            .WithId("variant-123")
            .WithPizza(pizza)
            .WithSize(PizzaSize.Small)
            .WithPrice(3.00m)
            .IsAvailable(true)
            .Build();

        var cartItem = TestDataBuilder.CartItem()
            .WithId("cart-item-1")
            .WithPizzaVariant(pizzaVariant)
            .WithQuantity(1)
            .Build();

        var cart = TestDataBuilder.Cart()
            .WithUserId(userId)
            .WithCartItems(cartItem)
            .Build();

        var command = new CheckoutCartCommand();

        _cartRepositoryMock
            .Setup(x => x.GetCartWithItemsByUserIdAsync(userId))
            .ReturnsAsync(cart);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Minimum order total is $5.00. Your cart total is $3.00.");
    }

    [Fact]
    public async Task Handle_WhenPizzaVariantUnavailable_ThrowsValidationException()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var pizza = TestDataBuilder.Pizza().WithName("Discontinued").Build();
        var pizzaVariant = TestDataBuilder.PizzaVariant()
            .WithId("variant-123")
            .WithPizza(pizza)
            .WithSize(PizzaSize.Large)
            .WithPrice(15.00m)
            .IsAvailable(false)
            .Build();

        var cartItem = TestDataBuilder.CartItem()
            .WithId("cart-item-1")
            .WithPizzaVariant(pizzaVariant)
            .WithQuantity(1)
            .Build();

        var cart = TestDataBuilder.Cart()
            .WithUserId(userId)
            .WithCartItems(cartItem)
            .Build();

        var command = new CheckoutCartCommand();

        _cartRepositoryMock
            .Setup(x => x.GetCartWithItemsByUserIdAsync(userId))
            .ReturnsAsync(cart);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Pizza variant 'Discontinued (Large)' is no longer available.");
    }

    [Fact]
    public async Task Handle_WhenToppingUnavailable_ThrowsValidationException()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var pizza = TestDataBuilder.Pizza().WithName("Pepperoni").Build();
        var pizzaVariant = TestDataBuilder.PizzaVariant()
            .WithId("variant-123")
            .WithPizza(pizza)
            .WithSize(PizzaSize.Medium)
            .WithPrice(10.00m)
            .IsAvailable(true)
            .Build();

        var topping = TestDataBuilder.Topping()
            .WithId("topping-1")
            .WithName("Out of Stock Topping")
            .WithPrice(1.50m)
            .IsAvailable(false)
            .Build();

        var cartItem = TestDataBuilder.CartItem()
            .WithId("cart-item-1")
            .WithPizzaVariant(pizzaVariant)
            .WithQuantity(1)
            .WithToppings(topping)
            .Build();

        var cart = TestDataBuilder.Cart()
            .WithUserId(userId)
            .WithCartItems(cartItem)
            .Build();

        var command = new CheckoutCartCommand();

        _cartRepositoryMock
            .Setup(x => x.GetCartWithItemsByUserIdAsync(userId))
            .ReturnsAsync(cart);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Topping 'Out of Stock Topping' is no longer available.");
    }

    [Fact]
    public async Task Handle_WhenCartNotFound_ThrowsValidationException()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        var command = new CheckoutCartCommand();

        _cartRepositoryMock
            .Setup(x => x.GetCartWithItemsByUserIdAsync(userId))
            .ReturnsAsync((Domain.Entities.Cart?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Cart is empty. Cannot proceed with checkout");
    }

    [Fact]
    public async Task Handle_WhenTransactionFails_RollsBackChanges()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var pizza = TestDataBuilder.Pizza().Build();
        var pizzaVariant = TestDataBuilder.PizzaVariant()
            .WithId("variant-123")
            .WithPizza(pizza)
            .WithPrice(10.00m)
            .IsAvailable(true)
            .Build();

        var cartItem = TestDataBuilder.CartItem()
            .WithId("cart-item-1")
            .WithPizzaVariant(pizzaVariant)
            .WithQuantity(1)
            .Build();

        var cart = TestDataBuilder.Cart()
            .WithUserId(userId)
            .WithCartItems(cartItem)
            .Build();

        var command = new CheckoutCartCommand();

        _cartRepositoryMock
            .Setup(x => x.GetCartWithItemsByUserIdAsync(userId))
            .ReturnsAsync(cart);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>(null!));

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        _unitOfWorkMock
            .Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");

        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
