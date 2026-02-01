using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PizzaStore.Application.Features.Auth.Commands.Login;
using PizzaStore.Core.Auth.DTOs;
using PizzaStore.Core.Auth.Interfaces;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;

namespace PizzaStore.Application.Tests.Features.Auth.Commands;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<LoginUserCommandHandler>> _loggerMock;
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<LoginUserCommandHandler>>();
        
        _handler = new LoginUserCommandHandler(
            _authServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ReturnsAuthResponse()
    {
        // Arrange
        var loginDto = new LoginUserDto("user@example.com", "ValidPassword123!");
        var command = new LoginUserCommand(loginDto);

        var user = new UserResponseDto("user-123", "John", "Doe", "user@example.com");
        var expectedResponse = new AuthResponseDto("jwt-token-here", user);

        _authServiceMock
            .Setup(x => x.LoginAsync(loginDto))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("jwt-token-here");
        result.User.Email.Should().Be("user@example.com");
        result.User.Id.Should().Be("user-123");

        _authServiceMock.Verify(x => x.LoginAsync(loginDto), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAuthServiceThrowsUnauthorizedException_PropagatesException()
    {
        // Arrange
        var loginDto = new LoginUserDto("user@example.com", "InvalidPassword");
        var command = new LoginUserCommand(loginDto);

        _authServiceMock
            .Setup(x => x.LoginAsync(loginDto))
            .ThrowsAsync(new UnauthorizedException("Invalid email or password"));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password");

        _authServiceMock.Verify(x => x.LoginAsync(loginDto), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAuthServiceThrowsNotFoundException_PropagatesException()
    {
        // Arrange
        var loginDto = new LoginUserDto("nonexistent@example.com", "Password123!");
        var command = new LoginUserCommand(loginDto);

        _authServiceMock
            .Setup(x => x.LoginAsync(loginDto))
            .ThrowsAsync(new NotFoundException("User not found"));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User not found");

        _authServiceMock.Verify(x => x.LoginAsync(loginDto), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAuthServiceThrowsGenericException_PropagatesException()
    {
        // Arrange
        var loginDto = new LoginUserDto("user@example.com", "Password123!");
        var command = new LoginUserCommand(loginDto);

        _authServiceMock
            .Setup(x => x.LoginAsync(loginDto))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database connection failed");

        _authServiceMock.Verify(x => x.LoginAsync(loginDto), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidCredentials_DelegatesToAuthService()
    {
        // Arrange
        var loginDto = new LoginUserDto("admin@example.com", "AdminPassword123!");
        var command = new LoginUserCommand(loginDto);

        var user = new UserResponseDto("admin-123", "Admin", "User", "admin@example.com");
        var expectedResponse = new AuthResponseDto("admin-jwt-token", user);

        _authServiceMock
            .Setup(x => x.LoginAsync(loginDto))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        _authServiceMock.Verify(x => x.LoginAsync(loginDto), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAuthServiceSucceeds_LogsInformation()
    {
        // Arrange
        var loginDto = new LoginUserDto("user@example.com", "Password123!");
        var command = new LoginUserCommand(loginDto);

        var user = new UserResponseDto("user-123", "John", "Doe", "user@example.com");
        var expectedResponse = new AuthResponseDto("jwt-token", user);

        _authServiceMock
            .Setup(x => x.LoginAsync(loginDto))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("logged in successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAuthServiceFails_LogsWarning()
    {
        // Arrange
        var loginDto = new LoginUserDto("user@example.com", "WrongPassword");
        var command = new LoginUserCommand(loginDto);

        _authServiceMock
            .Setup(x => x.LoginAsync(loginDto))
            .ThrowsAsync(new UnauthorizedException("Invalid credentials"));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed login attempt")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMultipleDtoFieldsProvided_PassesAllFieldsToAuthService()
    {
        // Arrange
        var loginDto = new LoginUserDto("test@example.com", "TestPassword123!");
        var command = new LoginUserCommand(loginDto);

        var user = new UserResponseDto("test-id", "Test", "User", "test@example.com");
        var expectedResponse = new AuthResponseDto("test-token", user);

        _authServiceMock
            .Setup(x => x.LoginAsync(It.Is<LoginUserDto>(
                dto => dto.Email == "test@example.com" && dto.Password == "TestPassword123!")))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        _authServiceMock.Verify(
            x => x.LoginAsync(It.Is<LoginUserDto>(
                dto => dto.Email == "test@example.com" && dto.Password == "TestPassword123!")), 
            Times.Once);
    }
}
