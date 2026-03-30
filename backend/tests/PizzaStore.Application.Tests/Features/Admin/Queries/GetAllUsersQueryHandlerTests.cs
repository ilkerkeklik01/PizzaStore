using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using PizzaStore.Application.Features.Admin.Queries.GetAllUsers;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Tests.Features.Admin.Queries;

public class GetAllUsersQueryHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryHandlerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _handler = new GetAllUsersQueryHandler(_userManagerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUsersExist_ReturnsAllUsersOrderedByUsername()
    {
        // Arrange
        var user1 = TestDataBuilder.User()
            .WithId("user-1")
            .WithEmail("charlie@example.com")
            .Build();

        var user2 = TestDataBuilder.User()
            .WithId("user-2")
            .WithEmail("alice@example.com")
            .Build();

        var user3 = TestDataBuilder.User()
            .WithId("user-3")
            .WithEmail("bob@example.com")
            .Build();

        var users = new List<ApplicationUser> { user1, user2, user3 };

        _userManagerMock.Setup(x => x.Users)
            .Returns(users.AsQueryable());

        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "User" });

        var query = new GetAllUsersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].Email.Should().Be("alice@example.com");
        result[1].Email.Should().Be("bob@example.com");
        result[2].Email.Should().Be("charlie@example.com");
        
        _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Exactly(3));
    }

    [Fact]
    public async Task Handle_WhenUsersHaveDifferentRoles_ReturnsUsersWithCorrectRoles()
    {
        // Arrange
        var adminUser = TestDataBuilder.User()
            .WithId("admin-1")
            .WithEmail("admin@example.com")
            .Build();

        var regularUser = TestDataBuilder.User()
            .WithId("user-1")
            .WithEmail("user@example.com")
            .Build();

        var users = new List<ApplicationUser> { adminUser, regularUser };

        _userManagerMock.Setup(x => x.Users)
            .Returns(users.AsQueryable());

        _userManagerMock.Setup(x => x.GetRolesAsync(adminUser))
            .ReturnsAsync(new List<string> { "Admin", "User" });

        _userManagerMock.Setup(x => x.GetRolesAsync(regularUser))
            .ReturnsAsync(new List<string> { "User" });

        var query = new GetAllUsersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var adminDto = result.First(u => u.Email == "admin@example.com");
        adminDto.Roles.Should().Contain("Admin");
        adminDto.Roles.Should().Contain("User");
        
        var userDto = result.First(u => u.Email == "user@example.com");
        userDto.Roles.Should().NotContain("Admin");
        userDto.Roles.Should().Contain("User");
    }

    [Fact]
    public async Task Handle_WhenNoUsersExist_ReturnsEmptyList()
    {
        // Arrange
        var users = new List<ApplicationUser>();

        _userManagerMock.Setup(x => x.Users)
            .Returns(users.AsQueryable());

        var query = new GetAllUsersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
