using FluentAssertions;
using Moq;
using PizzaStore.Application.Features.Topping.Commands.DeleteTopping;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Tests.Features.Topping.Commands;

public class DeleteToppingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IToppingRepository> _toppingRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly DeleteToppingCommandHandler _handler;

    public DeleteToppingCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _toppingRepositoryMock = new Mock<IToppingRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _unitOfWorkMock.Setup(x => x.Toppings).Returns(_toppingRepositoryMock.Object);
        
        _handler = new DeleteToppingCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsAdmin_AndToppingExists_SoftDeletesAndReturnsSuccess()
    {
        // Arrange
        var toppingId = "topping-123";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var existingTopping = TestDataBuilder.Topping()
            .WithId(toppingId)
            .WithName("Mushrooms")
            .WithPrice(1.50m)
            .IsAvailable(true)
            .Build();

        var command = new DeleteToppingCommand(toppingId);

        _toppingRepositoryMock
            .Setup(x => x.GetByIdAsync(toppingId))
            .ReturnsAsync(existingTopping);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Contain("successfully deleted");
        result.Message.Should().Contain("marked as unavailable");

        existingTopping.IsAvailable.Should().BeFalse();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAdmin_ThrowsUnauthorizedException()
    {
        // Arrange
        var toppingId = "topping-123";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(false);

        var command = new DeleteToppingCommand(toppingId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Only administrators can delete toppings");

        _toppingRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<string>()), 
            Times.Never);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenToppingDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var toppingId = "non-existent-topping";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var command = new DeleteToppingCommand(toppingId);

        _toppingRepositoryMock
            .Setup(x => x.GetByIdAsync(toppingId))
            .ReturnsAsync((Domain.Entities.Topping?)null);

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
    public async Task Handle_WhenToppingIsAlreadyUnavailable_StillPerformsSoftDelete()
    {
        // Arrange
        var toppingId = "topping-123";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var existingTopping = TestDataBuilder.Topping()
            .WithId(toppingId)
            .WithName("Olives")
            .WithPrice(1.25m)
            .IsAvailable(false)
            .Build();

        var command = new DeleteToppingCommand(toppingId);

        _toppingRepositoryMock
            .Setup(x => x.GetByIdAsync(toppingId))
            .ReturnsAsync(existingTopping);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        existingTopping.IsAvailable.Should().BeFalse();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDeletingMultipleToppings_EachIsHandledIndependently()
    {
        // Arrange
        var toppingId1 = "topping-1";
        var toppingId2 = "topping-2";
        
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var topping1 = TestDataBuilder.Topping()
            .WithId(toppingId1)
            .WithName("Peppers")
            .IsAvailable(true)
            .Build();

        var topping2 = TestDataBuilder.Topping()
            .WithId(toppingId2)
            .WithName("Onions")
            .IsAvailable(true)
            .Build();

        _toppingRepositoryMock
            .Setup(x => x.GetByIdAsync(toppingId1))
            .ReturnsAsync(topping1);

        _toppingRepositoryMock
            .Setup(x => x.GetByIdAsync(toppingId2))
            .ReturnsAsync(topping2);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command1 = new DeleteToppingCommand(toppingId1);
        var command2 = new DeleteToppingCommand(toppingId2);

        // Act
        await _handler.Handle(command1, CancellationToken.None);
        await _handler.Handle(command2, CancellationToken.None);

        // Assert
        topping1.IsAvailable.Should().BeFalse();
        topping2.IsAvailable.Should().BeFalse();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
