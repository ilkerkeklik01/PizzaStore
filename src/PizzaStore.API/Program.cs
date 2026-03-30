using DotNetEnv;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PizzaStore.API.Filters;
using PizzaStore.Application.Extensions;
using PizzaStore.Core.Auth.Extensions;
using PizzaStore.Core.CrossCuttingConcerns.Extensions;
using PizzaStore.Infrastructure.Persistence.Extensions;
using PizzaStore.Infrastructure.Persistence.Data;
using PizzaStore.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file (only if not running in test)
if (!builder.Environment.IsEnvironment("Test"))
{
    Env.TraversePath().Load();
}

// Configure environment variables
builder.Configuration.AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings instead of integers
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(PizzaStore.Application.Features.Auth.Commands.Register.RegisterUserDtoValidator).Assembly);

// Add Application services (CurrentUserService, MediatR + pipeline behaviors, etc.)
builder.Services.AddApplicationServices();

// Add Persistence services (DbContext, Identity, Repositories)
builder.Services.AddPersistenceServices(builder.Configuration);

// Add Auth services (JWT Authentication, AuthService)
builder.Services.AddAuthServices(builder.Configuration);

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PizzaStore API",
        Version = "v3.0.0",
        Description = @"
# Pizza Store REST API

A comprehensive pizza ordering system built with Clean Architecture and CQRS pattern.

## Features
- 🍕 Browse pizza menu with variants (sizes) and toppings
- 🛒 Shopping cart management
- 📦 Order placement and tracking
- 👤 User authentication with JWT
- 🔐 Role-based authorization (User/Admin)
- ⚙️ Admin management endpoints

## Authentication
All endpoints (except login/register) require JWT Bearer authentication.

1. Register a new user via `/api/auth/register`
2. Login via `/api/auth/login` to get a JWT token
3. Include the token in the Authorization header without the `Bearer` prefix: `{token}`

## Default Credentials
- **Admin**: `admin@pizzastore.com` / `Admin123!`
- **User**: `user@pizzastore.com` / `User123!`
        ",
        Contact = new OpenApiContact
        {
            Name = "PizzaStore API Support",
            Email = "ilkerkeklik50@gmail.com"
        }
    });

    // Include XML comments for documentation
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Serialize enums as strings in Swagger schema
    options.UseInlineDefinitionsForEnums();
    options.SchemaFilter<EnumSchemaFilter>();

    // Add JWT Bearer Authentication to Swagger
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme.ToLower(),
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme. Do not include the 'Bearer' prefix. Example: \"Authorization: {token}\""
    });

    // Apply JWT Bearer authentication globally to all endpoints
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Run migrations and seed database with initial data (skip in Test environment - handled by test factory)
if (!app.Environment.IsEnvironment("Test"))
{
    await MigrateAndSeedDatabase(app);
}

// Configure the HTTP request pipeline

// IMPORTANT: Exception handler must be first
app.UseCrossCuttingConcerns();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PizzaStore API V1");
    });
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Run migrations and seed initial data
static async Task MigrateAndSeedDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Ensure database exists and apply migrations automatically
        logger.LogInformation("Checking database and applying migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database is ready.");

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        await DbInitializer.SeedAsync(context, userManager, roleManager, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

// Make Program accessible for integration tests
public partial class Program { }

