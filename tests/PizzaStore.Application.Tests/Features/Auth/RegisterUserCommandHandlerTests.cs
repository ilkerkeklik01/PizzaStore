using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PizzaStore.Application.Features.Commands.Auth.Register;
using PizzaStore.Core.Auth.DTOs;
using PizzaStore.Core.Auth.Interfaces;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;

namespace PizzaStore.Application.Tests.Features.Auth;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<RegisterUserCommandHandler>> _loggerMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<RegisterUserCommandHandler>>();
        
        _handler = new RegisterUserCommandHandler(
            _authServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRegistrationIsValid_ReturnsAuthResponse()
    {
        // Arrange
        var registerDto = new RegisterUserDto("John", "Doe", "newuser@example.com", "SecurePassword123!");
        var command = new RegisterUserCommand(registerDto);

        var user = new UserResponseDto("new-user-123", "John", "Doe", "newuser@example.com");
        var expectedResponse = new AuthResponseDto("jwt-token-here", user);

        _authServiceMock
            .Setup(x => x.RegisterAsync(registerDto))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("jwt-token-here");
        result.User.Email.Should().Be("newuser@example.com");
        result.User.Id.Should().Be("new-user-123");

        _authServiceMock.Verify(x => x.RegisterAsync(registerDto), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_PropagatesValidationException()
    {
        // Arrange
        var registerDto = new RegisterUserDto("John", "Doe", "existing@example.com", "Password123!");
        var command = new RegisterUserCommand(registerDto);

        _authServiceMock
            .Setup(x => x.RegisterAsync(registerDto))
            .ThrowsAsync(new ValidationException("Email already exists"));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Email already exists");

        _authServiceMock.Verify(x => x.RegisterAsync(registerDto), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsWeak_PropagatesValidationException()
    {
        // Arrange
        var registerDto = new RegisterUserDto("John", "Doe", "user@example.com", "weak");
        var command = new RegisterUserCommand(registerDto);

        _authServiceMock
            .Setup(x => x.RegisterAsync(registerDto))
            .ThrowsAsync(new ValidationException("Password does not meet requirements"));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Password does not meet requirements");

        _authServiceMock.Verify(x => x.RegisterAsync(registerDto), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAuthServiceThrowsGenericException_PropagatesException()
    {
        // Arrange
        var registerDto = new RegisterUserDto("John", "Doe", "user@example.com", "Password123!");
        var command = new RegisterUserCommand(registerDto);

        _authServiceMock
            .Setup(x => x.RegisterAsync(registerDto))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database connection failed");

        _authServiceMock.Verify(x => x.RegisterAsync(registerDto), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidData_DelegatesToAuthService()
    {
        // Arrange
        var registerDto = new RegisterUserDto("Admin", "User", "admin@example.com", "AdminPassword123!");
        var command = new RegisterUserCommand(registerDto);

        var user = new UserResponseDto("admin-123", "Admin", "User", "admin@example.com");
        var expectedResponse = new AuthResponseDto("admin-jwt-token", user);

        _authServiceMock
            .Setup(x => x.RegisterAsync(registerDto))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        _authServiceMock.Verify(x => x.RegisterAsync(registerDto), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAuthServiceSucceeds_LogsInformation()
    {
        // Arrange
        var registerDto = new RegisterUserDto("John", "Doe", "newuser@example.com", "Password123!");
        var command = new RegisterUserCommand(registerDto);

        var user = new UserResponseDto("user-123", "John", "Doe", "newuser@example.com");
        var expectedResponse = new AuthResponseDto("jwt-token", user);

        _authServiceMock
            .Setup(x => x.RegisterAsync(registerDto))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("registered successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAuthServiceFails_LogsWarning()
    {
        // Arrange
        var registerDto = new RegisterUserDto("John", "Doe", "user@example.com", "Password123!");
        var command = new RegisterUserCommand(registerDto);

        _authServiceMock
            .Setup(x => x.RegisterAsync(registerDto))
            .ThrowsAsync(new ValidationException("Email already exists"));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed registration attempt")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMultipleDtoFieldsProvided_PassesAllFieldsToAuthService()
    {
        // Arrange
        var registerDto = new RegisterUserDto("Test", "User", "test@example.com", "TestPassword123!");
        var command = new RegisterUserCommand(registerDto);

        var user = new UserResponseDto("test-id", "Test", "User", "test@example.com");
        var expectedResponse = new AuthResponseDto("test-token", user);

        _authServiceMock
            .Setup(x => x.RegisterAsync(It.Is<RegisterUserDto>(
                dto => dto.Email == "test@example.com" 
                    && dto.Password == "TestPassword123!" 
                    && dto.FirstName == "Test"
                    && dto.LastName == "User")))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        _authServiceMock.Verify(
            x => x.RegisterAsync(It.Is<RegisterUserDto>(
                dto => dto.Email == "test@example.com" 
                    && dto.Password == "TestPassword123!" 
                    && dto.FirstName == "Test"
                    && dto.LastName == "User")), 
            Times.Once);
    }
}
