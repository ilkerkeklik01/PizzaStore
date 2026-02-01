using PizzaStore.Domain.Entities;

namespace PizzaStore.Domain.Interfaces;

public interface IPizzaVariantRepository : IRepository<PizzaVariant>
{
    Task<PizzaVariant?> GetByPizzaIdAndSizeAsync(string pizzaId, PizzaSize size);
    Task<IEnumerable<PizzaVariant>> GetVariantsByPizzaIdAsync(string pizzaId);
}
