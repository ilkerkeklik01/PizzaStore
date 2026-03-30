using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using PizzaStore.Application.Features.Topping.Commands.CreateTopping;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;
using ValidationException = PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException;

namespace PizzaStore.Application.Tests.Features.Topping.Commands;

public class CreateToppingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IToppingRepository> _toppingRepositoryMock;
    private readonly Mock<IValidator<CreateToppingDto>> _validatorMock;
    private readonly CreateToppingCommandHandler _handler;

    public CreateToppingCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _toppingRepositoryMock = new Mock<IToppingRepository>();
        _validatorMock = new Mock<IValidator<CreateToppingDto>>();

        _unitOfWorkMock.Setup(x => x.Toppings).Returns(_toppingRepositoryMock.Object);

        _handler = new CreateToppingCommandHandler(
            _unitOfWorkMock.Object,
            _validatorMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidationPasses_CreatesAndReturnsTopping()
    {
        // Arrange
        var dto = new CreateToppingDto { Name = "Extra Cheese", Price = 1.50m };
        var command = new CreateToppingCommand(dto);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _toppingRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Topping>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Extra Cheese");
        result.Price.Should().Be(1.50m);
        result.Id.Should().NotBeNullOrEmpty();
        result.Message.Should().Contain("Extra Cheese");
        result.Message.Should().Contain("created successfully");

        _toppingRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Domain.Entities.Topping>(
                t => t.Name == "Extra Cheese" && t.Price == 1.50m && t.IsAvailable)),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreateToppingDto { Name = "", Price = -1.00m };
        var command = new CreateToppingCommand(dto);

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
            x => x.AddAsync(It.IsAny<Domain.Entities.Topping>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValidationFailsWithSingleError_ThrowsValidationExceptionWithMessage()
    {
        // Arrange
        var dto = new CreateToppingDto { Name = "", Price = 1.50m };
        var command = new CreateToppingCommand(dto);

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name cannot be empty")
        };
        var validationResult = new ValidationResult(validationFailures);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Name cannot be empty");
    }
}
