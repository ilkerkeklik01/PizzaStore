using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Infrastructure.Persistence.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger logger)
    {
        try
        {
            // Create roles
            await CreateRoleIfNotExistsAsync(roleManager, "Admin");
            await CreateRoleIfNotExistsAsync(roleManager, "User");

            // Create admin user
            await CreateUserIfNotExistsAsync(
                userManager,
                email: "admin@pizzastore.com",
                firstName: "Admin",
                lastName: "User",
                password: "Admin123",
                role: "Admin");

            // Create regular user
            await CreateUserIfNotExistsAsync(
                userManager,
                email: "user@pizzastore.com",
                firstName: "Regular",
                lastName: "User",
                password: "User123",
                role: "User");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task CreateRoleIfNotExistsAsync(RoleManager<ApplicationRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
        }
    }

    private static async Task CreateUserIfNotExistsAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string firstName,
        string lastName,
        string password,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
