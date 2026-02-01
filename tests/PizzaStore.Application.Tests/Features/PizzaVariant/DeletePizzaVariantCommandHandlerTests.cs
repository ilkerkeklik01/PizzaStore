using FluentAssertions;
using Moq;
using PizzaStore.Application.Features.Commands.PizzaVariant.DeletePizzaVariant;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Tests.Features.PizzaVariant;

public class DeletePizzaVariantCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPizzaVariantRepository> _pizzaVariantRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly DeletePizzaVariantCommandHandler _handler;

    public DeletePizzaVariantCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _pizzaVariantRepositoryMock = new Mock<IPizzaVariantRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _unitOfWorkMock.Setup(x => x.PizzaVariants).Returns(_pizzaVariantRepositoryMock.Object);
        
        _handler = new DeletePizzaVariantCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsAdmin_AndVariantExists_SoftDeletesAndReturnsSuccess()
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

        var command = new DeletePizzaVariantCommand(variantId);

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByIdAsync(variantId))
            .ReturnsAsync(existingVariant);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Contain("successfully deleted");
        result.Message.Should().Contain("marked as unavailable");

        existingVariant.IsAvailable.Should().BeFalse();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAdmin_ThrowsUnauthorizedException()
    {
        // Arrange
        var variantId = "variant-123";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(false);

        var command = new DeletePizzaVariantCommand(variantId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Only administrators can delete pizza variants");

        _pizzaVariantRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<string>()), 
            Times.Never);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenVariantDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var variantId = "non-existent-variant";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var command = new DeletePizzaVariantCommand(variantId);

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByIdAsync(variantId))
            .ReturnsAsync((Domain.Entities.PizzaVariant?)null);

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
    public async Task Handle_WhenVariantIsAlreadyUnavailable_StillPerformsSoftDelete()
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

        var command = new DeletePizzaVariantCommand(variantId);

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByIdAsync(variantId))
            .ReturnsAsync(existingVariant);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        existingVariant.IsAvailable.Should().BeFalse();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDeletingDifferentSizeVariants_EachIsHandledIndependently()
    {
        // Arrange
        var variantId1 = "variant-1";
        var variantId2 = "variant-2";
        
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var variant1 = TestDataBuilder.PizzaVariant()
            .WithId(variantId1)
            .WithSize(PizzaSize.Small)
            .IsAvailable(true)
            .Build();

        var variant2 = TestDataBuilder.PizzaVariant()
            .WithId(variantId2)
            .WithSize(PizzaSize.Medium)
            .IsAvailable(true)
            .Build();

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByIdAsync(variantId1))
            .ReturnsAsync(variant1);

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByIdAsync(variantId2))
            .ReturnsAsync(variant2);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command1 = new DeletePizzaVariantCommand(variantId1);
        var command2 = new DeletePizzaVariantCommand(variantId2);

        // Act
        await _handler.Handle(command1, CancellationToken.None);
        await _handler.Handle(command2, CancellationToken.None);

        // Assert
        variant1.IsAvailable.Should().BeFalse();
        variant2.IsAvailable.Should().BeFalse();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WhenDeletingLastVariantOfPizza_SuccessfullyDeletes()
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

        var command = new DeletePizzaVariantCommand(variantId);

        _pizzaVariantRepositoryMock
            .Setup(x => x.GetByIdAsync(variantId))
            .ReturnsAsync(existingVariant);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Contain("successfully deleted");
        existingVariant.IsAvailable.Should().BeFalse();
    }
}
