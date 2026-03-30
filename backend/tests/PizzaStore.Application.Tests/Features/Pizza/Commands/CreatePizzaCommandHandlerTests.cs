using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using PizzaStore.Application.Features.Pizza.Commands.CreatePizza;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;
using ValidationException = PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException;

namespace PizzaStore.Application.Tests.Features.Pizza.Commands;

public class CreatePizzaCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPizzaRepository> _pizzaRepositoryMock;
    private readonly Mock<IValidator<CreatePizzaDto>> _validatorMock;
    private readonly Mock<ILogger<CreatePizzaCommandHandler>> _loggerMock;
    private readonly CreatePizzaCommandHandler _handler;

    public CreatePizzaCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _pizzaRepositoryMock = new Mock<IPizzaRepository>();
        _validatorMock = new Mock<IValidator<CreatePizzaDto>>();
        _loggerMock = new Mock<ILogger<CreatePizzaCommandHandler>>();

        _unitOfWorkMock.Setup(x => x.Pizzas).Returns(_pizzaRepositoryMock.Object);

        _handler = new CreatePizzaCommandHandler(
            _unitOfWorkMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidDto_CreatesPizzaWithVariants()
    {
        // Arrange
        var dto = new CreatePizzaDto
        {
            Name = "Margherita",
            Description = "Classic Italian pizza",
            Type = PizzaType.Vegetarian,
            ImageUrl = "https://example.com/margherita.jpg",
            Variants = new List<PizzaVariantDto>
            {
                new PizzaVariantDto { Size = PizzaSize.Small, Price = 8.99m },
                new PizzaVariantDto { Size = PizzaSize.Medium, Price = 12.99m },
                new PizzaVariantDto { Size = PizzaSize.Large, Price = 15.99m }
            }
        };
        var command = new CreatePizzaCommand(dto);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _pizzaRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Pizza>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.Name.Should().Be("Margherita");
        result.Message.Should().Contain("Margherita");
        result.Message.Should().Contain("created successfully");
        result.Message.Should().Contain("3 variant(s)");

        _pizzaRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Domain.Entities.Pizza>(
                p => p.Name == "Margherita"
                    && p.Description == "Classic Italian pizza"
                    && p.Type == PizzaType.Vegetarian
                    && p.IsAvailable == true
                    && p.Variants.Count == 3)),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreatePizzaDto
        {
            Name = "",
            Description = "Invalid pizza",
            Type = PizzaType.Vegetarian,
            ImageUrl = "",
            Variants = new List<PizzaVariantDto>()
        };
        var command = new CreatePizzaCommand(dto);

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required"),
            new ValidationFailure("ImageUrl", "Image URL is required"),
            new ValidationFailure("Variants", "At least one variant is required")
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
            x => x.AddAsync(It.IsAny<Domain.Entities.Pizza>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSingleVariant_CreatesPizzaWithOneVariant()
    {
        // Arrange
        var dto = new CreatePizzaDto
        {
            Name = "Hawaiian",
            Description = "Ham and pineapple",
            Type = PizzaType.Hawaiian,
            ImageUrl = "https://example.com/hawaiian.jpg",
            Variants = new List<PizzaVariantDto>
            {
                new PizzaVariantDto { Size = PizzaSize.Large, Price = 14.99m }
            }
        };
        var command = new CreatePizzaCommand(dto);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _pizzaRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Pizza>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Contain("1 variant(s)");

        _pizzaRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Domain.Entities.Pizza>(
                p => p.Variants.Count == 1
                    && p.Variants.First().Size == PizzaSize.Large
                    && p.Variants.First().Price == 14.99m
                    && p.Variants.First().IsAvailable == true)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMultipleVariants_EnsuresAllVariantsHaveCorrectPizzaId()
    {
        // Arrange
        var dto = new CreatePizzaDto
        {
            Name = "BBQ Chicken",
            Description = "Barbecue chicken pizza",
            Type = PizzaType.MeatLovers,
            ImageUrl = "https://example.com/bbq.jpg",
            Variants = new List<PizzaVariantDto>
            {
                new PizzaVariantDto { Size = PizzaSize.Small, Price = 9.99m },
                new PizzaVariantDto { Size = PizzaSize.Medium, Price = 13.99m }
            }
        };
        var command = new CreatePizzaCommand(dto);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _pizzaRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Pizza>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _pizzaRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Domain.Entities.Pizza>(
                p => p.Variants.All(v => v.PizzaId == p.Id))),
            Times.Once);
    }
}
