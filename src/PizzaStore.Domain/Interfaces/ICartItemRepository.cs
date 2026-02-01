namespace PizzaStore.Domain.Interfaces;

public interface ICartItemRepository : IRepository<Entities.CartItem>
{
    Task<Entities.CartItem?> GetCartItemWithDetailsAsync(string cartItemId);
}
