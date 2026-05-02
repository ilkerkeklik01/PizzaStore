using FluentAssertions;
using Moq;
using PizzaStore.Application.Features.Order.Queries.GetMyOrders;
using PizzaStore.Application.Services;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Tests.Features.Order.Queries;

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
    public async Task Handle_WhenUserIsAuthenticated_ReturnsPagedUserOrders()
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
            .Setup(x => x.GetFilteredOrdersByUserIdAsync(
                userId,
                It.IsAny<OrderStatus?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync((orders, 2));

        var query = new GetMyOrdersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items[0].Id.Should().Be("order-1");
        result.Items[0].TotalPrice.Should().Be(25.99m);
        result.Items[1].Id.Should().Be("order-2");
        result.Items[1].TotalPrice.Should().Be(45.50m);

        _orderRepositoryMock.Verify(
            x => x.GetFilteredOrdersByUserIdAsync(userId, null, null, null, 1, 10),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoOrders_ReturnsEmptyPagedResult()
    {
        // Arrange
        var userId = "user-456";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);

        _orderRepositoryMock
            .Setup(x => x.GetFilteredOrdersByUserIdAsync(
                userId,
                It.IsAny<OrderStatus?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync((new List<Domain.Entities.Order>(), 0));

        var query = new GetMyOrdersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
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

    [Fact]
    public async Task Handle_WithStatusFilter_PassesFilterToRepository()
    {
        // Arrange
        var userId = "user-789";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);

        _orderRepositoryMock
            .Setup(x => x.GetFilteredOrdersByUserIdAsync(
                userId,
                OrderStatus.Delivered,
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync((new List<Domain.Entities.Order>(), 0));

        var query = new GetMyOrdersQuery(Status: OrderStatus.Delivered);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _orderRepositoryMock.Verify(
            x => x.GetFilteredOrdersByUserIdAsync(userId, OrderStatus.Delivered, null, null, 1, 10),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithPagination_CalculatesTotalPagesCorrectly()
    {
        // Arrange
        var userId = "user-999";
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);

        _orderRepositoryMock
            .Setup(x => x.GetFilteredOrdersByUserIdAsync(
                userId,
                It.IsAny<OrderStatus?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync((new List<Domain.Entities.Order>(), 25));

        var query = new GetMyOrdersQuery(Page: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }
}
