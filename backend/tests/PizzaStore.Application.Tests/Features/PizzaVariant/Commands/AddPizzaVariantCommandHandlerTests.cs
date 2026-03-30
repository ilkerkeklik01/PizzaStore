using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using PizzaStore.Application.Features.PizzaVariant.Commands.AddPizzaVariant;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;
using ValidationException = PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException;

namespace PizzaStore.Application.Tests.Features.PizzaVariant.Commands;

public class AddPizzaVariantCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPizzaRepository> _pizzaRepositoryMock;
    private readonly Mock<IPizzaVariantRepository> _pizzaVariantRepositoryMock;
    private readonly Mock<IValidator<AddPizzaVariantDto>> _validatorMock;
    private readonly AddPizzaVariantCommandHandler _handler;

    public AddPizzaVariantCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _pizzaRepositoryMock = new Mock<IPizzaRepository>();
        _pizzaVariantRepositoryMock = new Mock<IPizzaVariantRepository>();
        _validatorMock = new Mock<IValidator<AddPizzaVariantDto>>();

        _unitOfWorkMock.Setup(x => x.Pizzas).Returns(_pizzaRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.PizzaVariants).Returns(_pizzaVariantRepositoryMock.Object);

        _handler = new AddPizzaVariantCommandHandler(
            _unitOfWorkMock.Object,
            _validatorMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidationPasses_AndPizzaExists_AndNoConflict_CreatesVariant()
    {
        // Arrange
        var pizzaId = "pizza-123";

        var pizza = TestDataBuilder.Pizza()
            .WithId(pizzaId)
            .WithName("Margherita")
            .WithVariants()
            .Build();

        var dto = new AddPizzaVariantDto { PizzaId = pizzaId, Size = PizzaSize.Large, Price = 15.99m };
        var command = new AddPizzaVariantCommand(dto);

        _pizzaRepositoryMock
            .Setup(x => x.GetByIdAsync(pizzaId))
            .ReturnsAsync(pizza);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _pizzaVariantRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Domain.Entities.PizzaVariant>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.Size.Should().Be("Large");
        result.Price.Should().Be(15.99m);
        result.Message.Should().Contain("Large");
        result.Message.Should().Contain("Margherita");
        result.Message.Should().Contain("added successfully");

        _pizzaVariantRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Domain.Entities.PizzaVariant>(
                v => v.PizzaId == pizzaId && v.Size == PizzaSize.Large && v.Price == 15.99m && v.IsAvailable)),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ThrowsValidationException()
    {
        // Arrange
        var dto = new AddPizzaVariantDto { PizzaId = "", Size = PizzaSize.Medium, Price = -1.00m };
        var command = new AddPizzaVariantCommand(dto);

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("PizzaId", "Pizza ID is required"),
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
            .WithMessage("*Pizza ID is required*Price must be greater than 0*");

        _pizzaRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<string>()),
            Times.Never);
        _pizzaVariantRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.PizzaVariant>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPizzaDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var pizzaId = "non-existent-pizza";

        var dto = new AddPizzaVariantDto { PizzaId = pizzaId, Size = PizzaSize.Medium, Price = 12.99m };
        var command = new AddPizzaVariantCommand(dto);

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

        _pizzaVariantRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.PizzaVariant>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenVariantWithSameSizeAlreadyExists_ThrowsValidationException()
    {
        // Arrange
        var pizzaId = "pizza-123";

        var existingVariant = TestDataBuilder.PizzaVariant()
            .WithPizzaId(pizzaId)
            .WithSize(PizzaSize.Medium)
            .WithPrice(12.99m)
            .IsAvailable(true)
            .Build();

        var pizza = TestDataBuilder.Pizza()
            .WithId(pizzaId)
            .WithName("Pepperoni")
            .WithVariants(existingVariant)
            .Build();

        var dto = new AddPizzaVariantDto { PizzaId = pizzaId, Size = PizzaSize.Medium, Price = 13.99m };
        var command = new AddPizzaVariantCommand(dto);

        _pizzaRepositoryMock
            .Setup(x => x.GetByIdAsync(pizzaId))
            .ReturnsAsync(pizza);

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByPizzaIdAndSizeAsync(It.IsAny<string>(), It.IsAny<PizzaSize>()))
            .ReturnsAsync(existingVariant);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Medium*already exists*");

        _pizzaVariantRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.PizzaVariant>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenVariantWithSameSizeExistsButIsUnavailable_CreatesNewVariant()
    {
        // Arrange
        var pizzaId = "pizza-123";

        var existingVariant = TestDataBuilder.PizzaVariant()
            .WithPizzaId(pizzaId)
            .WithSize(PizzaSize.Medium)
            .WithPrice(12.99m)
            .IsAvailable(false)
            .Build();

        var pizza = TestDataBuilder.Pizza()
            .WithId(pizzaId)
            .WithName("Margherita")
            .WithVariants(existingVariant)
            .Build();

        var dto = new AddPizzaVariantDto { PizzaId = pizzaId, Size = PizzaSize.Medium, Price = 13.99m };
        var command = new AddPizzaVariantCommand(dto);

        _pizzaRepositoryMock
            .Setup(x => x.GetByIdAsync(pizzaId))
            .ReturnsAsync(pizza);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _pizzaVariantRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Domain.Entities.PizzaVariant>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _pizzaVariantRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.PizzaVariant>()),
            Times.Once);
    }
}
