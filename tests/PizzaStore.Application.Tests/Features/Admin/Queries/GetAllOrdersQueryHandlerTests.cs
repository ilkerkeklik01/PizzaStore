using FluentAssertions;
using Moq;
using PizzaStore.Application.Features.Admin.Queries.GetAllOrders;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Domain.Interfaces;
using DomainOrder = PizzaStore.Domain.Entities.Order;
using OrderStatus = PizzaStore.Domain.Entities.OrderStatus;

namespace PizzaStore.Application.Tests.Features.Admin.Queries;

public class GetAllOrdersQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetAllOrdersQueryHandler _handler;

    public GetAllOrdersQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        
        _unitOfWorkMock.Setup(x => x.Orders).Returns(_orderRepositoryMock.Object);
        
        _handler = new GetAllOrdersQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenNoFiltersProvided_ReturnsAllOrdersOrderedByCreatedAtDescending()
    {
        // Arrange
        var order1 = TestDataBuilder.Order()
            .WithId("order-1")
            .WithUserId("user-1")
            .WithTotalPrice(25.99m)
            .WithStatus(OrderStatus.Pending)
            .WithCreatedAt(DateTime.UtcNow.AddDays(-2))
            .Build();

        var order2 = TestDataBuilder.Order()
            .WithId("order-2")
            .WithUserId("user-2")
            .WithTotalPrice(45.50m)
            .WithStatus(OrderStatus.Confirmed)
            .WithCreatedAt(DateTime.UtcNow.AddDays(-1))
            .Build();

        var order3 = TestDataBuilder.Order()
            .WithId("order-3")
            .WithUserId("user-3")
            .WithTotalPrice(35.75m)
            .WithStatus(OrderStatus.Delivered)
            .WithCreatedAt(DateTime.UtcNow)
            .Build();

        var orders = new List<DomainOrder> { order1, order2, order3 };

        _orderRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(orders);

        var query = new GetAllOrdersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].Id.Should().Be("order-3"); // Most recent first
        result[1].Id.Should().Be("order-2");
        result[2].Id.Should().Be("order-1");
        
        _orderRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFilteredByStatus_ReturnsOnlyOrdersWithMatchingStatus()
    {
        // Arrange
        var order1 = TestDataBuilder.Order()
            .WithId("order-1")
            .WithStatus(OrderStatus.Pending)
            .Build();

        var order2 = TestDataBuilder.Order()
            .WithId("order-2")
            .WithStatus(OrderStatus.Confirmed)
            .Build();

        var order3 = TestDataBuilder.Order()
            .WithId("order-3")
            .WithStatus(OrderStatus.Pending)
            .Build();

        var orders = new List<DomainOrder> { order1, order2, order3 };

        _orderRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(orders);

        var query = new GetAllOrdersQuery(Status: OrderStatus.Pending);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(o => o.Status == OrderStatus.Pending);
    }

    [Fact]
    public async Task Handle_WhenFilteredByUserId_ReturnsOnlyOrdersForSpecificUser()
    {
        // Arrange
        var userId = "user-123";
        var order1 = TestDataBuilder.Order()
            .WithId("order-1")
            .WithUserId(userId)
            .Build();

        var order2 = TestDataBuilder.Order()
            .WithId("order-2")
            .WithUserId("user-456")
            .Build();

        var order3 = TestDataBuilder.Order()
            .WithId("order-3")
            .WithUserId(userId)
            .Build();

        var orders = new List<DomainOrder> { order1, order2, order3 };

        _orderRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(orders);

        var query = new GetAllOrdersQuery(UserId: userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(o => o.UserId == userId);
    }

    [Fact]
    public async Task Handle_WhenFilteredByDateRange_ReturnsOnlyOrdersWithinRange()
    {
        // Arrange
        var fromDate = new DateTime(2024, 1, 10);
        var toDate = new DateTime(2024, 1, 20);

        var order1 = TestDataBuilder.Order()
            .WithId("order-1")
            .WithCreatedAt(new DateTime(2024, 1, 5)) // Before range
            .Build();

        var order2 = TestDataBuilder.Order()
            .WithId("order-2")
            .WithCreatedAt(new DateTime(2024, 1, 15)) // Within range
            .Build();

        var order3 = TestDataBuilder.Order()
            .WithId("order-3")
            .WithCreatedAt(new DateTime(2024, 1, 20, 12, 0, 0)) // On toDate
            .Build();

        var order4 = TestDataBuilder.Order()
            .WithId("order-4")
            .WithCreatedAt(new DateTime(2024, 1, 25)) // After range
            .Build();

        var orders = new List<DomainOrder> { order1, order2, order3, order4 };

        _orderRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(orders);

        var query = new GetAllOrdersQuery(FromDate: fromDate, ToDate: toDate);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(o => o.Id == "order-2");
        result.Should().Contain(o => o.Id == "order-3");
    }

    [Fact]
    public async Task Handle_WhenMultipleFiltersApplied_ReturnsOrdersMatchingAllFilters()
    {
        // Arrange
        var userId = "user-123";
        var status = OrderStatus.Confirmed;
        var fromDate = new DateTime(2024, 1, 10);

        var order1 = TestDataBuilder.Order()
            .WithId("order-1")
            .WithUserId(userId)
            .WithStatus(status)
            .WithCreatedAt(new DateTime(2024, 1, 15))
            .Build();

        var order2 = TestDataBuilder.Order()
            .WithId("order-2")
            .WithUserId(userId)
            .WithStatus(OrderStatus.Pending) // Different status
            .WithCreatedAt(new DateTime(2024, 1, 15))
            .Build();

        var order3 = TestDataBuilder.Order()
            .WithId("order-3")
            .WithUserId("user-456") // Different user
            .WithStatus(status)
            .WithCreatedAt(new DateTime(2024, 1, 15))
            .Build();

        var orders = new List<DomainOrder> { order1, order2, order3 };

        _orderRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(orders);

        var query = new GetAllOrdersQuery(Status: status, UserId: userId, FromDate: fromDate);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("order-1");
    }

    [Fact]
    public async Task Handle_WhenNoOrdersExist_ReturnsEmptyList()
    {
        // Arrange
        _orderRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<DomainOrder>());

        var query = new GetAllOrdersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
