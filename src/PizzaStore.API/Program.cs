using DotNetEnv;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using PizzaStore.Core.Auth.Extensions;
using PizzaStore.Core.CrossCuttingConcerns.Extensions;
using PizzaStore.Infrastructure.Persistence.Extensions;
using PizzaStore.Infrastructure.Persistence.Data;
using PizzaStore.Domain.Entities;

// Load environment variables from .env file
Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Configure environment variables
builder.Configuration.AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers();

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PizzaStore.Application.Features.Commands.Auth.Register.RegisterUserCommand).Assembly));

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(PizzaStore.Application.Features.Commands.Auth.Register.RegisterUserDtoValidator).Assembly);

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
        Version = "v1",
        Description = "Pizza Store API with Clean Architecture, JWT Authentication, and Authorization"
    });

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

// Seed database with initial data
await SeedDatabase(app);

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

// Seed initial data
static async Task SeedDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    await DbInitializer.SeedAsync(context, userManager, roleManager, logger);
}

