using Microsoft.EntityFrameworkCore;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;
using PizzaStore.Infrastructure.Persistence.Data;

namespace PizzaStore.Infrastructure.Persistence.Repositories;

public class CartRepository : Repository<Cart>, ICartRepository
{
    public CartRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Cart?> GetCartWithItemsByUserIdAsync(string userId)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.PizzaVariant)
                    .ThenInclude(pv => pv.Pizza)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.CartItemToppings)
                    .ThenInclude(cit => cit.Topping)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart> GetOrCreateCartForUserAsync(string userId)
    {
        var cart = await _dbSet.FirstOrDefaultAsync(c => c.UserId == userId);
        
        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _dbSet.AddAsync(cart);
        }
        
        return cart;
    }

    public async Task<CartItem?> GetCartItemByIdAsync(string cartItemId)
    {
        return await _context.Set<CartItem>()
            .AsNoTracking()
            .Include(ci => ci.PizzaVariant)
                .ThenInclude(pv => pv.Pizza)
            .Include(ci => ci.CartItemToppings)
                .ThenInclude(cit => cit.Topping)
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
    }

    public async Task<bool> IsCartItemOwnedByUserAsync(string cartItemId, string userId)
    {
        return await _context.Set<CartItem>()
            .AsNoTracking()
            .Include(ci => ci.Cart)
            .AnyAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);
    }
}
