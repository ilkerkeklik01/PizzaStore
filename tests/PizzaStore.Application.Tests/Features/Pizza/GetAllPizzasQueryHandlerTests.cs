using FluentAssertions;
using Moq;
using PizzaStore.Application.Features.Queries.Pizza;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Tests.Features.Pizza;

public class GetAllPizzasQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPizzaRepository> _pizzaRepositoryMock;
    private readonly GetAllPizzasQueryHandler _handler;

    public GetAllPizzasQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _pizzaRepositoryMock = new Mock<IPizzaRepository>();
        _unitOfWorkMock.Setup(x => x.Pizzas).Returns(_pizzaRepositoryMock.Object);
        
        _handler = new GetAllPizzasQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenPizzasExist_ReturnsListOfPizzaDtos()
    {
        // Arrange
        var pizza1 = TestDataBuilder.Pizza()
            .WithId("pizza-1")
            .WithName("Margherita")
            .WithType(PizzaType.Vegetarian)
            .WithVariants(
                TestDataBuilder.PizzaVariant()
                    .WithId("variant-1")
                    .WithSize(PizzaSize.Small)
                    .WithPrice(8.99m)
                    .Build(),
                TestDataBuilder.PizzaVariant()
                    .WithId("variant-2")
                    .WithSize(PizzaSize.Large)
                    .WithPrice(14.99m)
                    .Build()
            )
            .Build();

        var pizza2 = TestDataBuilder.Pizza()
            .WithId("pizza-2")
            .WithName("Pepperoni")
            .WithType(PizzaType.MeatLovers)
            .Build();

        var pizzas = new List<Domain.Entities.Pizza> { pizza1, pizza2 };

        _pizzaRepositoryMock
            .Setup(x => x.GetAvailablePizzasWithVariantsAsync())
            .ReturnsAsync(pizzas);

        var query = new GetAllPizzasQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        result[0].Id.Should().Be("pizza-1");
        result[0].Name.Should().Be("Margherita");
        result[0].Type.Should().Be(PizzaType.Vegetarian);
        result[0].Variants.Should().HaveCount(2);
        result[0].Variants[0].Size.Should().Be(PizzaSize.Small);
        result[0].Variants[0].Price.Should().Be(8.99m);
        
        result[1].Id.Should().Be("pizza-2");
        result[1].Name.Should().Be("Pepperoni");
        
        _pizzaRepositoryMock.Verify(x => x.GetAvailablePizzasWithVariantsAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoPizzasExist_ReturnsEmptyList()
    {
        // Arrange
        _pizzaRepositoryMock
            .Setup(x => x.GetAvailablePizzasWithVariantsAsync())
            .ReturnsAsync(new List<Domain.Entities.Pizza>());

        var query = new GetAllPizzasQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        
        _pizzaRepositoryMock.Verify(x => x.GetAvailablePizzasWithVariantsAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPizzaHasNoVariants_ReturnsEmptyVariantsList()
    {
        // Arrange
        var pizza = TestDataBuilder.Pizza()
            .WithId("pizza-1")
            .WithVariants()
            .Build();

        _pizzaRepositoryMock
            .Setup(x => x.GetAvailablePizzasWithVariantsAsync())
            .ReturnsAsync(new List<Domain.Entities.Pizza> { pizza });

        var query = new GetAllPizzasQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Variants.Should().NotBeNull();
        result[0].Variants.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenPizzaHasNullVariants_ReturnsEmptyVariantsList()
    {
        // Arrange
        var pizza = TestDataBuilder.Pizza()
            .WithId("pizza-1")
            .Build();
        pizza.Variants = null!;

        _pizzaRepositoryMock
            .Setup(x => x.GetAvailablePizzasWithVariantsAsync())
            .ReturnsAsync(new List<Domain.Entities.Pizza> { pizza });

        var query = new GetAllPizzasQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Variants.Should().NotBeNull();
        result[0].Variants.Should().BeEmpty();
    }
}
