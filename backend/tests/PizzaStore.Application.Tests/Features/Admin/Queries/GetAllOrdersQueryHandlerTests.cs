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

    private void SetupPagedRepo(IEnumerable<DomainOrder> orders, int totalCount,
        OrderStatus? status = null, string? userId = null,
        DateTime? fromDate = null, DateTime? toDate = null,
        int page = 1, int pageSize = 10)
    {
        _orderRepositoryMock
            .Setup(x => x.GetAllOrdersPagedAsync(status, userId, fromDate, toDate, page, pageSize))
            .ReturnsAsync((orders, totalCount));
    }

    [Fact]
    public async Task Handle_WhenNoFiltersProvided_ReturnsPagedResultWithAllOrders()
    {
        // Arrange
        var order1 = TestDataBuilder.Order().WithId("order-1").WithUserId("user-1")
            .WithTotalPrice(25.99m).WithStatus(OrderStatus.Pending)
            .WithCreatedAt(DateTime.UtcNow.AddDays(-2)).Build();

        var order2 = TestDataBuilder.Order().WithId("order-2").WithUserId("user-2")
            .WithTotalPrice(45.50m).WithStatus(OrderStatus.Confirmed)
            .WithCreatedAt(DateTime.UtcNow.AddDays(-1)).Build();

        var order3 = TestDataBuilder.Order().WithId("order-3").WithUserId("user-3")
            .WithTotalPrice(35.75m).WithStatus(OrderStatus.Delivered)
            .WithCreatedAt(DateTime.UtcNow).Build();

        var orders = new List<DomainOrder> { order3, order2, order1 }; // repo returns desc order
        SetupPagedRepo(orders, 3);

        var query = new GetAllOrdersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);

        _orderRepositoryMock.Verify(x => x.GetAllOrdersPagedAsync(null, null, null, null, 1, 10), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFilteredByStatus_ReturnsOnlyOrdersWithMatchingStatus()
    {
        // Arrange
        var order1 = TestDataBuilder.Order().WithId("order-1").WithStatus(OrderStatus.Pending).Build();
        var order3 = TestDataBuilder.Order().WithId("order-3").WithStatus(OrderStatus.Pending).Build();

        SetupPagedRepo(new List<DomainOrder> { order3, order1 }, 2, status: OrderStatus.Pending);

        var query = new GetAllOrdersQuery(Status: OrderStatus.Pending);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(o => o.Status == OrderStatus.Pending);
    }

    [Fact]
    public async Task Handle_WhenFilteredByUserId_ReturnsOnlyOrdersForSpecificUser()
    {
        // Arrange
        var userId = "user-123";
        var order1 = TestDataBuilder.Order().WithId("order-1").WithUserId(userId).Build();
        var order3 = TestDataBuilder.Order().WithId("order-3").WithUserId(userId).Build();

        SetupPagedRepo(new List<DomainOrder> { order3, order1 }, 2, userId: userId);

        var query = new GetAllOrdersQuery(UserId: userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(o => o.UserId == userId);
    }

    [Fact]
    public async Task Handle_WhenFilteredByDateRange_ForwardsDateParamsToRepository()
    {
        // Arrange
        var fromDate = new DateTime(2024, 1, 10);
        var toDate = new DateTime(2024, 1, 20);

        var order = TestDataBuilder.Order().WithId("order-2")
            .WithCreatedAt(new DateTime(2024, 1, 15)).Build();

        SetupPagedRepo(new List<DomainOrder> { order }, 1, fromDate: fromDate, toDate: toDate);

        var query = new GetAllOrdersQuery(FromDate: fromDate, ToDate: toDate);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].Id.Should().Be("order-2");

        _orderRepositoryMock.Verify(x => x.GetAllOrdersPagedAsync(null, null, fromDate, toDate, 1, 10), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMultipleFiltersApplied_ForwardsAllParamsToRepository()
    {
        // Arrange
        var userId = "user-123";
        var status = OrderStatus.Confirmed;
        var fromDate = new DateTime(2024, 1, 10);

        var order = TestDataBuilder.Order().WithId("order-1")
            .WithUserId(userId).WithStatus(status)
            .WithCreatedAt(new DateTime(2024, 1, 15)).Build();

        SetupPagedRepo(new List<DomainOrder> { order }, 1, status: status, userId: userId, fromDate: fromDate);

        var query = new GetAllOrdersQuery(Status: status, UserId: userId, FromDate: fromDate);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].Id.Should().Be("order-1");
    }

    [Fact]
    public async Task Handle_WhenNoOrdersExist_ReturnsEmptyPagedResult()
    {
        // Arrange
        SetupPagedRepo(new List<DomainOrder>(), 0);

        var query = new GetAllOrdersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithPaginationParams_ForwardsPageAndPageSizeToRepository()
    {
        // Arrange
        var orders = Enumerable.Range(1, 5)
            .Select(i => TestDataBuilder.Order().WithId($"order-{i}").Build())
            .ToList();

        SetupPagedRepo(orders, 25, page: 2, pageSize: 5);

        var query = new GetAllOrdersQuery(Page: 2, PageSize: 5);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(5);
        result.Items.Should().HaveCount(5);

        _orderRepositoryMock.Verify(x => x.GetAllOrdersPagedAsync(null, null, null, null, 2, 5), Times.Once);
    }

    [Fact]
    public async Task Handle_TotalPagesCalculation_RoundsUpForPartialPage()
    {
        // Arrange
        var orders = Enumerable.Range(1, 3)
            .Select(i => TestDataBuilder.Order().WithId($"order-{i}").Build())
            .ToList();

        SetupPagedRepo(orders, 23, page: 3, pageSize: 10);

        var query = new GetAllOrdersQuery(Page: 3, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalPages.Should().Be(3); // ceil(23/10) = 3
    }
}
