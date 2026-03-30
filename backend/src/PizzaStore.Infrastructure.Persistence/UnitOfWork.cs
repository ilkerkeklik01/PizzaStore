using Microsoft.EntityFrameworkCore.Storage;
using PizzaStore.Domain.Interfaces;
using PizzaStore.Infrastructure.Persistence.Data;
using PizzaStore.Infrastructure.Persistence.Repositories;

namespace PizzaStore.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private IUserRepository? _users;
    private IPizzaRepository? _pizzas;
    private IPizzaVariantRepository? _pizzaVariants;
    private IToppingRepository? _toppings;
    private ICartRepository? _carts;
    private ICartItemRepository? _cartItems;
    private ICartItemToppingRepository? _cartItemToppings;
    private IOrderRepository? _orders;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IPizzaRepository Pizzas => _pizzas ??= new PizzaRepository(_context);
    public IPizzaVariantRepository PizzaVariants => _pizzaVariants ??= new PizzaVariantRepository(_context);
    public IToppingRepository Toppings => _toppings ??= new ToppingRepository(_context);
    public ICartRepository Carts => _carts ??= new CartRepository(_context);
    public ICartItemRepository CartItems => _cartItems ??= new CartItemRepository(_context);
    public ICartItemToppingRepository CartItemToppings => _cartItemToppings ??= new CartItemToppingRepository(_context);
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return _transaction;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
