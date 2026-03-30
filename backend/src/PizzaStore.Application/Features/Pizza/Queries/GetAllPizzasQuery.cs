using MediatR;
using PizzaStore.Application.Features.Pizza.Queries.DTOs;

namespace PizzaStore.Application.Features.Pizza.Queries;

public class GetAllPizzasQuery : IRequest<List<PizzaResponseDto>>
{
}
