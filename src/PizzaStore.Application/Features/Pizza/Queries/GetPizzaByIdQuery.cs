using MediatR;
using PizzaStore.Application.Features.Pizza.Queries.DTOs;

namespace PizzaStore.Application.Features.Pizza.Queries;

public class GetPizzaByIdQuery : IRequest<PizzaResponseDto>
{
    public string Id { get; set; } = string.Empty;
}
