using FluentAssertions;
using MediatR;
using Moq;
using PizzaStore.Application.Common.Behaviors;
using PizzaStore.Application.Common.Interfaces;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;

namespace PizzaStore.Application.Tests.Common.Behaviors;

public class AuthorizationBehaviorTests
{
    private sealed record PublicRequest : IRequest<string>;

    private sealed record SecuredRequest : IRequest<string>, ISecuredRequest;

    private sealed record AdminRequest : IRequest<string>, IAdminRequest;

    private static RequestHandlerDelegate<string> NextReturning(string value)
        => _ => Task.FromResult(value);

    [Fact]
    public async Task Handle_WhenRequestIsNotSecured_PassesThroughWithoutAuthCheck()
    {
        // Arrange
        var currentUserMock = MockCurrentUserServiceHelper.CreateUnauthenticatedUser();
        var behavior = new AuthorizationBehavior<PublicRequest, string>(currentUserMock.Object);

        // Act
        var result = await behavior.Handle(
            new PublicRequest(),
            NextReturning("ok"),
            CancellationToken.None);

        // Assert
        result.Should().Be("ok");
        currentUserMock.Verify(x => x.IsAuthenticated(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSecuredRequestAndAuthenticated_CallsNext()
    {
        // Arrange
        var currentUserMock = MockCurrentUserServiceHelper.CreateAuthenticatedUser();
        var behavior = new AuthorizationBehavior<SecuredRequest, string>(currentUserMock.Object);

        // Act
        var result = await behavior.Handle(
            new SecuredRequest(),
            NextReturning("ok"),
            CancellationToken.None);

        // Assert
        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_WhenSecuredRequestAndUnauthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        var currentUserMock = MockCurrentUserServiceHelper.CreateUnauthenticatedUser();
        var behavior = new AuthorizationBehavior<SecuredRequest, string>(currentUserMock.Object);

        // Act
        var act = async () => await behavior.Handle(
            new SecuredRequest(),
            NextReturning("ok"),
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenAdminRequestAndUserIsAdmin_CallsNext()
    {
        // Arrange
        var currentUserMock = MockCurrentUserServiceHelper.CreateAdminUser();
        var behavior = new AuthorizationBehavior<AdminRequest, string>(currentUserMock.Object);

        // Act
        var result = await behavior.Handle(
            new AdminRequest(),
            NextReturning("ok"),
            CancellationToken.None);

        // Assert
        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_WhenAdminRequestAndUserIsNotAdmin_ThrowsForbiddenException()
    {
        // Arrange
        var currentUserMock = MockCurrentUserServiceHelper.CreateAuthenticatedUser(isAdmin: false);
        var behavior = new AuthorizationBehavior<AdminRequest, string>(currentUserMock.Object);

        // Act
        var act = async () => await behavior.Handle(
            new AdminRequest(),
            NextReturning("ok"),
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenAdminRequestAndUnauthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        var currentUserMock = MockCurrentUserServiceHelper.CreateUnauthenticatedUser();
        var behavior = new AuthorizationBehavior<AdminRequest, string>(currentUserMock.Object);

        // Act
        var act = async () => await behavior.Handle(
            new AdminRequest(),
            NextReturning("ok"),
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
