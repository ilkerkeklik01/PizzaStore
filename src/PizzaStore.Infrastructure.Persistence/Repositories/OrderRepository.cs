using Microsoft.EntityFrameworkCore;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;
using PizzaStore.Infrastructure.Persistence.Data;

namespace PizzaStore.Infrastructure.Persistence.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemToppings)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderByIdWithDetailsAsync(string orderId)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemToppings)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<IEnumerable<Order>> GetAllOrdersWithDetailsAsync(OrderStatus? status = null, string? userId = null)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemToppings)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(o => o.UserId == userId);
        }

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> IsOrderOwnedByUserAsync(string orderId, string userId)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(o => o.Id == orderId && o.UserId == userId);
    }
}
