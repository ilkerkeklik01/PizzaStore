using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Tests.Helpers;

/// <summary>
/// Provides fluent builder pattern for creating test data entities
/// </summary>
public static class TestDataBuilder
{
    public static PizzaBuilder Pizza() => new();
    public static PizzaVariantBuilder PizzaVariant() => new();
    public static ToppingBuilder Topping() => new();
    public static CartBuilder Cart() => new();
    public static CartItemBuilder CartItem() => new();
    public static OrderBuilder Order() => new();
    public static OrderItemBuilder OrderItem() => new();
    public static UserBuilder User() => new();
}

public class PizzaBuilder
{
    private readonly Pizza _pizza = new()
    {
        Id = Guid.NewGuid().ToString(),
        Name = "Margherita",
        Description = "Classic pizza",
        Type = PizzaType.Vegetarian,
        ImageUrl = "https://example.com/pizza.jpg",
        IsAvailable = true,
        Variants = new List<PizzaVariant>()
    };

    public PizzaBuilder WithId(string id) { _pizza.Id = id; return this; }
    public PizzaBuilder WithName(string name) { _pizza.Name = name; return this; }
    public PizzaBuilder WithDescription(string description) { _pizza.Description = description; return this; }
    public PizzaBuilder WithType(PizzaType type) { _pizza.Type = type; return this; }
    public PizzaBuilder WithImageUrl(string imageUrl) { _pizza.ImageUrl = imageUrl; return this; }
    public PizzaBuilder IsAvailable(bool isAvailable) { _pizza.IsAvailable = isAvailable; return this; }
    public PizzaBuilder WithVariants(params PizzaVariant[] variants) 
    { 
        _pizza.Variants = variants.ToList(); 
        return this; 
    }
    public Pizza Build() => _pizza;
}

public class PizzaVariantBuilder
{
    private readonly PizzaVariant _variant = new()
    {
        Id = Guid.NewGuid().ToString(),
        PizzaId = Guid.NewGuid().ToString(),
        Size = PizzaSize.Medium,
        Price = 12.99m,
        IsAvailable = true
    };

    public PizzaVariantBuilder WithId(string id) { _variant.Id = id; return this; }
    public PizzaVariantBuilder WithPizzaId(string pizzaId) { _variant.PizzaId = pizzaId; return this; }
    public PizzaVariantBuilder WithSize(PizzaSize size) { _variant.Size = size; return this; }
    public PizzaVariantBuilder WithPrice(decimal price) { _variant.Price = price; return this; }
    public PizzaVariantBuilder IsAvailable(bool isAvailable) { _variant.IsAvailable = isAvailable; return this; }
    public PizzaVariantBuilder WithPizza(Pizza pizza) 
    { 
        _variant.Pizza = pizza;
        _variant.PizzaId = pizza.Id;
        return this; 
    }
    public PizzaVariant Build() => _variant;
}

public class ToppingBuilder
{
    private readonly Topping _topping = new()
    {
        Id = Guid.NewGuid().ToString(),
        Name = "Mushrooms",
        Price = 1.50m,
        IsAvailable = true
    };

    public ToppingBuilder WithId(string id) { _topping.Id = id; return this; }
    public ToppingBuilder WithName(string name) { _topping.Name = name; return this; }
    public ToppingBuilder WithPrice(decimal price) { _topping.Price = price; return this; }
    public ToppingBuilder IsAvailable(bool isAvailable) { _topping.IsAvailable = isAvailable; return this; }
    public Topping Build() => _topping;
}

public class CartBuilder
{
    private readonly Cart _cart = new()
    {
        Id = Guid.NewGuid().ToString(),
        UserId = Guid.NewGuid().ToString(),
        CartItems = new List<CartItem>()
    };

    public CartBuilder WithId(string id) { _cart.Id = id; return this; }
    public CartBuilder WithUserId(string userId) { _cart.UserId = userId; return this; }
    public CartBuilder WithCartItems(params CartItem[] items) 
    { 
        _cart.CartItems = items.ToList();
        foreach (var item in items)
        {
            item.CartId = _cart.Id;
        }
        return this; 
    }
    public Cart Build() => _cart;
}

public class CartItemBuilder
{
    private readonly CartItem _cartItem = new()
    {
        Id = Guid.NewGuid().ToString(),
        CartId = Guid.NewGuid().ToString(),
        PizzaVariantId = Guid.NewGuid().ToString(),
        Quantity = 1,
        SpecialInstructions = null,
        CartItemToppings = new List<CartItemTopping>()
    };

    public CartItemBuilder WithId(string id) { _cartItem.Id = id; return this; }
    public CartItemBuilder WithCartId(string cartId) { _cartItem.CartId = cartId; return this; }
    public CartItemBuilder WithPizzaVariantId(string variantId) { _cartItem.PizzaVariantId = variantId; return this; }
    public CartItemBuilder WithQuantity(int quantity) { _cartItem.Quantity = quantity; return this; }
    public CartItemBuilder WithSpecialInstructions(string instructions) { _cartItem.SpecialInstructions = instructions; return this; }
    public CartItemBuilder WithPizzaVariant(PizzaVariant variant) 
    { 
        _cartItem.PizzaVariant = variant;
        _cartItem.PizzaVariantId = variant.Id;
        return this; 
    }
    public CartItemBuilder WithToppings(params Topping[] toppings)
    {
        _cartItem.CartItemToppings = toppings.Select(t => new CartItemTopping
        {
            CartItemId = _cartItem.Id,
            ToppingId = t.Id,
            Topping = t
        }).ToList();
        return this;
    }
    public CartItem Build() => _cartItem;
}

public class OrderBuilder
{
    private readonly Order _order = new()
    {
        Id = Guid.NewGuid().ToString(),
        UserId = Guid.NewGuid().ToString(),
        TotalPrice = 15.99m,
        Status = OrderStatus.Pending,
        CreatedAt = DateTime.UtcNow,
        OrderItems = new List<OrderItem>()
    };

    public OrderBuilder WithId(string id) { _order.Id = id; return this; }
    public OrderBuilder WithUserId(string userId) { _order.UserId = userId; return this; }
    public OrderBuilder WithTotalPrice(decimal totalPrice) { _order.TotalPrice = totalPrice; return this; }
    public OrderBuilder WithStatus(OrderStatus status) { _order.Status = status; return this; }
    public OrderBuilder WithCreatedAt(DateTime createdAt) { _order.CreatedAt = createdAt; return this; }
    public OrderBuilder WithConfirmedAt(DateTime? confirmedAt) { _order.ConfirmedAt = confirmedAt; return this; }
    public OrderBuilder WithCompletedAt(DateTime? completedAt) { _order.CompletedAt = completedAt; return this; }
    public OrderBuilder WithCancelledAt(DateTime? cancelledAt) { _order.CancelledAt = cancelledAt; return this; }
    public OrderBuilder WithOrderItems(params OrderItem[] items) 
    { 
        _order.OrderItems = items.ToList();
        foreach (var item in items)
        {
            item.OrderId = _order.Id;
        }
        return this; 
    }
    public Order Build() => _order;
}

public class OrderItemBuilder
{
    private readonly OrderItem _orderItem = new()
    {
        Id = Guid.NewGuid().ToString(),
        OrderId = Guid.NewGuid().ToString(),
        PizzaVariantId = Guid.NewGuid().ToString(),
        PizzaNameAtOrder = "Margherita",
        PizzaSizeAtOrder = "Medium",
        PizzaBasePriceAtOrder = 12.99m,
        Quantity = 1,
        SpecialInstructions = null,
        SubtotalAtOrder = 12.99m,
        OrderItemToppings = new List<OrderItemTopping>()
    };

    public OrderItemBuilder WithId(string id) { _orderItem.Id = id; return this; }
    public OrderItemBuilder WithOrderId(string orderId) { _orderItem.OrderId = orderId; return this; }
    public OrderItemBuilder WithPizzaVariantId(string variantId) { _orderItem.PizzaVariantId = variantId; return this; }
    public OrderItemBuilder WithPizzaName(string name) { _orderItem.PizzaNameAtOrder = name; return this; }
    public OrderItemBuilder WithQuantity(int quantity) { _orderItem.Quantity = quantity; return this; }
    public OrderItemBuilder WithSubtotal(decimal subtotal) { _orderItem.SubtotalAtOrder = subtotal; return this; }
    public OrderItem Build() => _orderItem;
}

public class UserBuilder
{
    private readonly ApplicationUser _user = new()
    {
        Id = Guid.NewGuid().ToString(),
        Email = "test@example.com",
        UserName = "test@example.com"
    };

    public UserBuilder WithId(string id) { _user.Id = id; return this; }
    public UserBuilder WithEmail(string email) 
    { 
        _user.Email = email;
        _user.UserName = email;
        return this; 
    }
    public ApplicationUser Build() => _user;
}
