using MediatR;
using PizzaStore.Application.Features.Order.Queries;

namespace PizzaStore.Application.Features.Order.Commands.CheckoutCart;

public record CheckoutCartCommand : IRequest<OrderDto>;
