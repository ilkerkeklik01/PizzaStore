using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PizzaStore.Application.Features.Commands.Pizza.DeletePizza;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Tests.Features.Pizza;

public class DeletePizzaCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPizzaRepository> _pizzaRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<DeletePizzaCommandHandler>> _loggerMock;
    private readonly DeletePizzaCommandHandler _handler;

    public DeletePizzaCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _pizzaRepositoryMock = new Mock<IPizzaRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<DeletePizzaCommandHandler>>();
        
        _unitOfWorkMock.Setup(x => x.Pizzas).Returns(_pizzaRepositoryMock.Object);
        
        _handler = new DeletePizzaCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAdminAndPizzaExists_SoftDeletesPizza()
    {
        // Arrange
        var pizzaId = "pizza-123";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var existingPizza = TestDataBuilder.Pizza()
            .WithId(pizzaId)
            .WithName("Margherita")
            .IsAvailable(true)
            .Build();

        var command = new DeletePizzaCommand(pizzaId);

        _pizzaRepositoryMock
            .Setup(x => x.GetByIdAsync(pizzaId))
            .ReturnsAsync(existingPizza);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Contain("successfully deleted");
        result.Message.Should().Contain("marked as unavailable");

        existingPizza.IsAvailable.Should().BeFalse();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotAdmin_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(false);

        var command = new DeletePizzaCommand("pizza-123");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Only administrators can delete pizzas");

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

        var command = new DeletePizzaCommand(pizzaId);

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
    public async Task Handle_WhenAdminDeletesAlreadyUnavailablePizza_StillSucceeds()
    {
        // Arrange
        var pizzaId = "pizza-123";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var existingPizza = TestDataBuilder.Pizza()
            .WithId(pizzaId)
            .WithName("Pepperoni")
            .IsAvailable(false)
            .Build();

        var command = new DeletePizzaCommand(pizzaId);

        _pizzaRepositoryMock
            .Setup(x => x.GetByIdAsync(pizzaId))
            .ReturnsAsync(existingPizza);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Contain("successfully deleted");
        existingPizza.IsAvailable.Should().BeFalse();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAdminDeletes_DoesNotHardDeletePizza()
    {
        // Arrange
        var pizzaId = "pizza-123";
        _currentUserServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var existingPizza = TestDataBuilder.Pizza()
            .WithId(pizzaId)
            .WithName("Hawaiian")
            .IsAvailable(true)
            .Build();

        var command = new DeletePizzaCommand(pizzaId);

        _pizzaRepositoryMock
            .Setup(x => x.GetByIdAsync(pizzaId))
            .ReturnsAsync(existingPizza);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _pizzaRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<Domain.Entities.Pizza>()), 
            Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
