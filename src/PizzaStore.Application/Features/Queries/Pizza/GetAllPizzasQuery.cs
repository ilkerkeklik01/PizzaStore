using MediatR;
using PizzaStore.Application.Features.Queries.Pizza.DTOs;

namespace PizzaStore.Application.Features.Queries.Pizza;

public class GetAllPizzasQuery : IRequest<List<PizzaResponseDto>>
{
}
