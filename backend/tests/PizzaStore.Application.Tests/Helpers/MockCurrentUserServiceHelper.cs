using Moq;
using PizzaStore.Application.Services;

namespace PizzaStore.Application.Tests.Helpers;

/// <summary>
/// Helper for setting up common ICurrentUserService mock scenarios
/// </summary>
public static class MockCurrentUserServiceHelper
{
    public static Mock<ICurrentUserService> CreateAuthenticatedUser(
        string userId = "test-user-id",
        string email = "test@example.com",
        bool isAdmin = false)
    {
        var mock = new Mock<ICurrentUserService>();
        
        mock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        mock.Setup(x => x.GetCurrentUserEmail()).Returns(email);
        mock.Setup(x => x.IsAuthenticated()).Returns(true);
        mock.Setup(x => x.IsInRole("Admin")).Returns(isAdmin);
        mock.Setup(x => x.IsInRole("User")).Returns(true);
        
        return mock;
    }

    public static Mock<ICurrentUserService> CreateUnauthenticatedUser()
    {
        var mock = new Mock<ICurrentUserService>();
        
        mock.Setup(x => x.GetCurrentUserId()).Returns((string?)null);
        mock.Setup(x => x.GetCurrentUserEmail()).Returns((string?)null);
        mock.Setup(x => x.IsAuthenticated()).Returns(false);
        mock.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(false);
        
        return mock;
    }

    public static Mock<ICurrentUserService> CreateAdminUser(
        string userId = "admin-user-id",
        string email = "admin@example.com")
    {
        return CreateAuthenticatedUser(userId, email, isAdmin: true);
    }
}
