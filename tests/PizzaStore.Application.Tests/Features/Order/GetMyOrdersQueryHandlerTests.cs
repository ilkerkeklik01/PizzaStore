using FluentAssertions;
using Moq;
using PizzaStore.Application.Features.Queries.Order.GetMyOrders;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Tests.Features.Order;

public class GetMyOrdersQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetMyOrdersQueryHandler _handler;

    public GetMyOrdersQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _unitOfWorkMock.Setup(x => x.Orders).Returns(_orderRepositoryMock.Object);
        
        _handler = new GetMyOrdersQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticated_ReturnsUserOrders()
    {
        // Arrange
        var userId = "user-123";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);

        var order1 = TestDataBuilder.Order()
            .WithId("order-1")
            .WithUserId(userId)
            .WithTotalPrice(25.99m)
            .WithStatus(OrderStatus.Pending)
            .Build();

        var order2 = TestDataBuilder.Order()
            .WithId("order-2")
            .WithUserId(userId)
            .WithTotalPrice(45.50m)
            .WithStatus(OrderStatus.Confirmed)
            .Build();

        var orders = new List<Domain.Entities.Order> { order1, order2 };

        _orderRepositoryMock
            .Setup(x => x.GetOrdersByUserIdAsync(userId))
            .ReturnsAsync(orders);

        var query = new GetMyOrdersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Id.Should().Be("order-1");
        result[0].TotalPrice.Should().Be(25.99m);
        result[1].Id.Should().Be("order-2");
        result[1].TotalPrice.Should().Be(45.50m);
        
        _orderRepositoryMock.Verify(x => x.GetOrdersByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoOrders_ReturnsEmptyList()
    {
        // Arrange
        var userId = "user-456";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);

        _orderRepositoryMock
            .Setup(x => x.GetOrdersByUserIdAsync(userId))
            .ReturnsAsync(new List<Domain.Entities.Order>());

        var query = new GetMyOrdersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns((string?)null);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(false);

        var query = new GetMyOrdersQuery();

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
