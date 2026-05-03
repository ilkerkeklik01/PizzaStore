---
paths: ["backend/**/*.cs"]
---

# C# Architecture Rules — SOLID, DRY, CQRS

## SOLID Principles

### Single Responsibility (SRP)
- One handler per use case. Never merge two commands or two queries into one handler class.
- Controllers only dispatch to MediatR — zero business logic belongs in controllers.
- FluentValidation `Validator` classes handle input validation only; handlers handle orchestration only.
- Each repository interface covers one aggregate root.

### Open/Closed (OCP)
- Add a new handler when a new use case is needed; never expand an existing handler with a conditional branch that changes its fundamental purpose.
- Extend `BaseEntity` for new entities; never modify `BaseEntity` to accommodate a single entity's edge case.
- Add methods to the specific `I{Entity}Repository` interface for entity-specific queries; do not make generic `IRepository<T>` methods accept flags or enums to alter behavior.

### Liskov Substitution (LSP)
- Every `IRepository<T>` implementation must fulfill all inherited interface members — never throw `NotImplementedException` on a base repository method.
- Domain entities must behave correctly when accessed through `BaseEntity` references (Id, audit fields always present).

### Interface Segregation (ISP)
- Keep interfaces narrow. `IOrderRepository` extends `IRepository<Order>` and adds only Order-specific query methods.
- Do not add unrelated methods to an existing interface to avoid creating a new one. Create the new interface.
- `ICurrentUserService` exposes only `UserId` and `Role` — nothing else that callers do not need.

### Dependency Inversion (DIP)
- Handlers depend on `IUnitOfWork` and `ILogger<T>`, never on `ApplicationDbContext` or concrete repository classes.
- Register all dependencies in the layer-specific extension methods (`ApplicationServiceExtensions`, `CrossCuttingServiceExtensions`). Never inline registrations in `Program.cs`.
- Never instantiate a service with `new` inside a handler or controller — always inject through the constructor.

## DRY Principle

- **DTO mapping:** Use static `FromEntity()` factory methods on DTOs (e.g., `OrderDto.FromEntity(order)`) rather than mapping inline in multiple handlers. If no `FromEntity` exists, create one.
- **Base types:** `BaseEntity` owns `Id`, `CreatedAt`, `UpdatedAt`. Never duplicate these on a derived entity.
- **Generic CRUD:** `IRepository<T>` covers `GetByIdAsync`, `AddAsync`, `RemoveAsync`. Add a method to `IOrderRepository` only when the query needs Order-specific EF Core includes or filters.
- **Test helpers:** Use `TestDataBuilder` for test entities and `MockCurrentUserServiceHelper` for auth mocks. Never construct raw entities inline across multiple test classes.

## CQRS Boundaries

- **Commands** change state. **Queries** only read state. Never mix both in one handler.
- Commands return the resulting DTO (for API usability). Return `Unit` only when there is genuinely nothing to return to the caller.
- Queries never call `SaveChangesAsync` or modify any entity.
- `UnitOfWork.SaveChangesAsync(cancellationToken)` is the single save point. Never call `DbContext.SaveChangesAsync` directly from a handler.

## Auth Conventions

- Auth intent is declared on the Request class by implementing `ISecuredRequest` (authenticated user) or `IAdminRequest` (Admin role).
- Never add `[Authorize]` attributes to controllers — `AuthorizationBehavior` enforces auth in the MediatR pipeline before any handler runs.
- Never duplicate authorization checks inside a handler that already implements `ISecuredRequest`/`IAdminRequest`.
