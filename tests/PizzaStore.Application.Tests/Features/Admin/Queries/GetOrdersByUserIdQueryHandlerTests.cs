using FluentAssertions;
using Moq;
using PizzaStore.Application.Features.Admin.Queries.GetOrdersByUserId;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;
using DomainOrder = PizzaStore.Domain.Entities.Order;
using OrderStatus = PizzaStore.Domain.Entities.OrderStatus;

namespace PizzaStore.Application.Tests.Features.Admin.Queries;

public class GetOrdersByUserIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetOrdersByUserIdQueryHandler _handler;

    public GetOrdersByUserIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        
        _unitOfWorkMock.Setup(x => x.Orders).Returns(_orderRepositoryMock.Object);
        
        _handler = new GetOrdersByUserIdQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserHasOrders_ReturnsOrdersOrderedByCreatedAtDescending()
    {
        // Arrange
        var userId = "user-123";
        var order1 = TestDataBuilder.Order()
            .WithId("order-1")
            .WithUserId(userId)
            .WithTotalPrice(25.99m)
            .WithStatus(OrderStatus.Pending)
            .WithCreatedAt(DateTime.UtcNow.AddDays(-2))
            .Build();

        var order2 = TestDataBuilder.Order()
            .WithId("order-2")
            .WithUserId(userId)
            .WithTotalPrice(45.50m)
            .WithStatus(OrderStatus.Confirmed)
            .WithCreatedAt(DateTime.UtcNow.AddDays(-1))
            .Build();

        var order3 = TestDataBuilder.Order()
            .WithId("order-3")
            .WithUserId(userId)
            .WithTotalPrice(35.75m)
            .WithStatus(OrderStatus.Delivered)
            .WithCreatedAt(DateTime.UtcNow)
            .Build();

        var orders = new List<DomainOrder> { order1, order2, order3 };

        _orderRepositoryMock
            .Setup(x => x.GetOrdersByUserIdAsync(userId))
            .ReturnsAsync(orders);

        var query = new GetOrdersByUserIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].Id.Should().Be("order-3"); // Most recent first
        result[1].Id.Should().Be("order-2");
        result[2].Id.Should().Be("order-1");
        result.Should().OnlyContain(o => o.UserId == userId);
        
        _orderRepositoryMock.Verify(x => x.GetOrdersByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasSingleOrder_ReturnsSingleOrder()
    {
        // Arrange
        var userId = "user-456";
        var order = TestDataBuilder.Order()
            .WithId("order-1")
            .WithUserId(userId)
            .WithTotalPrice(19.99m)
            .WithStatus(OrderStatus.Pending)
            .Build();

        _orderRepositoryMock
            .Setup(x => x.GetOrdersByUserIdAsync(userId))
            .ReturnsAsync(new List<DomainOrder> { order });

        var query = new GetOrdersByUserIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("order-1");
        result[0].UserId.Should().Be(userId);
        result[0].TotalPrice.Should().Be(19.99m);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoOrders_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "user-without-orders";

        _orderRepositoryMock
            .Setup(x => x.GetOrdersByUserIdAsync(userId))
            .ReturnsAsync(new List<DomainOrder>());

        var query = new GetOrdersByUserIdQuery(userId);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"No orders found for user with ID '{userId}'");
        
        _orderRepositoryMock.Verify(x => x.GetOrdersByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsNull_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "user-789";

        _orderRepositoryMock
            .Setup(x => x.GetOrdersByUserIdAsync(userId))
            .ReturnsAsync((List<DomainOrder>?)null!);

        var query = new GetOrdersByUserIdQuery(userId);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"No orders found for user with ID '{userId}'");
    }

    [Fact]
    public async Task Handle_WhenUserHasOrdersWithDifferentStatuses_ReturnsAllOrders()
    {
        // Arrange
        var userId = "user-999";
        var order1 = TestDataBuilder.Order()
            .WithId("order-1")
            .WithUserId(userId)
            .WithStatus(OrderStatus.Pending)
            .Build();

        var order2 = TestDataBuilder.Order()
            .WithId("order-2")
            .WithUserId(userId)
            .WithStatus(OrderStatus.Confirmed)
            .Build();

        var order3 = TestDataBuilder.Order()
            .WithId("order-3")
            .WithUserId(userId)
            .WithStatus(OrderStatus.Delivered)
            .Build();

        var order4 = TestDataBuilder.Order()
            .WithId("order-4")
            .WithUserId(userId)
            .WithStatus(OrderStatus.Cancelled)
            .Build();

        var orders = new List<DomainOrder> { order1, order2, order3, order4 };

        _orderRepositoryMock
            .Setup(x => x.GetOrdersByUserIdAsync(userId))
            .ReturnsAsync(orders);

        var query = new GetOrdersByUserIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(4);
        result.Should().Contain(o => o.Status == OrderStatus.Pending);
        result.Should().Contain(o => o.Status == OrderStatus.Confirmed);
        result.Should().Contain(o => o.Status == OrderStatus.Delivered);
        result.Should().Contain(o => o.Status == OrderStatus.Cancelled);
    }
}
