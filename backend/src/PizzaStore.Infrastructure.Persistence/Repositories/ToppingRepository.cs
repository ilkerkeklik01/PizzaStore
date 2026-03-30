using Microsoft.EntityFrameworkCore;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;
using PizzaStore.Infrastructure.Persistence.Data;

namespace PizzaStore.Infrastructure.Persistence.Repositories;

public class ToppingRepository : Repository<Topping>, IToppingRepository
{
    public ToppingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Topping>> GetAvailableToppingsAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.IsAvailable)
            .ToListAsync();
    }

    public async Task<IEnumerable<Topping>> GetToppingsByIdsAsync(IEnumerable<string> ids)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => ids.Contains(t.Id))
            .ToListAsync();
    }
}
