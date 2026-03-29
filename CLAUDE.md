# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Documentation Updates

After completing any changes, ask the user: **"Should I update the readme and changelog files to keep them up-to-date?"** Do NOT update `README.md` or `CHANGELOG.md` without explicit user approval.

## Commands

```bash
# Restore & build
dotnet restore
dotnet build

# Run API (Swagger at https://localhost:5001/swagger)
cd src/PizzaStore.API && dotnet run

# Run all unit tests
dotnet test

# Run tests for a specific project
cd tests/PizzaStore.Application.Tests && dotnet test

# Filter to a specific test class
dotnet test --filter "GetPizzaByIdQueryHandlerTests"

# Watch mode
dotnet watch test

# E2E tests (requires newman)
newman run postman/PizzaStore-E2E-Tests.postman_collection.json
```

**Setup:** Copy `.env.example` to `.env` and configure JWT and SQL Server values before running.

## Architecture

Clean Architecture with CQRS via MediatR. Four main layers:

- **`PizzaStore.Domain`** — Core entities (Pizza, PizzaVariant, Topping, Cart, CartItem, Order, OrderItem, ApplicationUser), enums, `BaseEntity`, and repository/unit-of-work interfaces. No external dependencies.
- **`PizzaStore.Application`** — 32+ CQRS handlers organized by business context (`Admin`, `Auth`, `Pizza`, `Cart`, `Order`, `PizzaVariant`, `Topping`). Each context lives under `Features/{Context}/Commands` and `Features/{Context}/Queries`, where each feature folder contains: Request, Handler, DTO, and FluentValidation Validator.
- **`PizzaStore.Infrastructure.Persistence`** — EF Core 10 / SQL Server. `ApplicationDbContext` extends Identity. Repositories and Unit of Work implementations. `DbInitializer` seeds roles, users, toppings, and pizzas on startup.
- **`PizzaStore.Core.Auth`** — JWT (HMAC-SHA256) generation, `AuthService`, `CurrentUserService`.
- **`PizzaStore.Core.CrossCuttingConcerns`** — `GlobalExceptionHandlingMiddleware` maps custom exceptions (`ValidationException`, `NotFoundException`, `UnauthorizedException`, `ForbiddenException`) to HTTP responses.
- **`PizzaStore.API`** — 6 controllers, 31 endpoints. Startup in `Program.cs` wires MediatR, FluentValidation, JWT, EF Core, Swagger, and runs migrations + seeding automatically.

### Request flow

```
Controller → MediatR → Handler (Command/Query) → Repository/UnitOfWork → EF Core → SQL Server
```

### Adding a new feature

Follow the existing pattern for the relevant context:
1. Add domain entity/interface to `PizzaStore.Domain` if needed.
2. Create `Request`, `Handler`, `DTO`, and `Validator` under `PizzaStore.Application/Features/{Context}/{Commands|Queries}/`.
3. Add repository method if required in `PizzaStore.Infrastructure.Persistence`.
4. Add controller endpoint in `PizzaStore.API/Controllers/`.

## Key Configuration

`.env` file (from `.env.example`):
- `JWT_SECRET_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_EXPIRY_MINUTES`
- `SQL_SERVER_HOST`, `SQL_SERVER_PORT`, `SQL_SERVER_DATABASE`, `SQL_SERVER_USER`, `SQL_SERVER_PASSWORD`
- `AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_API_KEY`, `AZURE_OPENAI_DEPLOYMENT`, `ASSISTANT_API_BASE_URL`, `ASSISTANT_MAX_TOKENS`, `ASSISTANT_TEMPERATURE`

## Testing

- **Unit tests:** xUnit + Moq + FluentAssertions. 193 tests covering all handlers. AAA pattern throughout. `TestDataBuilder` and `MockCurrentUserServiceHelper` provide shared test utilities.
- **E2E tests:** 57 Postman tests in `postman/PizzaStore-E2E-Tests.postman_collection.json`.
- Seeded test credentials: `admin@pizzastore.com` / `Admin123!` and `user@pizzastore.com` / `User123!`.
