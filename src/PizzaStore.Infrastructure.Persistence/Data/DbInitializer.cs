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
                password: "Admin123!",
                role: "Admin");

            // Create regular user
            await CreateUserIfNotExistsAsync(
                userManager,
                email: "user@pizzastore.com",
                firstName: "Regular",
                lastName: "User",
                password: "User123!",
                role: "User");

            // Seed Toppings
            await SeedToppingsAsync(context);

            // Seed Pizzas and Variants
            await SeedPizzasAsync(context);

            await context.SaveChangesAsync();
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

    private static async Task SeedToppingsAsync(ApplicationDbContext context)
    {
        if (!context.Toppings.Any())
        {
            var toppings = new List<Topping>
            {
                new Topping { Name = "Pepperoni", Price = 1.50m, IsAvailable = true },
                new Topping { Name = "Mushrooms", Price = 1.00m, IsAvailable = true },
                new Topping { Name = "Onions", Price = 0.75m, IsAvailable = true },
                new Topping { Name = "Bell Peppers", Price = 1.00m, IsAvailable = true },
                new Topping { Name = "Black Olives", Price = 1.25m, IsAvailable = true },
                new Topping { Name = "Extra Cheese", Price = 2.00m, IsAvailable = true },
                new Topping { Name = "Bacon", Price = 1.75m, IsAvailable = true },
                new Topping { Name = "Sausage", Price = 1.50m, IsAvailable = true },
                new Topping { Name = "Pineapple", Price = 1.00m, IsAvailable = true },
                new Topping { Name = "Jalapeños", Price = 0.75m, IsAvailable = true }
            };

            await context.Toppings.AddRangeAsync(toppings);
        }
    }

    private static async Task SeedPizzasAsync(ApplicationDbContext context)
    {
        if (!context.Pizzas.Any())
        {
            var pizzas = new List<Pizza>
            {
                new Pizza
                {
                    Name = "Margherita",
                    Description = "Classic pizza with fresh mozzarella, tomatoes, and basil",
                    Type = PizzaType.Margherita,
                    ImageUrl = "/images/pizzas/margherita.jpg",
                    IsAvailable = true,
                    Variants = new List<PizzaVariant>
                    {
                        new PizzaVariant { Size = PizzaSize.Small, Price = 8.99m, IsAvailable = true },
                        new PizzaVariant { Size = PizzaSize.Medium, Price = 12.99m, IsAvailable = true },
                        new PizzaVariant { Size = PizzaSize.Large, Price = 16.99m, IsAvailable = true },
                        new PizzaVariant { Size = PizzaSize.ExtraLarge, Price = 20.99m, IsAvailable = true }
                    }
                },
                new Pizza
                {
                    Name = "Pepperoni",
                    Description = "Classic pepperoni pizza with mozzarella cheese and tomato sauce",
                    Type = PizzaType.MeatLovers,
                    ImageUrl = "/images/pizzas/pepperoni.jpg",
                    IsAvailable = true,
                    Variants = new List<PizzaVariant>
                    {
                        new PizzaVariant { Size = PizzaSize.Small, Price = 9.99m, IsAvailable = true },
                        new PizzaVariant { Size = PizzaSize.Medium, Price = 13.99m, IsAvailable = true },
                        new PizzaVariant { Size = PizzaSize.Large, Price = 17.99m, IsAvailable = true },
                        new PizzaVariant { Size = PizzaSize.ExtraLarge, Price = 21.99m, IsAvailable = true }
                    }
                },
                new Pizza
                {
                    Name = "Hawaiian",
                    Description = "Ham, pineapple, and mozzarella cheese",
                    Type = PizzaType.Hawaiian,
                    ImageUrl = "/images/pizzas/hawaiian.jpg",
                    IsAvailable = true,
                    Variants = new List<PizzaVariant>
                    {
                        new PizzaVariant { Size = PizzaSize.Small, Price = 10.99m, IsAvailable = true },
                        new PizzaVariant { Size = PizzaSize.Medium, Price = 14.99m, IsAvailable = true },
                        new PizzaVariant { Size = PizzaSize.Large, Price = 18.99m, IsAvailable = true },
                        new PizzaVariant { Size = PizzaSize.ExtraLarge, Price = 22.99m, IsAvailable = true }
                    }
                },
                new Pizza
                {
                    Name = "Veggie Supreme",
                    Description = "Loaded with mushrooms, onions, bell peppers, olives, and tomatoes",
                    Type = PizzaType.Vegetarian,
                    ImageUrl = "/images/pizzas/veggie-supreme.jpg",
                    IsAvailable = true,
                    Variants = new List<PizzaVariant>
                    {
                        new PizzaVariant { Size = PizzaSize.Small, Price = 10.49m, IsAvailable = true },
                        new PizzaVariant { Size = PizzaSize.Medium, Price = 14.49m, IsAvailable = true },
                        new PizzaVariant { Size = PizzaSize.Large, Price = 18.49m, IsAvailable = true },
                        new PizzaVariant { Size = PizzaSize.ExtraLarge, Price = 22.49m, IsAvailable = true }
                    }
                },
                new Pizza
                {
                    Name = "Meat Lovers",
                    Description = "Loaded with pepperoni, sausage, bacon, and ham",
                    Type = PizzaType.MeatLovers,
                    ImageUrl = "/images/pizzas/meat-lovers.jpg",
                    IsAvailable = true,
                    Variants = new List<PizzaVariant>
                    {
                        new PizzaVariant { Size = PizzaSize.Small, Price = 11.99m, IsAvailable = true },
                        new PizzaVariant { Size = PizzaSize.Medium, Price = 15.99m, IsAvailable = true },
                        new PizzaVariant { Size = PizzaSize.Large, Price = 19.99m, IsAvailable = true },
                        new PizzaVariant { Size = PizzaSize.ExtraLarge, Price = 24.99m, IsAvailable = true }
                    }
                }
            };

            await context.Pizzas.AddRangeAsync(pizzas);
        }
    }
}
