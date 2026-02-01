using FluentAssertions;
using Moq;
using PizzaStore.Application.Features.Queries.Topping;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Tests.Features.Topping;

public class GetAllToppingsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IToppingRepository> _toppingRepositoryMock;
    private readonly GetAllToppingsQueryHandler _handler;

    public GetAllToppingsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _toppingRepositoryMock = new Mock<IToppingRepository>();
        _unitOfWorkMock.Setup(x => x.Toppings).Returns(_toppingRepositoryMock.Object);
        
        _handler = new GetAllToppingsQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenToppingsExist_ReturnsListOfToppingDtos()
    {
        // Arrange
        var topping1 = TestDataBuilder.Topping()
            .WithId("topping-1")
            .WithName("Mushrooms")
            .WithPrice(1.50m)
            .IsAvailable(true)
            .Build();

        var topping2 = TestDataBuilder.Topping()
            .WithId("topping-2")
            .WithName("Extra Cheese")
            .WithPrice(2.00m)
            .IsAvailable(true)
            .Build();

        var topping3 = TestDataBuilder.Topping()
            .WithId("topping-3")
            .WithName("Pepperoni")
            .WithPrice(2.50m)
            .IsAvailable(false)
            .Build();

        var toppings = new List<Domain.Entities.Topping> { topping1, topping2, topping3 };

        _toppingRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(toppings);

        var query = new GetAllToppingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        
        result[0].Id.Should().Be("topping-1");
        result[0].Name.Should().Be("Mushrooms");
        result[0].Price.Should().Be(1.50m);
        result[0].IsAvailable.Should().BeTrue();
        
        result[1].Id.Should().Be("topping-2");
        result[1].Name.Should().Be("Extra Cheese");
        result[1].Price.Should().Be(2.00m);
        result[1].IsAvailable.Should().BeTrue();
        
        result[2].Id.Should().Be("topping-3");
        result[2].Name.Should().Be("Pepperoni");
        result[2].Price.Should().Be(2.50m);
        result[2].IsAvailable.Should().BeFalse();
        
        _toppingRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoToppingsExist_ReturnsEmptyList()
    {
        // Arrange
        _toppingRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Domain.Entities.Topping>());

        var query = new GetAllToppingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        
        _toppingRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenToppingsHaveVariousPrices_ReturnsAllToppingsWithCorrectPrices()
    {
        // Arrange
        var cheapTopping = TestDataBuilder.Topping()
            .WithId("topping-1")
            .WithName("Oregano")
            .WithPrice(0.50m)
            .Build();

        var expensiveTopping = TestDataBuilder.Topping()
            .WithId("topping-2")
            .WithName("Truffle Oil")
            .WithPrice(5.00m)
            .Build();

        var toppings = new List<Domain.Entities.Topping> { cheapTopping, expensiveTopping };

        _toppingRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(toppings);

        var query = new GetAllToppingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Price.Should().Be(0.50m);
        result[1].Price.Should().Be(5.00m);
    }

    [Fact]
    public async Task Handle_WhenToppingsIncludeBothAvailableAndUnavailable_ReturnsAllToppingsWithCorrectAvailability()
    {
        // Arrange
        var availableTopping = TestDataBuilder.Topping()
            .WithId("topping-1")
            .WithName("Olives")
            .IsAvailable(true)
            .Build();

        var unavailableTopping = TestDataBuilder.Topping()
            .WithId("topping-2")
            .WithName("Anchovies")
            .IsAvailable(false)
            .Build();

        var toppings = new List<Domain.Entities.Topping> { availableTopping, unavailableTopping };

        _toppingRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(toppings);

        var query = new GetAllToppingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].IsAvailable.Should().BeTrue();
        result[1].IsAvailable.Should().BeFalse();
    }
}
