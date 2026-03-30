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
- **`PizzaStore.Application`** — 32+ CQRS handlers organized by business context (`Admin`, `Auth`, `Pizza`, `Cart`, `Order`, `PizzaVariant`, `Topping`). Each context lives under `Features/{Context}/Commands` and `Features/{Context}/Queries`, where each feature folder contains: Request, Handler, DTO, and FluentValidation Validator. `Common/Behaviors/AuthorizationBehavior.cs` is a MediatR pipeline behavior that enforces auth before any handler runs. `Common/Interfaces/ISecuredRequest.cs` defines `ISecuredRequest` (requires authentication) and `IAdminRequest : ISecuredRequest` (requires Admin role) — requests implement these interfaces to declare their auth requirements.
- **`PizzaStore.Infrastructure.Persistence`** — EF Core 10 / SQL Server. `ApplicationDbContext` extends Identity. Repositories and Unit of Work implementations. `DbInitializer` seeds roles, users, toppings, and pizzas on startup.
- **`PizzaStore.Core.Auth`** — JWT (HMAC-SHA256) generation, `AuthService`, `CurrentUserService`.
- **`PizzaStore.Core.CrossCuttingConcerns`** — `GlobalExceptionHandlingMiddleware` maps custom exceptions (`ValidationException`, `NotFoundException`, `UnauthorizedException`, `ForbiddenException`) to HTTP responses.
- **`PizzaStore.API`** — 6 controllers, 31 endpoints. Startup in `Program.cs` wires MediatR, FluentValidation, JWT, EF Core, Swagger, and runs migrations + seeding automatically.

### Request flow

```
Controller → MediatR → AuthorizationBehavior → Handler (Command/Query) → Repository/UnitOfWork → EF Core → SQL Server
```

`AuthorizationBehavior` is transparent for requests that don't implement `ISecuredRequest`. For secured requests it throws `UnauthorizedException` (401) or `ForbiddenException` (403) before the handler is ever reached. Controllers carry no `[Authorize]` attributes — all auth is enforced in the pipeline.

### Adding a new feature

Follow the existing pattern for the relevant context:
1. Add domain entity/interface to `PizzaStore.Domain` if needed.
2. Create `Request`, `Handler`, `DTO`, and `Validator` under `PizzaStore.Application/Features/{Context}/{Commands|Queries}/`.
3. If the request requires auth, implement `ISecuredRequest` (authenticated users) or `IAdminRequest` (Admin role) on the request class — no controller attribute needed.
4. Add repository method if required in `PizzaStore.Infrastructure.Persistence`.
5. Add controller endpoint in `PizzaStore.API/Controllers/`.

## Key Configuration

`.env` file (from `.env.example`):
- `JWT_SECRET_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_EXPIRY_MINUTES`
- `SQL_SERVER_HOST`, `SQL_SERVER_PORT`, `SQL_SERVER_DATABASE`, `SQL_SERVER_USER`, `SQL_SERVER_PASSWORD`
- `AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_API_KEY`, `AZURE_OPENAI_DEPLOYMENT`, `ASSISTANT_API_BASE_URL`, `ASSISTANT_MAX_TOKENS`, `ASSISTANT_TEMPERATURE`

## Testing

- **Unit tests:** xUnit + Moq + FluentAssertions. 189 tests covering all handlers and `AuthorizationBehavior`. AAA pattern throughout. `TestDataBuilder` and `MockCurrentUserServiceHelper` provide shared test utilities. Authorization scenarios (unauthenticated, wrong role) are tested in `AuthorizationBehaviorTests` — do not duplicate them in handler tests.
- **E2E tests:** 57 Postman tests in `postman/PizzaStore-E2E-Tests.postman_collection.json`.
- Seeded test credentials: `admin@pizzastore.com` / `Admin123!` and `user@pizzastore.com` / `User123!`.
