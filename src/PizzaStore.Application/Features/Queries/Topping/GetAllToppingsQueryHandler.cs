using MediatR;
using PizzaStore.Application.Features.Queries.Topping.DTOs;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Queries.Topping;

/// <summary>
/// Retrieves all toppings including their availability status
/// </summary>
public class GetAllToppingsQueryHandler : IRequestHandler<GetAllToppingsQuery, List<ToppingResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllToppingsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ToppingResponseDto>> Handle(GetAllToppingsQuery request, CancellationToken cancellationToken)
    {
        var toppings = await _unitOfWork.Toppings.GetAllAsync();

        return toppings
            .Select(MapToDto)
            .ToList();
    }

    private ToppingResponseDto MapToDto(Domain.Entities.Topping topping)
    {
        return new ToppingResponseDto
        {
            Id = topping.Id,
            Name = topping.Name,
            Price = topping.Price,
            IsAvailable = topping.IsAvailable
        };
    }
}
