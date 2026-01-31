using PizzaStore.Domain.Entities;

namespace PizzaStore.Domain.Interfaces;

public interface IUserRepository : IRepository<ApplicationUser>
{
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<bool> ExistsAsync(string email);
}
