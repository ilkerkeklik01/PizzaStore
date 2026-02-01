using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PizzaStore.Application.Features.Commands.Admin.UpdateOrderStatus;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Tests.Features.Admin;

public class UpdateOrderStatusCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<UpdateOrderStatusCommandHandler>> _loggerMock;
    private readonly UpdateOrderStatusCommandHandler _handler;

    public UpdateOrderStatusCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _currentUserServiceMock = MockCurrentUserServiceHelper.CreateAdminUser();
        _loggerMock = new Mock<ILogger<UpdateOrderStatusCommandHandler>>();

        _unitOfWorkMock.Setup(x => x.Orders).Returns(_orderRepositoryMock.Object);

        _handler = new UpdateOrderStatusCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenPendingToConfirmed_SetsConfirmedAtTimestamp()
    {
        // Arrange
        var orderId = "order-123";
        var order = TestDataBuilder.Order()
            .WithId(orderId)
            .WithStatus(OrderStatus.Pending)
            .WithCreatedAt(DateTime.UtcNow.AddMinutes(-10))
            .Build();

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Confirmed);

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
        result.Status.Should().Be(OrderStatus.Confirmed);
        
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.ConfirmedAt.Should().NotBeNull();
        order.ConfirmedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        order.CompletedAt.Should().BeNull();
        order.CancelledAt.Should().BeNull();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenConfirmedToDelivered_SetsCompletedAtTimestamp()
    {
        // Arrange
        var orderId = "order-123";
        var confirmedAt = DateTime.UtcNow.AddMinutes(-20);
        var order = TestDataBuilder.Order()
            .WithId(orderId)
            .WithStatus(OrderStatus.Confirmed)
            .WithCreatedAt(DateTime.UtcNow.AddMinutes(-30))
            .WithConfirmedAt(confirmedAt)
            .Build();

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Delivered);

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
        result.Status.Should().Be(OrderStatus.Delivered);
        
        order.Status.Should().Be(OrderStatus.Delivered);
        order.ConfirmedAt.Should().Be(confirmedAt);
        order.CompletedAt.Should().NotBeNull();
        order.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        order.CancelledAt.Should().BeNull();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPendingToCancelled_SetsCancelledAtTimestamp()
    {
        // Arrange
        var orderId = "order-123";
        var order = TestDataBuilder.Order()
            .WithId(orderId)
            .WithStatus(OrderStatus.Pending)
            .WithCreatedAt(DateTime.UtcNow.AddMinutes(-5))
            .Build();

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Cancelled);

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
        result.Status.Should().Be(OrderStatus.Cancelled);
        
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelledAt.Should().NotBeNull();
        order.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        order.ConfirmedAt.Should().BeNull();
        order.CompletedAt.Should().BeNull();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNonAdminUser_ThrowsUnauthorizedException()
    {
        // Arrange
        var nonAdminUserService = MockCurrentUserServiceHelper.CreateAuthenticatedUser(isAdmin: false);
        var handler = new UpdateOrderStatusCommandHandler(
            _unitOfWorkMock.Object,
            nonAdminUserService.Object,
            _loggerMock.Object);

        var orderId = "order-123";
        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Confirmed);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Only administrators can update order status");
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var orderId = "non-existent-order";
        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Confirmed);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((Domain.Entities.Order?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Order with ID 'non-existent-order' not found");
    }

    [Fact]
    public async Task Handle_WhenDeliveredOrderTransition_ThrowsValidationException()
    {
        // Arrange
        var orderId = "order-123";
        var order = TestDataBuilder.Order()
            .WithId(orderId)
            .WithStatus(OrderStatus.Delivered)
            .WithCompletedAt(DateTime.UtcNow.AddHours(-1))
            .Build();

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Pending);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Cannot transition from Delivered to Pending. Delivered orders cannot be changed");
    }

    [Fact]
    public async Task Handle_WhenCancelledOrderTransition_ThrowsValidationException()
    {
        // Arrange
        var orderId = "order-123";
        var order = TestDataBuilder.Order()
            .WithId(orderId)
            .WithStatus(OrderStatus.Cancelled)
            .WithCancelledAt(DateTime.UtcNow.AddHours(-1))
            .Build();

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Confirmed);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Cannot transition from Cancelled to Confirmed. Cancelled orders cannot be changed");
    }

    [Fact]
    public async Task Handle_WhenSameStatusTransition_DoesNotThrowAndNoOpAllowed()
    {
        // Arrange
        var orderId = "order-123";
        var confirmedAt = DateTime.UtcNow.AddMinutes(-10);
        var order = TestDataBuilder.Order()
            .WithId(orderId)
            .WithStatus(OrderStatus.Confirmed)
            .WithConfirmedAt(confirmedAt)
            .Build();

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Confirmed);

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
        result.Status.Should().Be(OrderStatus.Confirmed);
        
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.ConfirmedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDeliveredToDelivered_AllowsTransition()
    {
        // Arrange
        var orderId = "order-123";
        var completedAt = DateTime.UtcNow.AddHours(-1);
        var order = TestDataBuilder.Order()
            .WithId(orderId)
            .WithStatus(OrderStatus.Delivered)
            .WithCompletedAt(completedAt)
            .Build();

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Delivered);

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
        result.Status.Should().Be(OrderStatus.Delivered);
        
        order.Status.Should().Be(OrderStatus.Delivered);
        order.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCancelledToCancelled_AllowsTransition()
    {
        // Arrange
        var orderId = "order-123";
        var cancelledAt = DateTime.UtcNow.AddHours(-1);
        var order = TestDataBuilder.Order()
            .WithId(orderId)
            .WithStatus(OrderStatus.Cancelled)
            .WithCancelledAt(cancelledAt)
            .Build();

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Cancelled);

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
        result.Status.Should().Be(OrderStatus.Cancelled);
        
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
