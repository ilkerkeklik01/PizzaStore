# GEMINI.md - PizzaStore Project Context

This project is a production-ready .NET 10 Web API for a pizza ordering system, built using **Clean Architecture** and **Vertical Slice Architecture** principles.

## 🏗️ Architecture & Project Structure

The solution follows a modular design with clear separation of concerns:

- **src/PizzaStore.API**: Presentation layer. Contains Controllers, Swagger/OpenAPI configuration, and application bootstrapping.
- **src/PizzaStore.Application**: Business logic layer. Organized by business context (Admin, Auth, Pizza, Cart, Order, etc.) using the **CQRS** pattern with **MediatR**.
- **src/PizzaStore.Core.Auth**: Security layer. Handles ASP.NET Core Identity integration and JWT token generation.
- **src/PizzaStore.Core.CrossCuttingConcerns**: Shared infrastructure for global exception handling, custom exceptions, and middleware.
- **src/PizzaStore.Domain**: Core layer. Contains domain entities (`Pizza`, `Order`, `ApplicationUser`, etc.) and repository interfaces.
- **src/PizzaStore.Infrastructure.Persistence**: Data access implementation using **Entity Framework Core 10** with Repository and Unit of Work patterns.
- **tests/**: Comprehensive testing suite with individual projects for each module.

## 🛠️ Technology Stack

- **Framework**: .NET 10.0
- **Database**: SQL Server (EF Core 10)
- **Identity**: ASP.NET Core Identity
- **CQRS**: MediatR 14.0.0
- **Validation**: FluentValidation 12.1.1
- **Auth**: JWT Bearer Authentication
- **Configuration**: DotNetEnv (.env file support)

## 🚀 Key Commands

- **Build Solution**: `dotnet build`
- **Run API**: `cd src/PizzaStore.API && dotnet run`
- **Run Tests**: `dotnet test`
- **Database Migrations**:
  - Add: `dotnet ef migrations add <MigrationName> --project src/PizzaStore.Infrastructure.Persistence --startup-project src/PizzaStore.API`
  - Update: `dotnet ef database update --project src/PizzaStore.Infrastructure.Persistence --startup-project src/PizzaStore.API`

## 📝 Development Conventions

- **Feature Organization**:
  - **Commands**: Follow `{Context}/Commands/{Action}/` (e.g., `Pizza/Commands/CreatePizza/`).
  - **Queries**: Follow `{Context}/Queries/` with a shared `DTOs/` folder (e.g., `Pizza/Queries/GetAllPizzasQuery.cs`).
- **CQRS Pattern**: Controllers must delegate to MediatR. Do not put business logic in controllers.
- **Data Access**: Always use `IUnitOfWork` within handlers. Do not inject `ApplicationDbContext` directly.
- **Authorization**: Use `ICurrentUserService` to access user claims and roles within the application layer.
- **Validation**: Every Command/Query DTO must have a corresponding `IValidator`.
- **Error Handling**: Throw custom exceptions from `Core.CrossCuttingConcerns.Exceptions` (e.g., `NotFoundException`). These are handled globally by middleware.
- **Mapping**: Perform manual mapping in handlers to maintain clarity and avoid AutoMapper complexities.
- **Documentation Maintenance**: ALWAYS update `README.md` and `CHANGELOG.md` after making changes to the codebase to ensure all documentation and version history remain up-to-date.

## 🔐 Default Credentials (Seed Data)

- **Admin**: `admin@pizzastore.com` / `Admin123!`
- **User**: `user@pizzastore.com` / `User123!`

## 🧪 Testing Guidelines

- **Unit Tests**: Focus on MediatR handlers in `tests/PizzaStore.Application.Tests`.
- **E2E Tests**: Use the Postman collection in `postman/` for full API validation.
- **Patterns**: Follow **AAA (Arrange-Act-Assert)** and utilize `TestDataBuilder` for consistent test data.

---
*Note: This file is for AI context. For full project details, refer to `README.md`.*
