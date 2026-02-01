using MediatR;
using PizzaStore.Application.Features.Queries.Pizza.DTOs;

namespace PizzaStore.Application.Features.Queries.Pizza;

public class GetPizzaByIdQuery : IRequest<PizzaResponseDto>
{
    public string Id { get; set; } = string.Empty;
}
