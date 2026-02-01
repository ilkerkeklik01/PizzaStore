using MediatR;
using PizzaStore.Application.Features.Queries.Topping.DTOs;

namespace PizzaStore.Application.Features.Queries.Topping;

public record GetAllToppingsQuery : IRequest<List<ToppingResponseDto>>;
