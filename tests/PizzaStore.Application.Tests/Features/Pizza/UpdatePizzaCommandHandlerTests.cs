using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using PizzaStore.Application.Features.Commands.Pizza.UpdatePizza;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;
using ValidationException = PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException;

namespace PizzaStore.Application.Tests.Features.Pizza;

public class UpdatePizzaCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPizzaRepository> _pizzaRepositoryMock;
    private readonly Mock<IValidator<UpdatePizzaDto>> _validatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdatePizzaCommandHandler _handler;

    public UpdatePizzaCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _pizzaRepositoryMock = new Mock<IPizzaRepository>();
        _validatorMock = new Mock<IValidator<UpdatePizzaDto>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _unitOfWorkMock.Setup(x => x.Pizzas).Returns(_pizzaRepositoryMock.Object);
        
        _handler = new UpdatePizzaCommandHandler(
            _unitOfWorkMock.Object,
            _validatorMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAdminAndValidDto_UpdatesPizzaProperties()
    {
        // Arrange
        var pizzaId = "pizza-123";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var existingPizza = TestDataBuilder.Pizza()
            .WithId(pizzaId)
            .WithName("Old Name")
            .WithDescription("Old Description")
            .WithType(PizzaType.Vegetarian)
            .WithImageUrl("https://example.com/old.jpg")
            .IsAvailable(true)
            .Build();

        var dto = new UpdatePizzaDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Type = PizzaType.MeatLovers,
            ImageUrl = "https://example.com/new.jpg",
            IsAvailable = false
        };
        var command = new UpdatePizzaCommand(pizzaId, dto);

        _pizzaRepositoryMock
            .Setup(x => x.GetByIdAsync(pizzaId))
            .ReturnsAsync(existingPizza);

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
        result.Id.Should().Be(pizzaId);
        result.Name.Should().Be("Updated Name");
        result.Message.Should().Contain("Updated Name");
        result.Message.Should().Contain("updated successfully");

        existingPizza.Name.Should().Be("Updated Name");
        existingPizza.Description.Should().Be("Updated Description");
        existingPizza.Type.Should().Be(PizzaType.MeatLovers);
        existingPizza.ImageUrl.Should().Be("https://example.com/new.jpg");
        existingPizza.IsAvailable.Should().BeFalse();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotAdmin_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(false);

        var dto = new UpdatePizzaDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Type = PizzaType.Vegetarian,
            ImageUrl = "https://example.com/new.jpg",
            IsAvailable = true
        };
        var command = new UpdatePizzaCommand("pizza-123", dto);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Only administrators can update pizzas");

        _validatorMock.Verify(
            x => x.ValidateAsync(It.IsAny<UpdatePizzaDto>(), It.IsAny<CancellationToken>()), 
            Times.Never);
        _pizzaRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<string>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ThrowsValidationException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var dto = new UpdatePizzaDto
        {
            Name = "",
            Description = "Updated Description",
            Type = PizzaType.Vegetarian,
            ImageUrl = "",
            IsAvailable = true
        };
        var command = new UpdatePizzaCommand("pizza-123", dto);

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required"),
            new ValidationFailure("ImageUrl", "Image URL is required")
        };
        var validationResult = new ValidationResult(validationFailures);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Name is required*");

        _pizzaRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<string>()), 
            Times.Never);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPizzaNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var pizzaId = "non-existent-pizza";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var dto = new UpdatePizzaDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Type = PizzaType.Vegetarian,
            ImageUrl = "https://example.com/new.jpg",
            IsAvailable = true
        };
        var command = new UpdatePizzaCommand(pizzaId, dto);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _pizzaRepositoryMock
            .Setup(x => x.GetByIdAsync(pizzaId))
            .ReturnsAsync((Domain.Entities.Pizza?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Pizza with ID '{pizzaId}' not found.");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAdminUpdatesAvailability_ChangesIsAvailableFlag()
    {
        // Arrange
        var pizzaId = "pizza-123";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var existingPizza = TestDataBuilder.Pizza()
            .WithId(pizzaId)
            .WithName("Margherita")
            .IsAvailable(true)
            .Build();

        var dto = new UpdatePizzaDto
        {
            Name = "Margherita",
            Description = "Classic pizza",
            Type = PizzaType.Vegetarian,
            ImageUrl = "https://example.com/margherita.jpg",
            IsAvailable = false
        };
        var command = new UpdatePizzaCommand(pizzaId, dto);

        _pizzaRepositoryMock
            .Setup(x => x.GetByIdAsync(pizzaId))
            .ReturnsAsync(existingPizza);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingPizza.IsAvailable.Should().BeFalse();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAdminUpdatesType_ChangesPizzaType()
    {
        // Arrange
        var pizzaId = "pizza-123";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var existingPizza = TestDataBuilder.Pizza()
            .WithId(pizzaId)
            .WithName("Pepperoni")
            .WithType(PizzaType.MeatLovers)
            .Build();

        var dto = new UpdatePizzaDto
        {
            Name = "Pepperoni",
            Description = "Now vegetarian",
            Type = PizzaType.Vegetarian,
            ImageUrl = "https://example.com/pepperoni.jpg",
            IsAvailable = true
        };
        var command = new UpdatePizzaCommand(pizzaId, dto);

        _pizzaRepositoryMock
            .Setup(x => x.GetByIdAsync(pizzaId))
            .ReturnsAsync(existingPizza);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingPizza.Type.Should().Be(PizzaType.Vegetarian);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
