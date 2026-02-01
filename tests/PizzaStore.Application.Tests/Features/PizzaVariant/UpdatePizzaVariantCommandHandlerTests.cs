using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using PizzaStore.Application.Features.Commands.PizzaVariant.UpdatePizzaVariant;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;
using ValidationException = PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException;

namespace PizzaStore.Application.Tests.Features.PizzaVariant;

public class UpdatePizzaVariantCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPizzaVariantRepository> _pizzaVariantRepositoryMock;
    private readonly Mock<IValidator<UpdatePizzaVariantDto>> _validatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdatePizzaVariantCommandHandler _handler;

    public UpdatePizzaVariantCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _pizzaVariantRepositoryMock = new Mock<IPizzaVariantRepository>();
        _validatorMock = new Mock<IValidator<UpdatePizzaVariantDto>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _unitOfWorkMock.Setup(x => x.PizzaVariants).Returns(_pizzaVariantRepositoryMock.Object);
        
        _handler = new UpdatePizzaVariantCommandHandler(
            _unitOfWorkMock.Object,
            _validatorMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsAdmin_AndVariantExists_AndValidationPasses_UpdatesAndReturnsVariant()
    {
        // Arrange
        var variantId = "variant-123";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var existingVariant = TestDataBuilder.PizzaVariant()
            .WithId(variantId)
            .WithSize(PizzaSize.Medium)
            .WithPrice(12.99m)
            .IsAvailable(true)
            .Build();

        var dto = new UpdatePizzaVariantDto { Price = 14.99m, IsAvailable = false };
        var command = new UpdatePizzaVariantCommand(variantId, dto);

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByIdAsync(variantId))
            .ReturnsAsync(existingVariant);

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
        result.Id.Should().Be(variantId);
        result.Price.Should().Be(14.99m);
        result.IsAvailable.Should().BeFalse();
        result.Message.Should().Contain("updated successfully");

        existingVariant.Price.Should().Be(14.99m);
        existingVariant.IsAvailable.Should().BeFalse();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAdmin_ThrowsUnauthorizedException()
    {
        // Arrange
        var variantId = "variant-123";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(false);

        var dto = new UpdatePizzaVariantDto { Price = 14.99m };
        var command = new UpdatePizzaVariantCommand(variantId, dto);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Only administrators can update pizza variants");

        _validatorMock.Verify(
            x => x.ValidateAsync(It.IsAny<UpdatePizzaVariantDto>(), It.IsAny<CancellationToken>()), 
            Times.Never);
        _pizzaVariantRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<string>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenVariantDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var variantId = "non-existent-variant";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var dto = new UpdatePizzaVariantDto { Price = 14.99m };
        var command = new UpdatePizzaVariantCommand(variantId, dto);

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByIdAsync(variantId))
            .ReturnsAsync((Domain.Entities.PizzaVariant?)null);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Pizza variant with ID '{variantId}' not found.");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ThrowsValidationException()
    {
        // Arrange
        var variantId = "variant-123";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var dto = new UpdatePizzaVariantDto { Price = -1.00m };
        var command = new UpdatePizzaVariantCommand(variantId, dto);

        var validationFailures = new List<ValidationFailure>
        {
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
            .WithMessage("Price must be greater than 0");

        _pizzaVariantRepositoryMock.Verify(
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
        var variantId = "variant-123";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var existingVariant = TestDataBuilder.PizzaVariant()
            .WithId(variantId)
            .WithSize(PizzaSize.Large)
            .WithPrice(15.99m)
            .IsAvailable(false)
            .Build();

        var dto = new UpdatePizzaVariantDto { Price = 16.99m, IsAvailable = true };
        var command = new UpdatePizzaVariantCommand(variantId, dto);

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByIdAsync(variantId))
            .ReturnsAsync(existingVariant);

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
        result.Price.Should().Be(16.99m);
        existingVariant.IsAvailable.Should().BeTrue();
        existingVariant.Price.Should().Be(16.99m);
    }

    [Fact]
    public async Task Handle_WhenUpdatingPriceOnly_KeepsAvailabilityUnchanged()
    {
        // Arrange
        var variantId = "variant-123";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var existingVariant = TestDataBuilder.PizzaVariant()
            .WithId(variantId)
            .WithSize(PizzaSize.Small)
            .WithPrice(9.99m)
            .IsAvailable(true)
            .Build();

        var dto = new UpdatePizzaVariantDto { Price = 10.99m, IsAvailable = true };
        var command = new UpdatePizzaVariantCommand(variantId, dto);

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByIdAsync(variantId))
            .ReturnsAsync(existingVariant);

        _validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Price.Should().Be(10.99m);
        result.IsAvailable.Should().BeTrue();
        existingVariant.Price.Should().Be(10.99m);
        existingVariant.IsAvailable.Should().BeTrue();
    }
}
