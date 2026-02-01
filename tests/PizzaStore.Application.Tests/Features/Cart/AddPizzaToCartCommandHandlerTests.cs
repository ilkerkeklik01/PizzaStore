using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using PizzaStore.Application.Features.Commands.Cart.AddPizzaToCart;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Tests.Features.Cart;

public class AddPizzaToCartCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;
    private readonly Mock<ICartItemToppingRepository> _cartItemToppingRepositoryMock;
    private readonly Mock<IPizzaVariantRepository> _pizzaVariantRepositoryMock;
    private readonly Mock<IToppingRepository> _toppingRepositoryMock;
    private readonly Mock<IValidator<AddPizzaToCartDto>> _validatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly AddPizzaToCartCommandHandler _handler;

    public AddPizzaToCartCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cartRepositoryMock = new Mock<ICartRepository>();
        _cartItemRepositoryMock = new Mock<ICartItemRepository>();
        _cartItemToppingRepositoryMock = new Mock<ICartItemToppingRepository>();
        _pizzaVariantRepositoryMock = new Mock<IPizzaVariantRepository>();
        _toppingRepositoryMock = new Mock<IToppingRepository>();
        _validatorMock = new Mock<IValidator<AddPizzaToCartDto>>();
        _currentUserServiceMock = MockCurrentUserServiceHelper.CreateAuthenticatedUser();

        _unitOfWorkMock.Setup(x => x.Carts).Returns(_cartRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.CartItems).Returns(_cartItemRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.CartItemToppings).Returns(_cartItemToppingRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.PizzaVariants).Returns(_pizzaVariantRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Toppings).Returns(_toppingRepositoryMock.Object);

        _handler = new AddPizzaToCartCommandHandler(
            _unitOfWorkMock.Object,
            _validatorMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAddingPizzaWithNoToppings_ReturnsCartItemDto()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var pizza = TestDataBuilder.Pizza().WithName("Margherita").Build();
        var pizzaVariant = TestDataBuilder.PizzaVariant()
            .WithId("variant-123")
            .WithPizza(pizza)
            .WithSize(PizzaSize.Medium)
            .WithPrice(12.99m)
            .IsAvailable(true)
            .Build();

        var cart = TestDataBuilder.Cart()
            .WithUserId(userId)
            .WithCartItems()
            .Build();

        var dto = new AddPizzaToCartDto
        {
            PizzaVariantId = pizzaVariant.Id,
            Quantity = 1,
            ToppingIds = new List<string>()
        };

        var command = new AddPizzaToCartCommand(dto);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByIdAsync(pizzaVariant.Id))
            .ReturnsAsync(pizzaVariant);

        _cartRepositoryMock
            .Setup(x => x.GetOrCreateCartForUserAsync(userId))
            .ReturnsAsync(cart);

        CartItem? capturedCartItem = null;
        _cartItemRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<CartItem>()))
            .Callback<CartItem>(item => capturedCartItem = item)
            .Returns(Task.CompletedTask);

        _cartItemRepositoryMock
            .Setup(x => x.GetCartItemWithDetailsAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) =>
            {
                var item = new CartItem
                {
                    Id = id,
                    CartId = cart.Id,
                    PizzaVariantId = pizzaVariant.Id,
                    PizzaVariant = pizzaVariant,
                    Quantity = dto.Quantity,
                    SpecialInstructions = dto.SpecialInstructions ?? string.Empty,
                    CartItemToppings = new List<CartItemTopping>()
                };
                return item;
            });

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PizzaVariantId.Should().Be(pizzaVariant.Id);
        result.Quantity.Should().Be(1);
        result.Toppings.Should().BeEmpty();

        capturedCartItem.Should().NotBeNull();
        capturedCartItem!.PizzaVariantId.Should().Be(pizzaVariant.Id);
        capturedCartItem.Quantity.Should().Be(1);

        _cartItemToppingRepositoryMock.Verify(x => x.AddAsync(It.IsAny<CartItemTopping>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAddingPizzaWithMultipleToppings_CreatesCartItemWithToppings()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var pizza = TestDataBuilder.Pizza().WithName("Pepperoni").Build();
        var pizzaVariant = TestDataBuilder.PizzaVariant()
            .WithId("variant-123")
            .WithPizza(pizza)
            .WithSize(PizzaSize.Large)
            .WithPrice(15.99m)
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
            .WithPrice(1.25m)
            .IsAvailable(true)
            .Build();

        var topping3 = TestDataBuilder.Topping()
            .WithId("topping-3")
            .WithName("Extra Cheese")
            .WithPrice(2.00m)
            .IsAvailable(true)
            .Build();

        var cart = TestDataBuilder.Cart()
            .WithUserId(userId)
            .WithCartItems()
            .Build();

        var dto = new AddPizzaToCartDto
        {
            PizzaVariantId = pizzaVariant.Id,
            Quantity = 2,
            ToppingIds = new List<string> { topping1.Id, topping2.Id, topping3.Id },
            SpecialInstructions = "Extra crispy"
        };

        var command = new AddPizzaToCartCommand(dto);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByIdAsync(pizzaVariant.Id))
            .ReturnsAsync(pizzaVariant);

        _toppingRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Domain.Entities.Topping> { topping1, topping2, topping3 });

        _cartRepositoryMock
            .Setup(x => x.GetOrCreateCartForUserAsync(userId))
            .ReturnsAsync(cart);

        var addedToppings = new List<CartItemTopping>();
        _cartItemToppingRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<CartItemTopping>()))
            .Callback<CartItemTopping>(topping => addedToppings.Add(topping))
            .Returns(Task.CompletedTask);

        _cartItemRepositoryMock
            .Setup(x => x.GetCartItemWithDetailsAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) =>
            {
                var item = new CartItem
                {
                    Id = id,
                    CartId = cart.Id,
                    PizzaVariantId = pizzaVariant.Id,
                    PizzaVariant = pizzaVariant,
                    Quantity = dto.Quantity,
                    SpecialInstructions = dto.SpecialInstructions ?? string.Empty,
                    CartItemToppings = new List<Domain.Entities.CartItemTopping>
                    {
                        new() { CartItemId = id, ToppingId = topping1.Id, Topping = topping1 },
                        new() { CartItemId = id, ToppingId = topping2.Id, Topping = topping2 },
                        new() { CartItemId = id, ToppingId = topping3.Id, Topping = topping3 }
                    }
                };
                return item;
            });

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PizzaVariantId.Should().Be(pizzaVariant.Id);
        result.Quantity.Should().Be(2);
        result.SpecialInstructions.Should().Be("Extra crispy");
        result.Toppings.Should().HaveCount(3);
        result.Toppings.Should().Contain(t => t.ToppingName == "Mushrooms");
        result.Toppings.Should().Contain(t => t.ToppingName == "Olives");
        result.Toppings.Should().Contain(t => t.ToppingName == "Extra Cheese");

        addedToppings.Should().HaveCount(3);
        _cartItemToppingRepositoryMock.Verify(x => x.AddAsync(It.IsAny<CartItemTopping>()), Times.Exactly(3));
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDtoValidationFails_ThrowsValidationException()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var dto = new AddPizzaToCartDto
        {
            PizzaVariantId = "",
            Quantity = 0,
            ToppingIds = new List<string>()
        };

        var command = new AddPizzaToCartCommand(dto);

        var validationFailures = new List<ValidationFailure>
        {
            new("PizzaVariantId", "Pizza variant ID is required"),
            new("Quantity", "Quantity must be greater than 0")
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException>()
            .WithMessage("*Pizza variant ID is required*");
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns((string?)null);

        var dto = new AddPizzaToCartDto
        {
            PizzaVariantId = "variant-123",
            Quantity = 1,
            ToppingIds = new List<string>()
        };

        var command = new AddPizzaToCartCommand(dto);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated");
    }

    [Fact]
    public async Task Handle_WhenPizzaVariantNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var dto = new AddPizzaToCartDto
        {
            PizzaVariantId = "non-existent-variant",
            Quantity = 1,
            ToppingIds = new List<string>()
        };

        var command = new AddPizzaToCartCommand(dto);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByIdAsync("non-existent-variant"))
            .ReturnsAsync((Domain.Entities.PizzaVariant?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Pizza variant with ID 'non-existent-variant' not found");
    }

    [Fact]
    public async Task Handle_WhenToppingNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var pizza = TestDataBuilder.Pizza().Build();
        var pizzaVariant = TestDataBuilder.PizzaVariant()
            .WithId("variant-123")
            .WithPizza(pizza)
            .IsAvailable(true)
            .Build();

        var topping1 = TestDataBuilder.Topping().WithId("topping-1").Build();

        var dto = new AddPizzaToCartDto
        {
            PizzaVariantId = pizzaVariant.Id,
            Quantity = 1,
            ToppingIds = new List<string> { topping1.Id, "non-existent-topping" }
        };

        var command = new AddPizzaToCartCommand(dto);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByIdAsync(pizzaVariant.Id))
            .ReturnsAsync(pizzaVariant);

        _toppingRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Domain.Entities.Topping> { topping1 });

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Topping with ID 'non-existent-topping' not found");
    }

    [Fact]
    public async Task Handle_WhenPizzaVariantNotAvailable_ThrowsValidationException()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var pizza = TestDataBuilder.Pizza().WithName("Discontinued Pizza").Build();
        var pizzaVariant = TestDataBuilder.PizzaVariant()
            .WithId("variant-123")
            .WithPizza(pizza)
            .WithSize(PizzaSize.Large)
            .IsAvailable(false)
            .Build();

        var dto = new AddPizzaToCartDto
        {
            PizzaVariantId = pizzaVariant.Id,
            Quantity = 1,
            ToppingIds = new List<string>()
        };

        var command = new AddPizzaToCartCommand(dto);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByIdAsync(pizzaVariant.Id))
            .ReturnsAsync(pizzaVariant);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException>()
            .WithMessage("Pizza variant 'Large' is not available");
    }

    [Fact]
    public async Task Handle_WhenToppingNotAvailable_ThrowsValidationException()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var pizza = TestDataBuilder.Pizza().Build();
        var pizzaVariant = TestDataBuilder.PizzaVariant()
            .WithId("variant-123")
            .WithPizza(pizza)
            .IsAvailable(true)
            .Build();

        var topping1 = TestDataBuilder.Topping()
            .WithId("topping-1")
            .WithName("Available Topping")
            .IsAvailable(true)
            .Build();

        var topping2 = TestDataBuilder.Topping()
            .WithId("topping-2")
            .WithName("Unavailable Topping")
            .IsAvailable(false)
            .Build();

        var dto = new AddPizzaToCartDto
        {
            PizzaVariantId = pizzaVariant.Id,
            Quantity = 1,
            ToppingIds = new List<string> { topping1.Id, topping2.Id }
        };

        var command = new AddPizzaToCartCommand(dto);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByIdAsync(pizzaVariant.Id))
            .ReturnsAsync(pizzaVariant);

        _toppingRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Domain.Entities.Topping> { topping1, topping2 });

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException>()
            .WithMessage("Topping 'Unavailable Topping' is not available");
    }

    [Fact]
    public async Task Handle_WhenCartSizeLimitExceeded_ThrowsValidationException()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var pizza = TestDataBuilder.Pizza().Build();
        var pizzaVariant = TestDataBuilder.PizzaVariant()
            .WithId("variant-123")
            .WithPizza(pizza)
            .IsAvailable(true)
            .Build();

        // Create cart with 20 items (max limit)
        var cartItems = Enumerable.Range(0, 20)
            .Select(i => TestDataBuilder.CartItem().WithId($"item-{i}").Build())
            .ToArray();

        var cart = TestDataBuilder.Cart()
            .WithUserId(userId)
            .WithCartItems(cartItems)
            .Build();

        var dto = new AddPizzaToCartDto
        {
            PizzaVariantId = pizzaVariant.Id,
            Quantity = 1,
            ToppingIds = new List<string>()
        };

        var command = new AddPizzaToCartCommand(dto);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByIdAsync(pizzaVariant.Id))
            .ReturnsAsync(pizzaVariant);

        _cartRepositoryMock
            .Setup(x => x.GetOrCreateCartForUserAsync(userId))
            .ReturnsAsync(cart);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException>()
            .WithMessage("Cart cannot contain more than 20 items. Please remove some items before adding more.");
    }

    [Fact]
    public async Task Handle_WhenEmptyToppingList_AddsItemWithoutToppings()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var pizza = TestDataBuilder.Pizza().Build();
        var pizzaVariant = TestDataBuilder.PizzaVariant()
            .WithId("variant-123")
            .WithPizza(pizza)
            .IsAvailable(true)
            .Build();

        var cart = TestDataBuilder.Cart()
            .WithUserId(userId)
            .WithCartItems()
            .Build();

        var dto = new AddPizzaToCartDto
        {
            PizzaVariantId = pizzaVariant.Id,
            Quantity = 1,
            ToppingIds = new List<string>()
        };

        var command = new AddPizzaToCartCommand(dto);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByIdAsync(pizzaVariant.Id))
            .ReturnsAsync(pizzaVariant);

        _cartRepositoryMock
            .Setup(x => x.GetOrCreateCartForUserAsync(userId))
            .ReturnsAsync(cart);

        _cartItemRepositoryMock
            .Setup(x => x.GetCartItemWithDetailsAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) =>
            {
                var item = new CartItem
                {
                    Id = id,
                    CartId = cart.Id,
                    PizzaVariantId = pizzaVariant.Id,
                    PizzaVariant = pizzaVariant,
                    Quantity = dto.Quantity,
                    SpecialInstructions = string.Empty,
                    CartItemToppings = new List<CartItemTopping>()
                };
                return item;
            });

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Toppings.Should().BeEmpty();
        _cartItemToppingRepositoryMock.Verify(x => x.AddAsync(It.IsAny<CartItemTopping>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenQuantityGreaterThanOne_CreatesCartItemWithCorrectQuantity()
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

        var cart = TestDataBuilder.Cart()
            .WithUserId(userId)
            .WithCartItems()
            .Build();

        var dto = new AddPizzaToCartDto
        {
            PizzaVariantId = pizzaVariant.Id,
            Quantity = 5,
            ToppingIds = new List<string>()
        };

        var command = new AddPizzaToCartCommand(dto);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByIdAsync(pizzaVariant.Id))
            .ReturnsAsync(pizzaVariant);

        _cartRepositoryMock
            .Setup(x => x.GetOrCreateCartForUserAsync(userId))
            .ReturnsAsync(cart);

        CartItem? capturedCartItem = null;
        _cartItemRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<CartItem>()))
            .Callback<CartItem>(item => capturedCartItem = item)
            .Returns(Task.CompletedTask);

        _cartItemRepositoryMock
            .Setup(x => x.GetCartItemWithDetailsAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) =>
            {
                var item = new CartItem
                {
                    Id = id,
                    CartId = cart.Id,
                    PizzaVariantId = pizzaVariant.Id,
                    PizzaVariant = pizzaVariant,
                    Quantity = dto.Quantity,
                    SpecialInstructions = string.Empty,
                    CartItemToppings = new List<CartItemTopping>()
                };
                return item;
            });

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Quantity.Should().Be(5);

        capturedCartItem.Should().NotBeNull();
        capturedCartItem!.Quantity.Should().Be(5);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
