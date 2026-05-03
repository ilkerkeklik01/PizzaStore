---
paths: ["backend/**/*.cs"]
---

# C# Naming Conventions

## General Identifiers

| Element | Style | Example |
|---|---|---|
| Class, record, struct | PascalCase | `OrderController`, `BaseEntity` |
| Interface | `I` prefix + PascalCase | `IOrderRepository`, `IUnitOfWork` |
| Method | PascalCase | `GetOrderByIdAsync`, `ValidateStatusTransition` |
| Public property | PascalCase | `TotalPrice`, `OrderItems` |
| Private field | `_` prefix + camelCase | `_unitOfWork`, `_logger` |
| Local variable / parameter | camelCase | `updatedOrder`, `cancellationToken` |
| Generic type parameter | `T` prefix + PascalCase | `TEntity`, `TResult` |
| Enum type | PascalCase singular | `OrderStatus`, `PizzaSize` |
| Enum member | PascalCase | `OutForDelivery`, `Cancelled` |
| Constant | PascalCase | `MaxPageSize` |
| Async method | `Async` suffix | `GetByIdAsync`, `SaveChangesAsync` |

## CQRS Artifact Naming

Enforce strictly — these names must match across all feature folders:

| Artifact | Pattern | Example |
|---|---|---|
| Command | `{Feature}Command` | `UpdateOrderStatusCommand` |
| Query | `{Feature}Query` | `GetMyOrdersQuery` |
| Handler | `{Feature}CommandHandler` / `{Feature}QueryHandler` | `UpdateOrderStatusCommandHandler` |
| DTO | `{Feature}Dto` or `{Feature}ResponseDto` | `OrderDto`, `PizzaResponseDto` |
| Validator | `{Feature}CommandValidator` / `{Feature}QueryValidator` | `CheckoutCartCommandValidator` |

## Rules

- Never abbreviate names except for universally accepted acronyms (`Id`, `Dto`, `Url`, `JWT`).
- Never use single-letter names except loop counters (`i`, `j`) and LINQ lambdas (`x => x.Id`).
- Interface names always start with `I` — never name an interface without the prefix.
- Boolean properties and parameters read as assertions: `IsAvailable`, `HasItems`, not `Available`, `Items`.
