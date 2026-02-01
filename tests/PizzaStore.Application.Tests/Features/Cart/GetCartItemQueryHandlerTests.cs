using FluentAssertions;
using Moq;
using PizzaStore.Application.Features.Queries.Cart.GetCartItem;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Tests.Features.Cart;

public class GetCartItemQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetCartItemQueryHandler _handler;

    public GetCartItemQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cartItemRepositoryMock = new Mock<ICartItemRepository>();
        _cartRepositoryMock = new Mock<ICartRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock.Setup(x => x.CartItems).Returns(_cartItemRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Carts).Returns(_cartRepositoryMock.Object);
        
        _handler = new GetCartItemQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndOwnsCartItem_ReturnsCartItemDto()
    {
        // Arrange
        var userId = "user-123";
        var cartItemId = "item-1";
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

        var topping = TestDataBuilder.Topping()
            .WithId("topping-1")
            .WithName("Mushrooms")
            .WithPrice(1.50m)
            .Build();

        var cartItem = TestDataBuilder.CartItem()
            .WithId(cartItemId)
            .WithCartId("cart-1")
            .WithQuantity(2)
            .WithSpecialInstructions("Extra crispy")
            .WithPizzaVariant(pizzaVariant)
            .WithToppings(topping)
            .Build();

        _cartItemRepositoryMock
            .Setup(x => x.GetCartItemWithDetailsAsync(cartItemId))
            .ReturnsAsync(cartItem);

        _cartRepositoryMock
            .Setup(x => x.IsCartItemOwnedByUserAsync(cartItemId, userId))
            .ReturnsAsync(true);

        var handler = new GetCartItemQueryHandler(_unitOfWorkMock.Object, currentUserServiceMock.Object);
        var query = new GetCartItemQuery(cartItemId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(cartItemId);
        result.CartId.Should().Be("cart-1");
        result.PizzaName.Should().Be("Margherita");
        result.PizzaVariantName.Should().Be("Large");
        result.Quantity.Should().Be(2);
        result.SpecialInstructions.Should().Be("Extra crispy");
        result.BasePrice.Should().Be(14.99m);
        result.Toppings.Should().HaveCount(1);
        result.Toppings[0].ToppingName.Should().Be("Mushrooms");
        
        _cartItemRepositoryMock.Verify(x => x.GetCartItemWithDetailsAsync(cartItemId), Times.Once);
        _cartRepositoryMock.Verify(x => x.IsCartItemOwnedByUserAsync(cartItemId, userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        var cartItemId = "item-1";
        var currentUserServiceMock = MockCurrentUserServiceHelper.CreateUnauthenticatedUser();

        var handler = new GetCartItemQueryHandler(_unitOfWorkMock.Object, currentUserServiceMock.Object);
        var query = new GetCartItemQuery(cartItemId);

        // Act
        var act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated");
        
        _cartItemRepositoryMock.Verify(x => x.GetCartItemWithDetailsAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCartItemDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "user-123";
        var cartItemId = "non-existent-item";
        var currentUserServiceMock = MockCurrentUserServiceHelper.CreateAuthenticatedUser(userId);

        _cartItemRepositoryMock
            .Setup(x => x.GetCartItemWithDetailsAsync(cartItemId))
            .ReturnsAsync((CartItem?)null);

        var handler = new GetCartItemQueryHandler(_unitOfWorkMock.Object, currentUserServiceMock.Object);
        var query = new GetCartItemQuery(cartItemId);

        // Act
        var act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Cart item with ID '{cartItemId}' not found");
        
        _cartItemRepositoryMock.Verify(x => x.GetCartItemWithDetailsAsync(cartItemId), Times.Once);
        _cartRepositoryMock.Verify(x => x.IsCartItemOwnedByUserAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotOwnCartItem_ThrowsUnauthorizedException()
    {
        // Arrange
        var userId = "user-123";
        var cartItemId = "item-1";
        var currentUserServiceMock = MockCurrentUserServiceHelper.CreateAuthenticatedUser(userId);

        var pizzaVariant = TestDataBuilder.PizzaVariant()
            .WithId("variant-1")
            .WithPizza(TestDataBuilder.Pizza()
                .WithId("pizza-1")
                .WithName("Margherita")
                .Build())
            .Build();

        var cartItem = TestDataBuilder.CartItem()
            .WithId(cartItemId)
            .WithCartId("cart-1")
            .WithPizzaVariant(pizzaVariant)
            .Build();

        _cartItemRepositoryMock
            .Setup(x => x.GetCartItemWithDetailsAsync(cartItemId))
            .ReturnsAsync(cartItem);

        _cartRepositoryMock
            .Setup(x => x.IsCartItemOwnedByUserAsync(cartItemId, userId))
            .ReturnsAsync(false);

        var handler = new GetCartItemQueryHandler(_unitOfWorkMock.Object, currentUserServiceMock.Object);
        var query = new GetCartItemQuery(cartItemId);

        // Act
        var act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("You do not have permission to view this cart item");
        
        _cartItemRepositoryMock.Verify(x => x.GetCartItemWithDetailsAsync(cartItemId), Times.Once);
        _cartRepositoryMock.Verify(x => x.IsCartItemOwnedByUserAsync(cartItemId, userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCartItemHasNoToppings_ReturnsCartItemWithEmptyToppingsList()
    {
        // Arrange
        var userId = "user-123";
        var cartItemId = "item-1";
        var currentUserServiceMock = MockCurrentUserServiceHelper.CreateAuthenticatedUser(userId);

        var pizzaVariant = TestDataBuilder.PizzaVariant()
            .WithId("variant-1")
            .WithSize(PizzaSize.Medium)
            .WithPrice(12.99m)
            .WithPizza(TestDataBuilder.Pizza()
                .WithId("pizza-1")
                .WithName("Pepperoni")
                .Build())
            .Build();

        var cartItem = TestDataBuilder.CartItem()
            .WithId(cartItemId)
            .WithPizzaVariant(pizzaVariant)
            .Build();

        _cartItemRepositoryMock
            .Setup(x => x.GetCartItemWithDetailsAsync(cartItemId))
            .ReturnsAsync(cartItem);

        _cartRepositoryMock
            .Setup(x => x.IsCartItemOwnedByUserAsync(cartItemId, userId))
            .ReturnsAsync(true);

        var handler = new GetCartItemQueryHandler(_unitOfWorkMock.Object, currentUserServiceMock.Object);
        var query = new GetCartItemQuery(cartItemId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Toppings.Should().NotBeNull();
        result.Toppings.Should().BeEmpty();
        result.ToppingsTotal.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenCartItemHasMultipleToppings_ReturnsAllToppings()
    {
        // Arrange
        var userId = "user-123";
        var cartItemId = "item-1";
        var currentUserServiceMock = MockCurrentUserServiceHelper.CreateAuthenticatedUser(userId);

        var pizzaVariant = TestDataBuilder.PizzaVariant()
            .WithId("variant-1")
            .WithSize(PizzaSize.Large)
            .WithPrice(14.99m)
            .WithPizza(TestDataBuilder.Pizza()
                .WithId("pizza-1")
                .WithName("Custom Pizza")
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

        var topping3 = TestDataBuilder.Topping()
            .WithId("topping-3")
            .WithName("Olives")
            .WithPrice(1.00m)
            .Build();

        var cartItem = TestDataBuilder.CartItem()
            .WithId(cartItemId)
            .WithPizzaVariant(pizzaVariant)
            .WithToppings(topping1, topping2, topping3)
            .Build();

        _cartItemRepositoryMock
            .Setup(x => x.GetCartItemWithDetailsAsync(cartItemId))
            .ReturnsAsync(cartItem);

        _cartRepositoryMock
            .Setup(x => x.IsCartItemOwnedByUserAsync(cartItemId, userId))
            .ReturnsAsync(true);

        var handler = new GetCartItemQueryHandler(_unitOfWorkMock.Object, currentUserServiceMock.Object);
        var query = new GetCartItemQuery(cartItemId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Toppings.Should().HaveCount(3);
        result.Toppings[0].ToppingName.Should().Be("Mushrooms");
        result.Toppings[1].ToppingName.Should().Be("Extra Cheese");
        result.Toppings[2].ToppingName.Should().Be("Olives");
        result.ToppingsTotal.Should().Be(4.50m);
    }
}
