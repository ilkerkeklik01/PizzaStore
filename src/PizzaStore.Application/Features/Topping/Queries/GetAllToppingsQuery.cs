using MediatR;
using PizzaStore.Application.Features.Topping.Queries.DTOs;

namespace PizzaStore.Application.Features.Topping.Queries;

public record GetAllToppingsQuery : IRequest<List<ToppingResponseDto>>;
