using Microsoft.EntityFrameworkCore;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;
using PizzaStore.Infrastructure.Persistence.Data;

namespace PizzaStore.Infrastructure.Persistence.Repositories;

public class PizzaVariantRepository : Repository<PizzaVariant>, IPizzaVariantRepository
{
    public PizzaVariantRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PizzaVariant?> GetByPizzaIdAndSizeAsync(string pizzaId, PizzaSize size)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(pv => pv.PizzaId == pizzaId && pv.Size == size);
    }

    public async Task<IEnumerable<PizzaVariant>> GetVariantsByPizzaIdAsync(string pizzaId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(pv => pv.PizzaId == pizzaId)
            .ToListAsync();
    }
}
