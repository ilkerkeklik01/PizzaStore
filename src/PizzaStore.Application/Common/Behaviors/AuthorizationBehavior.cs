using MediatR;
using PizzaStore.Application.Common.Interfaces;
using PizzaStore.Application.Services;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;

namespace PizzaStore.Application.Common.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserService _currentUserService;

    public AuthorizationBehavior(ICurrentUserService currentUserService)
        => _currentUserService = currentUserService;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ISecuredRequest)
            return await next(cancellationToken);

        if (!_currentUserService.IsAuthenticated())
            throw new UnauthorizedException("Authentication required");

        if (request is IAdminRequest && !_currentUserService.IsInRole("Admin"))
            throw new ForbiddenException("Administrator role required");

        return await next(cancellationToken);
    }
}
