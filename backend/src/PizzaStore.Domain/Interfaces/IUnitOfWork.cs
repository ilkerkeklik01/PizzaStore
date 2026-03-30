using Microsoft.EntityFrameworkCore.Storage;

namespace PizzaStore.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IPizzaRepository Pizzas { get; }
    IPizzaVariantRepository PizzaVariants { get; }
    IToppingRepository Toppings { get; }
    ICartRepository Carts { get; }
    ICartItemRepository CartItems { get; }
    ICartItemToppingRepository CartItemToppings { get; }
    IOrderRepository Orders { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
