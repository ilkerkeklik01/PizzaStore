using Microsoft.EntityFrameworkCore;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;
using PizzaStore.Infrastructure.Persistence.Data;

namespace PizzaStore.Infrastructure.Persistence.Repositories;

public class PizzaRepository : Repository<Pizza>, IPizzaRepository
{
    public PizzaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Pizza?> GetByIdWithVariantsAsync(string id)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Pizza>> GetAllWithVariantsAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Include(p => p.Variants)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pizza>> GetAvailablePizzasWithVariantsAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Include(p => p.Variants.Where(v => v.IsAvailable))
            .Where(p => p.IsAvailable)
            .ToListAsync();
    }
}
