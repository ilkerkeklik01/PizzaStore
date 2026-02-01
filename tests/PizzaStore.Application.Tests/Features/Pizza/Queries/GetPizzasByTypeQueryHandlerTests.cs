using FluentAssertions;
using Moq;
using PizzaStore.Application.Features.Pizza.Queries;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Tests.Features.Pizza.Queries;

public class GetPizzasByTypeQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPizzaRepository> _pizzaRepositoryMock;
    private readonly GetPizzasByTypeQueryHandler _handler;

    public GetPizzasByTypeQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _pizzaRepositoryMock = new Mock<IPizzaRepository>();
        _unitOfWorkMock.Setup(x => x.Pizzas).Returns(_pizzaRepositoryMock.Object);
        
        _handler = new GetPizzasByTypeQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenPizzasOfTypeExist_ReturnsFilteredListOfPizzaDtos()
    {
        // Arrange
        var vegetarianPizza1 = TestDataBuilder.Pizza()
            .WithId("pizza-1")
            .WithName("Margherita")
            .WithType(PizzaType.Vegetarian)
            .WithVariants(
                TestDataBuilder.PizzaVariant()
                    .WithId("variant-1")
                    .WithSize(PizzaSize.Small)
                    .WithPrice(8.99m)
                    .Build()
            )
            .Build();

        var vegetarianPizza2 = TestDataBuilder.Pizza()
            .WithId("pizza-2")
            .WithName("Veggie Supreme")
            .WithType(PizzaType.Vegetarian)
            .Build();

        var meatPizza = TestDataBuilder.Pizza()
            .WithId("pizza-3")
            .WithName("Pepperoni")
            .WithType(PizzaType.MeatLovers)
            .Build();

        var allPizzas = new List<Domain.Entities.Pizza> { vegetarianPizza1, vegetarianPizza2, meatPizza };

        _pizzaRepositoryMock
            .Setup(x => x.GetAvailablePizzasWithVariantsAsync())
            .ReturnsAsync(allPizzas);

        var query = new GetPizzasByTypeQuery { Type = PizzaType.Vegetarian };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.Type.Should().Be(PizzaType.Vegetarian));
        
        result[0].Id.Should().Be("pizza-1");
        result[0].Name.Should().Be("Margherita");
        result[0].Variants.Should().HaveCount(1);
        
        result[1].Id.Should().Be("pizza-2");
        result[1].Name.Should().Be("Veggie Supreme");
        
        _pizzaRepositoryMock.Verify(x => x.GetAvailablePizzasWithVariantsAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoPizzasOfTypeExist_ReturnsEmptyList()
    {
        // Arrange
        var meatPizza = TestDataBuilder.Pizza()
            .WithId("pizza-1")
            .WithName("Pepperoni")
            .WithType(PizzaType.MeatLovers)
            .Build();

        var allPizzas = new List<Domain.Entities.Pizza> { meatPizza };

        _pizzaRepositoryMock
            .Setup(x => x.GetAvailablePizzasWithVariantsAsync())
            .ReturnsAsync(allPizzas);

        var query = new GetPizzasByTypeQuery { Type = PizzaType.Vegetarian };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        
        _pizzaRepositoryMock.Verify(x => x.GetAvailablePizzasWithVariantsAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoPizzasExist_ReturnsEmptyList()
    {
        // Arrange
        _pizzaRepositoryMock
            .Setup(x => x.GetAvailablePizzasWithVariantsAsync())
            .ReturnsAsync(new List<Domain.Entities.Pizza>());

        var query = new GetPizzasByTypeQuery { Type = PizzaType.MeatLovers };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        
        _pizzaRepositoryMock.Verify(x => x.GetAvailablePizzasWithVariantsAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFilteringByMeatLovers_ReturnsOnlyMeatLoverPizzas()
    {
        // Arrange
        var meatPizza1 = TestDataBuilder.Pizza()
            .WithId("pizza-1")
            .WithName("Pepperoni")
            .WithType(PizzaType.MeatLovers)
            .Build();

        var meatPizza2 = TestDataBuilder.Pizza()
            .WithId("pizza-2")
            .WithName("BBQ Chicken")
            .WithType(PizzaType.MeatLovers)
            .Build();

        var vegetarianPizza = TestDataBuilder.Pizza()
            .WithId("pizza-3")
            .WithName("Margherita")
            .WithType(PizzaType.Vegetarian)
            .Build();

        var allPizzas = new List<Domain.Entities.Pizza> { meatPizza1, meatPizza2, vegetarianPizza };

        _pizzaRepositoryMock
            .Setup(x => x.GetAvailablePizzasWithVariantsAsync())
            .ReturnsAsync(allPizzas);

        var query = new GetPizzasByTypeQuery { Type = PizzaType.MeatLovers };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.Type.Should().Be(PizzaType.MeatLovers));
        
        result[0].Id.Should().Be("pizza-1");
        result[1].Id.Should().Be("pizza-2");
        
        _pizzaRepositoryMock.Verify(x => x.GetAvailablePizzasWithVariantsAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPizzaHasNullVariants_ReturnsEmptyVariantsList()
    {
        // Arrange
        var pizza = TestDataBuilder.Pizza()
            .WithId("pizza-1")
            .WithType(PizzaType.Vegetarian)
            .Build();
        pizza.Variants = null!;

        _pizzaRepositoryMock
            .Setup(x => x.GetAvailablePizzasWithVariantsAsync())
            .ReturnsAsync(new List<Domain.Entities.Pizza> { pizza });

        var query = new GetPizzasByTypeQuery { Type = PizzaType.Vegetarian };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Variants.Should().NotBeNull();
        result[0].Variants.Should().BeEmpty();
    }
}
