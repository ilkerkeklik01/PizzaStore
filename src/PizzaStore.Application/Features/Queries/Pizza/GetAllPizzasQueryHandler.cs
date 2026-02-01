using MediatR;
using PizzaStore.Application.Features.Queries.Pizza.DTOs;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Queries.Pizza;

/// <summary>
/// Retrieves all available pizzas with their variants
/// </summary>
public class GetAllPizzasQueryHandler : IRequestHandler<GetAllPizzasQuery, List<PizzaResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllPizzasQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PizzaResponseDto>> Handle(GetAllPizzasQuery request, CancellationToken cancellationToken)
    {
        var pizzas = await _unitOfWork.Pizzas.GetAvailablePizzasWithVariantsAsync();

        return pizzas
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
