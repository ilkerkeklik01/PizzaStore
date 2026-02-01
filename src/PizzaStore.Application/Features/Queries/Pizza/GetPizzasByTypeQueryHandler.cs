using MediatR;
using PizzaStore.Application.Features.Queries.Pizza.DTOs;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Queries.Pizza;

/// <summary>
/// Retrieves available pizzas filtered by pizza type
/// </summary>
public class GetPizzasByTypeQueryHandler : IRequestHandler<GetPizzasByTypeQuery, List<PizzaResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPizzasByTypeQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PizzaResponseDto>> Handle(GetPizzasByTypeQuery request, CancellationToken cancellationToken)
    {
        var pizzas = await _unitOfWork.Pizzas.GetAvailablePizzasWithVariantsAsync();

        var filteredPizzas = pizzas
            .Where(p => p.Type == request.Type)
            .ToList();

        return filteredPizzas
            .Select(MapToDto)
            .ToList();
    }

    private PizzaResponseDto MapToDto(Domain.Entities.Pizza pizza)
    {
        return new PizzaResponseDto
        {
            Id = pizza.Id,
            Name = pizza.Name,
            Description = pizza.Description,
            Type = pizza.Type,
            ImageUrl = pizza.ImageUrl,
            IsAvailable = pizza.IsAvailable,
            Variants = pizza.Variants
                ?.Select(v => new VariantResponseDto
                {
                    Id = v.Id,
                    Size = v.Size,
                    Price = v.Price,
                    IsAvailable = v.IsAvailable
                })
                .ToList() ?? new List<VariantResponseDto>()
        };
    }
}
