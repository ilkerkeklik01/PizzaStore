using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;
using PizzaStore.Infrastructure.Persistence.Data;

namespace PizzaStore.Infrastructure.Persistence.Repositories;

public class CartItemToppingRepository : Repository<CartItemTopping>, ICartItemToppingRepository
{
    public CartItemToppingRepository(ApplicationDbContext context) : base(context)
    {
    }
}
