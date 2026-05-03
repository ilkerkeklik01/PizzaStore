---
paths: ["backend/**/*.cs"]
---

# C# Code Style — Formatting, Async, and Exceptions

## Code Style

- Use **file-scoped namespaces**: `namespace PizzaStore.Application.Features.Order.Commands;`
- Use `var` when the type is obvious from the right-hand side. Use explicit types when clarity demands it (collection types in entity initializers, method return types in public API).
- Prefer **expression-bodied members** for trivial single-expression methods and properties.
- Use **string interpolation** (`$"Order {id} not found"`) over `string.Format` or concatenation.
- Required navigation properties in entities initialize with `null!`: `public ApplicationUser User { get; set; } = null!;`
- Optional collections initialize to empty: `public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();`
- Use **pattern matching** and **switch expressions** over chains of `if/else if` when matching on type or value.
- Prefer `is null` / `is not null` over `== null` / `!= null`.

## Comments

- Write no comments that explain *what* the code does — well-named identifiers do that.
- Only add a comment when the *why* is non-obvious: a hidden constraint, an external bug workaround, or a subtle invariant.
- Add **XML doc comments** (`/// <summary>`) on public handler classes and all controller action methods (Swagger reads them).
- Never write multi-line comment blocks for what belongs in a PR description.

## Async / Await

- Always propagate `CancellationToken cancellationToken` through the full call chain: controller → handler → repository.
- Use `async`/`await` throughout. Never call `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`.
- Only suffix a method with `Async` when it actually returns `Task` or `ValueTask`.
- Do not use `async` on a method that only wraps a single awaited call with no surrounding logic — return the `Task` directly.

## Exception Handling

- Use the project's domain exceptions exclusively: `NotFoundException`, `ValidationException`, `UnauthorizedException`, `ForbiddenException` (from `PizzaStore.Core.CrossCuttingConcerns`).
- Throw the narrowest applicable exception: `NotFoundException` for missing records, `ValidationException` for business rule violations.
- Do not catch and re-throw domain exceptions inside handlers unless structured logging is needed at that level.
- `GlobalExceptionHandlingMiddleware` maps domain exceptions to HTTP responses automatically — never construct `BadRequest`, `NotFound`, or `Unauthorized` results manually from within a handler.
- Input validation belongs in FluentValidation `Validator` classes, not inside `Handle()` methods.

## Logging

- Inject `ILogger<T>` (where `T` is the handler class) via constructor — never use a static logger or `LoggerFactory` directly.
- Use **structured logging** with message templates: `_logger.LogInformation("Order {OrderId} updated to {Status}", id, status)`.
- Log at `Information` for normal state transitions, `Warning` for recoverable anomalies (e.g., missing optional record), `Error` for unexpected failures.
- Never log sensitive data: passwords, tokens, full card numbers.
