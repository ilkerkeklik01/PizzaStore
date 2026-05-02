---
name: new-cqrs-feature
description: Scaffold a Clean Architecture CQRS feature (Request, Handler, DTO, Validator, and optional controller endpoint) for the PizzaStore backend. Accepts context name, feature name, and type (Command/Query).
---

You are scaffolding a new CQRS feature for the PizzaStore backend following its Clean Architecture conventions.

## What to ask the user (if not already specified)

1. **Context** — which business context? (`Admin`, `Auth`, `Pizza`, `Cart`, `Order`, `PizzaVariant`, `Topping`, or a new one)
2. **Feature name** — e.g., `UpdatePizzaPrice`, `GetOrderSummary`
3. **Type** — `Command` (write) or `Query` (read)
4. **Auth level** — `None` (public), `ISecuredRequest` (authenticated user), or `IAdminRequest` (admin only)
5. **Return type** — e.g., `OrderDto`, `bool`, `Guid`
6. **Controller endpoint** — should a new endpoint be added? If yes, which controller and HTTP method?

## File structure to create

All files live under:
`backend/src/PizzaStore.Application/Features/{Context}/{Commands|Queries}/{FeatureName}/`

Files:
- `{FeatureName}{Command|Query}.cs` — the MediatR request record
- `{FeatureName}{Command|Query}Handler.cs` — the handler
- `{FeatureName}Dto.cs` — response DTO (skip if reusing an existing DTO)
- `{FeatureName}Validator.cs` — FluentValidation validator (skip if no inputs)

## Code patterns to follow

### Request (Command example)
```csharp
using MediatR;
using PizzaStore.Application.Common.Interfaces;

namespace PizzaStore.Application.Features.{Context}.Commands.{FeatureName};

public record {FeatureName}Command(/* properties */) : IRequest<{ReturnType}>, ISecuredRequest;
// Use IAdminRequest instead of ISecuredRequest for admin-only
// Omit the auth interface for public endpoints
```

### Handler
```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.{Context}.Commands.{FeatureName};

public class {FeatureName}CommandHandler : IRequestHandler<{FeatureName}Command, {ReturnType}>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<{FeatureName}CommandHandler> _logger;

    public {FeatureName}CommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<{FeatureName}CommandHandler> _logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<{ReturnType}> Handle({FeatureName}Command request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

### FluentValidation Validator
```csharp
using FluentValidation;

namespace PizzaStore.Application.Features.{Context}.Commands.{FeatureName};

public class {FeatureName}Validator : AbstractValidator<{FeatureName}Command>
{
    public {FeatureName}Validator()
    {
        // RuleFor(x => x.Property).NotEmpty()...
    }
}
```

### Controller endpoint pattern
```csharp
[HttpPost]
[ProducesResponseType(typeof({ReturnType}), StatusCodes.Status200OK)]
public async Task<IActionResult> {FeatureName}([FromBody] {FeatureName}Command command)
{
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

## Checklist

After creating the files, verify:
- [ ] Namespace matches folder path exactly
- [ ] Auth interface (`ISecuredRequest` / `IAdminRequest`) matches the intended access level
- [ ] Validator exists for all Commands with user-supplied inputs
- [ ] If a new domain entity is needed, prompt the user to add it to `PizzaStore.Domain` first
- [ ] If a new repository method is needed, note it for `PizzaStore.Infrastructure.Persistence`
- [ ] Run `dotnet build` to confirm no compilation errors
