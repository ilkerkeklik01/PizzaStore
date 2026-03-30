using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PizzaStore.Application.Features.Order.Commands.CancelOrder;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;
using ValidationException = PizzaStore.Core.CrossCuttingConcerns.Exceptions.ValidationException;

namespace PizzaStore.Application.Tests.Features.Order.Commands;

public class CancelOrderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<CancelOrderCommandHandler>> _loggerMock;
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _currentUserServiceMock = MockCurrentUserServiceHelper.CreateAuthenticatedUser();
        _loggerMock = new Mock<ILogger<CancelOrderCommandHandler>>();
        
        _unitOfWorkMock.Setup(x => x.Orders).Returns(_orderRepositoryMock.Object);
        
        _handler = new CancelOrderCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAuthenticatedAndPendingOrder_CancelsOrder()
    {
        // Arrange
        var userId = "test-user-id";
        var orderId = "order-123";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var order = TestDataBuilder.Order()
            .WithId(orderId)
            .WithUserId(userId)
            .WithStatus(OrderStatus.Pending)
            .Build();

        var command = new CancelOrderCommand(orderId);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(x => x.GetOrderByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelledAt.Should().NotBeNull();
        order.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAuthenticatedAndConfirmedOrder_CancelsOrder()
    {
        // Arrange
        var userId = "test-user-id";
        var orderId = "order-123";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var order = TestDataBuilder.Order()
            .WithId(orderId)
            .WithUserId(userId)
            .WithStatus(OrderStatus.Confirmed)
            .WithConfirmedAt(DateTime.UtcNow.AddMinutes(-10))
            .Build();

        var command = new CancelOrderCommand(orderId);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(x => x.GetOrderByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelledAt.Should().NotBeNull();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        var unauthenticatedMock = MockCurrentUserServiceHelper.CreateUnauthenticatedUser();
        var handler = new CancelOrderCommandHandler(
            _unitOfWorkMock.Object,
            unauthenticatedMock.Object,
            _loggerMock.Object);

        var command = new CancelOrderCommand("order-123");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated");

        _orderRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<string>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-id";
        var orderId = "non-existent-order";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var command = new CancelOrderCommand(orderId);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((Domain.Entities.Order?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Order with ID '{orderId}' not found");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNotOwner_ThrowsUnauthorizedException()
    {
        // Arrange
        var userId = "test-user-id";
        var orderId = "order-123";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var order = TestDataBuilder.Order()
            .WithId(orderId)
            .WithUserId("another-user-id")
            .WithStatus(OrderStatus.Pending)
            .Build();

        var command = new CancelOrderCommand(orderId);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("You do not have permission to cancel this order");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenOrderIsCompleted_ThrowsValidationException()
    {
        // Arrange
        var userId = "test-user-id";
        var orderId = "order-123";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var order = TestDataBuilder.Order()
            .WithId(orderId)
            .WithUserId(userId)
            .WithStatus(OrderStatus.Delivered)
            .WithCompletedAt(DateTime.UtcNow.AddHours(-1))
            .Build();

        var command = new CancelOrderCommand(orderId);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Delivered*Only Pending or Confirmed orders can be cancelled*");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenOrderIsAlreadyCancelled_ThrowsValidationException()
    {
        // Arrange
        var userId = "test-user-id";
        var orderId = "order-123";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var order = TestDataBuilder.Order()
            .WithId(orderId)
            .WithUserId(userId)
            .WithStatus(OrderStatus.Cancelled)
            .WithCancelledAt(DateTime.UtcNow.AddMinutes(-5))
            .Build();

        var command = new CancelOrderCommand(orderId);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Cancelled*Only Pending or Confirmed orders can be cancelled*");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenOrderIsInPreparation_ThrowsValidationException()
    {
        // Arrange
        var userId = "test-user-id";
        var orderId = "order-123";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var order = TestDataBuilder.Order()
            .WithId(orderId)
            .WithUserId(userId)
            .WithStatus(OrderStatus.Preparing)
            .Build();

        var command = new CancelOrderCommand(orderId);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Preparing*Only Pending or Confirmed orders can be cancelled*");
    }

    [Fact]
    public async Task Handle_WhenOrderIsDelivered_ThrowsValidationException()
    {
        // Arrange
        var userId = "test-user-id";
        var orderId = "order-123";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var order = TestDataBuilder.Order()
            .WithId(orderId)
            .WithUserId(userId)
            .WithStatus(OrderStatus.Delivered)
            .Build();

        var command = new CancelOrderCommand(orderId);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Delivered*Only Pending or Confirmed orders can be cancelled*");
    }
}
