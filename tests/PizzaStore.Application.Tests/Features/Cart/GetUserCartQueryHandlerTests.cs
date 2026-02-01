using FluentAssertions;
using Moq;
using PizzaStore.Application.Features.Queries.Cart.GetUserCart;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Tests.Features.Cart;

public class GetUserCartQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetUserCartQueryHandler _handler;

    public GetUserCartQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cartRepositoryMock = new Mock<ICartRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock.Setup(x => x.Carts).Returns(_cartRepositoryMock.Object);
        
        _handler = new GetUserCartQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndRequestsOwnCart_ReturnsCartDto()
    {
        // Arrange
        var userId = "user-123";
        var currentUserServiceMock = MockCurrentUserServiceHelper.CreateAuthenticatedUser(userId);

        var pizzaVariant = TestDataBuilder.PizzaVariant()
            .WithId("variant-1")
            .WithSize(PizzaSize.Large)
            .WithPrice(14.99m)
            .WithPizza(TestDataBuilder.Pizza()
                .WithId("pizza-1")
                .WithName("Margherita")
                .Build())
            .Build();

        var topping1 = TestDataBuilder.Topping()
            .WithId("topping-1")
            .WithName("Mushrooms")
            .WithPrice(1.50m)
            .Build();

        var topping2 = TestDataBuilder.Topping()
            .WithId("topping-2")
            .WithName("Extra Cheese")
            .WithPrice(2.00m)
            .Build();

        var cartItem = TestDataBuilder.CartItem()
            .WithId("item-1")
            .WithQuantity(2)
            .WithPizzaVariant(pizzaVariant)
            .WithToppings(topping1, topping2)
            .Build();

        var cart = TestDataBuilder.Cart()
            .WithId("cart-1")
            .WithUserId(userId)
            .WithCartItems(cartItem)
            .Build();

        _cartRepositoryMock
            .Setup(x => x.GetCartWithItemsByUserIdAsync(userId))
            .ReturnsAsync(cart);

        var handler = new GetUserCartQueryHandler(_unitOfWorkMock.Object, currentUserServiceMock.Object);
        var query = new GetUserCartQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("cart-1");
        result.UserId.Should().Be(userId);
        result.Items.Should().HaveCount(1);
        result.Items[0].Id.Should().Be("item-1");
        result.Items[0].PizzaName.Should().Be("Margherita");
        result.Items[0].Quantity.Should().Be(2);
        result.Items[0].Toppings.Should().HaveCount(2);
        
        _cartRepositoryMock.Verify(x => x.GetCartWithItemsByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedButCartDoesNotExist_ReturnsEmptyCartDto()
    {
        // Arrange
        var userId = "user-123";
        var currentUserServiceMock = MockCurrentUserServiceHelper.CreateAuthenticatedUser(userId);

        _cartRepositoryMock
            .Setup(x => x.GetCartWithItemsByUserIdAsync(userId))
            .ReturnsAsync((Domain.Entities.Cart?)null);

        var handler = new GetUserCartQueryHandler(_unitOfWorkMock.Object, currentUserServiceMock.Object);
        var query = new GetUserCartQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeEmpty();
        result.UserId.Should().Be(userId);
        result.Items.Should().BeEmpty();
        result.ItemCount.Should().Be(0);
        result.TotalQuantity.Should().Be(0);
        result.SubTotal.Should().Be(0);
        
        _cartRepositoryMock.Verify(x => x.GetCartWithItemsByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        var userId = "user-123";
        var currentUserServiceMock = MockCurrentUserServiceHelper.CreateUnauthenticatedUser();

        var handler = new GetUserCartQueryHandler(_unitOfWorkMock.Object, currentUserServiceMock.Object);
        var query = new GetUserCartQuery(userId);

        // Act
        var act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated");
        
        _cartRepositoryMock.Verify(x => x.GetCartWithItemsByUserIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserTriesToAccessAnotherUsersCart_ThrowsUnauthorizedException()
    {
        // Arrange
        var currentUserId = "user-123";
        var otherUserId = "user-456";
        var currentUserServiceMock = MockCurrentUserServiceHelper.CreateAuthenticatedUser(currentUserId);

        var handler = new GetUserCartQueryHandler(_unitOfWorkMock.Object, currentUserServiceMock.Object);
        var query = new GetUserCartQuery(otherUserId);

        // Act
        var act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("You do not have permission to view this cart");
        
        _cartRepositoryMock.Verify(x => x.GetCartWithItemsByUserIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCartHasMultipleItems_ReturnsAllItems()
    {
        // Arrange
        var userId = "user-123";
        var currentUserServiceMock = MockCurrentUserServiceHelper.CreateAuthenticatedUser(userId);

        var pizzaVariant1 = TestDataBuilder.PizzaVariant()
            .WithId("variant-1")
            .WithSize(PizzaSize.Large)
            .WithPrice(14.99m)
            .WithPizza(TestDataBuilder.Pizza()
                .WithId("pizza-1")
                .WithName("Margherita")
                .Build())
            .Build();

        var pizzaVariant2 = TestDataBuilder.PizzaVariant()
            .WithId("variant-2")
            .WithSize(PizzaSize.Medium)
            .WithPrice(12.99m)
            .WithPizza(TestDataBuilder.Pizza()
                .WithId("pizza-2")
                .WithName("Pepperoni")
                .Build())
            .Build();

        var cartItem1 = TestDataBuilder.CartItem()
            .WithId("item-1")
            .WithQuantity(1)
            .WithPizzaVariant(pizzaVariant1)
            .Build();

        var cartItem2 = TestDataBuilder.CartItem()
            .WithId("item-2")
            .WithQuantity(3)
            .WithPizzaVariant(pizzaVariant2)
            .Build();

        var cart = TestDataBuilder.Cart()
            .WithId("cart-1")
            .WithUserId(userId)
            .WithCartItems(cartItem1, cartItem2)
            .Build();

        _cartRepositoryMock
            .Setup(x => x.GetCartWithItemsByUserIdAsync(userId))
            .ReturnsAsync(cart);

        var handler = new GetUserCartQueryHandler(_unitOfWorkMock.Object, currentUserServiceMock.Object);
        var query = new GetUserCartQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.ItemCount.Should().Be(2);
        result.TotalQuantity.Should().Be(4);
        result.Items[0].PizzaName.Should().Be("Margherita");
        result.Items[1].PizzaName.Should().Be("Pepperoni");
    }

    [Fact]
    public async Task Handle_WhenCartHasNoItems_ReturnsEmptyItemsList()
    {
        // Arrange
        var userId = "user-123";
        var currentUserServiceMock = MockCurrentUserServiceHelper.CreateAuthenticatedUser(userId);

        var cart = TestDataBuilder.Cart()
            .WithId("cart-1")
            .WithUserId(userId)
            .WithCartItems()
            .Build();

        _cartRepositoryMock
            .Setup(x => x.GetCartWithItemsByUserIdAsync(userId))
            .ReturnsAsync(cart);

        var handler = new GetUserCartQueryHandler(_unitOfWorkMock.Object, currentUserServiceMock.Object);
        var query = new GetUserCartQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("cart-1");
        result.Items.Should().BeEmpty();
        result.ItemCount.Should().Be(0);
        result.TotalQuantity.Should().Be(0);
    }
}
