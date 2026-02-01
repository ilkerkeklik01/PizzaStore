using FluentAssertions;
using Moq;
using PizzaStore.Application.Features.Pizza.Queries;
using PizzaStore.Application.Tests.Helpers;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Tests.Features.Pizza.Queries;

public class GetPizzaByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPizzaRepository> _pizzaRepositoryMock;
    private readonly GetPizzaByIdQueryHandler _handler;

    public GetPizzaByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _pizzaRepositoryMock = new Mock<IPizzaRepository>();
        _unitOfWorkMock.Setup(x => x.Pizzas).Returns(_pizzaRepositoryMock.Object);
        
        _handler = new GetPizzaByIdQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenPizzaExists_ReturnsPizzaDto()
    {
        // Arrange
        var pizzaId = "pizza-123";
        var pizza = TestDataBuilder.Pizza()
            .WithId(pizzaId)
            .WithName("Margherita")
            .WithDescription("Classic Italian pizza")
            .WithType(PizzaType.Vegetarian)
            .WithImageUrl("https://example.com/margherita.jpg")
            .IsAvailable(true)
            .WithVariants(
                TestDataBuilder.PizzaVariant()
                    .WithId("variant-1")
                    .WithSize(PizzaSize.Small)
                    .WithPrice(8.99m)
                    .IsAvailable(true)
                    .Build(),
                TestDataBuilder.PizzaVariant()
                    .WithId("variant-2")
                    .WithSize(PizzaSize.Medium)
                    .WithPrice(12.99m)
                    .IsAvailable(true)
                    .Build()
            )
            .Build();

        _pizzaRepositoryMock
            .Setup(x => x.GetByIdWithVariantsAsync(pizzaId))
            .ReturnsAsync(pizza);

        var query = new GetPizzaByIdQuery { Id = pizzaId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(pizzaId);
        result.Name.Should().Be("Margherita");
        result.Description.Should().Be("Classic Italian pizza");
        result.Type.Should().Be(PizzaType.Vegetarian);
        result.ImageUrl.Should().Be("https://example.com/margherita.jpg");
        result.IsAvailable.Should().BeTrue();
        result.Variants.Should().HaveCount(2);
        result.Variants[0].Size.Should().Be(PizzaSize.Small);
        result.Variants[0].Price.Should().Be(8.99m);
        result.Variants[1].Size.Should().Be(PizzaSize.Medium);
        result.Variants[1].Price.Should().Be(12.99m);
        
        _pizzaRepositoryMock.Verify(x => x.GetByIdWithVariantsAsync(pizzaId), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPizzaDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var pizzaId = "non-existent-id";

        _pizzaRepositoryMock
            .Setup(x => x.GetByIdWithVariantsAsync(pizzaId))
            .ReturnsAsync((Domain.Entities.Pizza?)null);

        var query = new GetPizzaByIdQuery { Id = pizzaId };

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Pizza with ID {pizzaId} not found");
        
        _pizzaRepositoryMock.Verify(x => x.GetByIdWithVariantsAsync(pizzaId), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPizzaExistsWithNoVariants_ReturnsEmptyVariantsList()
    {
        // Arrange
        var pizzaId = "pizza-456";
        var pizza = TestDataBuilder.Pizza()
            .WithId(pizzaId)
            .WithName("Special Pizza")
            .WithVariants()
            .Build();

        _pizzaRepositoryMock
            .Setup(x => x.GetByIdWithVariantsAsync(pizzaId))
            .ReturnsAsync(pizza);

        var query = new GetPizzaByIdQuery { Id = pizzaId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(pizzaId);
        result.Variants.Should().NotBeNull();
        result.Variants.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenPizzaIsNotAvailable_ReturnsWithIsAvailableFalse()
    {
        // Arrange
        var pizzaId = "pizza-789";
        var pizza = TestDataBuilder.Pizza()
            .WithId(pizzaId)
            .WithName("Discontinued Pizza")
            .IsAvailable(false)
            .Build();

        _pizzaRepositoryMock
            .Setup(x => x.GetByIdWithVariantsAsync(pizzaId))
            .ReturnsAsync(pizza);

        var query = new GetPizzaByIdQuery { Id = pizzaId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenPizzaHasVariantsWithDifferentAvailability_ReturnsMixedAvailability()
    {
        // Arrange
        var pizzaId = "pizza-mixed";
        var pizza = TestDataBuilder.Pizza()
            .WithId(pizzaId)
            .WithVariants(
                TestDataBuilder.PizzaVariant()
                    .WithSize(PizzaSize.Small)
                    .IsAvailable(true)
                    .Build(),
                TestDataBuilder.PizzaVariant()
                    .WithSize(PizzaSize.Large)
                    .IsAvailable(false)
                    .Build()
            )
            .Build();

        _pizzaRepositoryMock
            .Setup(x => x.GetByIdWithVariantsAsync(pizzaId))
            .ReturnsAsync(pizza);

        var query = new GetPizzaByIdQuery { Id = pizzaId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Variants.Should().HaveCount(2);
        result.Variants[0].IsAvailable.Should().BeTrue();
        result.Variants[1].IsAvailable.Should().BeFalse();
    }
}
