using MediatR;
using PizzaStore.Application.Features.Queries.Pizza.DTOs;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Features.Queries.Pizza;

public class GetPizzasByTypeQuery : IRequest<List<PizzaResponseDto>>
{
    public PizzaType Type { get; set; }
}
