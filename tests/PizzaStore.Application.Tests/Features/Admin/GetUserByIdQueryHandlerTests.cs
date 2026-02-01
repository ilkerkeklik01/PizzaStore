using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using PizzaStore.Application.Features.Queries.Admin.GetUserById;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Tests.Features.Admin;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _handler = new GetUserByIdQueryHandler(_userManagerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ReturnsUserWithRoles()
    {
        // Arrange
        var userId = "user-123";
        var user = TestDataBuilder.User()
            .WithId(userId)
            .WithEmail("testuser@example.com")
            .Build();

        var roles = new List<string> { "User", "Admin" };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Email.Should().Be("testuser@example.com");
        result.UserName.Should().Be("testuser@example.com");
        result.Roles.Should().HaveCount(2);
        result.Roles.Should().Contain("User");
        result.Roles.Should().Contain("Admin");
        
        _userManagerMock.Verify(x => x.FindByIdAsync(userId), Times.Once);
        _userManagerMock.Verify(x => x.GetRolesAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserExistsWithNoRoles_ReturnsUserWithEmptyRolesList()
    {
        // Arrange
        var userId = "user-456";
        var user = TestDataBuilder.User()
            .WithId(userId)
            .WithEmail("noroles@example.com")
            .Build();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Roles.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "non-existent-user";

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        var query = new GetUserByIdQuery(userId);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"User with ID '{userId}' not found");
        
        _userManagerMock.Verify(x => x.FindByIdAsync(userId), Times.Once);
        _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }
}
