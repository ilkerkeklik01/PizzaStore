using FluentAssertions;
using Moq;
using PizzaStore.Application.Features.Queries.Order.GetOrderById;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Tests.Features.Order;

public class GetOrderByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _unitOfWorkMock.Setup(x => x.Orders).Returns(_orderRepositoryMock.Object);
        
        _handler = new GetOrderByIdQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenOrderExistsAndUserIsOwner_ReturnsOrder()
    {
        // Arrange
        var userId = "user-123";
        var orderId = "order-456";
        
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);

        var order = TestDataBuilder.Order()
            .WithId(orderId)
            .WithUserId(userId)
            .WithTotalPrice(35.99m)
            .WithStatus(OrderStatus.Confirmed)
            .Build();

        _orderRepositoryMock
            .Setup(x => x.GetOrderByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);

        var query = new GetOrderByIdQuery(orderId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(orderId);
        result.TotalPrice.Should().Be(35.99m);
        result.Status.Should().Be(OrderStatus.Confirmed);
        
        _orderRepositoryMock.Verify(x => x.GetOrderByIdWithDetailsAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "user-123";
        var orderId = "non-existent-order";
        
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);

        _orderRepositoryMock
            .Setup(x => x.GetOrderByIdWithDetailsAsync(orderId))
            .ReturnsAsync((Domain.Entities.Order?)null);

        var query = new GetOrderByIdQuery(orderId);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Order with ID '{orderId}' not found");
    }

    [Fact]
    public async Task Handle_WhenUserIsNotOwner_ThrowsUnauthorizedException()
    {
        // Arrange
        var userId = "user-123";
        var differentUserId = "user-999";
        var orderId = "order-456";
        
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);

        var order = TestDataBuilder.Order()
            .WithId(orderId)
            .WithUserId(differentUserId) // Different user owns this order
            .Build();

        _orderRepositoryMock
            .Setup(x => x.GetOrderByIdWithDetailsAsync(orderId))
            .ReturnsAsync(order);

        var query = new GetOrderByIdQuery(orderId);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("You do not have permission to view this order");
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns((string?)null);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(false);

        var query = new GetOrderByIdQuery("order-123");

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
