using MediatR;
using PizzaStore.Application.Features.Pizza.Queries.DTOs;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Features.Pizza.Queries;

public class GetPizzasByTypeQuery : IRequest<List<PizzaResponseDto>>
{
    public PizzaType Type { get; set; }
}
