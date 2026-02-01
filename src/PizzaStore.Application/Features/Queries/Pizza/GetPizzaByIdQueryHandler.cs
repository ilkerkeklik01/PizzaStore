using MediatR;
using PizzaStore.Application.Features.Queries.Pizza.DTOs;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Interfaces;

namespace PizzaStore.Application.Features.Queries.Pizza;

/// <summary>
/// Retrieves a single pizza by ID with its variants
/// </summary>
public class GetPizzaByIdQueryHandler : IRequestHandler<GetPizzaByIdQuery, PizzaResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPizzaByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PizzaResponseDto> Handle(GetPizzaByIdQuery request, CancellationToken cancellationToken)
    {
        var pizza = await _unitOfWork.Pizzas.GetByIdWithVariantsAsync(request.Id);

        if (pizza == null)
        {
            throw new NotFoundException($"Pizza with ID {request.Id} not found");
        }

        return MapToDto(pizza);
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
