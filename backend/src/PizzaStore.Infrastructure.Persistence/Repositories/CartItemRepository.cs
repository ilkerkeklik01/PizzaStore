using Microsoft.EntityFrameworkCore;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;
using PizzaStore.Infrastructure.Persistence.Data;

namespace PizzaStore.Infrastructure.Persistence.Repositories;

public class CartItemRepository : Repository<CartItem>, ICartItemRepository
{
    public CartItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<CartItem?> GetCartItemWithDetailsAsync(string cartItemId)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(ci => ci.PizzaVariant)
                .ThenInclude(pv => pv.Pizza)
            .Include(ci => ci.CartItemToppings)
                .ThenInclude(cit => cit.Topping)
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
    }
}
