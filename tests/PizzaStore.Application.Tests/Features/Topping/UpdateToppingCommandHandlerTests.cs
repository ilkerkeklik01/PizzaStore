using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using PizzaStore.Application.Features.Commands.Topping.UpdateTopping;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;
using ValidationException = PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException;

namespace PizzaStore.Application.Tests.Features.Topping;

public class UpdateToppingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IToppingRepository> _toppingRepositoryMock;
    private readonly Mock<IValidator<UpdateToppingDto>> _validatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdateToppingCommandHandler _handler;

    public UpdateToppingCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _toppingRepositoryMock = new Mock<IToppingRepository>();
        _validatorMock = new Mock<IValidator<UpdateToppingDto>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _unitOfWorkMock.Setup(x => x.Toppings).Returns(_toppingRepositoryMock.Object);
        
        _handler = new UpdateToppingCommandHandler(
            _unitOfWorkMock.Object,
            _validatorMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsAdmin_AndToppingExists_AndValidationPasses_UpdatesAndReturnsTopping()
    {
        // Arrange
        var toppingId = "topping-123";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var existingTopping = TestDataBuilder.Topping()
            .WithId(toppingId)
            .WithName("Old Name")
            .WithPrice(1.00m)
            .IsAvailable(true)
            .Build();

        var dto = new UpdateToppingDto { Name = "Updated Name", Price = 2.50m, IsAvailable = false };
        var command = new UpdateToppingCommand(toppingId, dto);

        _toppingRepositoryMock
            .Setup(x => x.GetByIdAsync(toppingId))
            .ReturnsAsync(existingTopping);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(toppingId);
        result.Name.Should().Be("Updated Name");
        result.Price.Should().Be(2.50m);
        result.IsAvailable.Should().BeFalse();
        result.Message.Should().Contain("Updated Name");
        result.Message.Should().Contain("updated successfully");

        existingTopping.Name.Should().Be("Updated Name");
        existingTopping.Price.Should().Be(2.50m);
        existingTopping.IsAvailable.Should().BeFalse();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAdmin_ThrowsUnauthorizedException()
    {
        // Arrange
        var toppingId = "topping-123";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(false);

        var dto = new UpdateToppingDto { Name = "Updated Name", Price = 2.50m };
        var command = new UpdateToppingCommand(toppingId, dto);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Only administrators can update toppings");

        _validatorMock.Verify(
            x => x.ValidateAsync(It.IsAny<UpdateToppingDto>(), It.IsAny<CancellationToken>()), 
            Times.Never);
        _toppingRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<string>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenToppingDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var toppingId = "non-existent-topping";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var dto = new UpdateToppingDto { Name = "Updated Name", Price = 2.50m };
        var command = new UpdateToppingCommand(toppingId, dto);

        _toppingRepositoryMock
            .Setup(x => x.GetByIdAsync(toppingId))
            .ReturnsAsync((Domain.Entities.Topping?)null);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Topping with ID '{toppingId}' not found.");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ThrowsValidationException()
    {
        // Arrange
        var toppingId = "topping-123";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var dto = new UpdateToppingDto { Name = "", Price = -1.00m };
        var command = new UpdateToppingCommand(toppingId, dto);

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required"),
            new ValidationFailure("Price", "Price must be greater than 0")
        };
        var validationResult = new ValidationResult(validationFailures);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Name is required*Price must be greater than 0*");

        _toppingRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<string>()), 
            Times.Never);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUpdatingToAvailableTrue_UpdatesCorrectly()
    {
        // Arrange
        var toppingId = "topping-123";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var existingTopping = TestDataBuilder.Topping()
            .WithId(toppingId)
            .WithName("Pepperoni")
            .WithPrice(2.00m)
            .IsAvailable(false)
            .Build();

        var dto = new UpdateToppingDto { Name = "Pepperoni", Price = 2.25m, IsAvailable = true };
        var command = new UpdateToppingCommand(toppingId, dto);

        _toppingRepositoryMock
            .Setup(x => x.GetByIdAsync(toppingId))
            .ReturnsAsync(existingTopping);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsAvailable.Should().BeTrue();
        existingTopping.IsAvailable.Should().BeTrue();
    }
}
