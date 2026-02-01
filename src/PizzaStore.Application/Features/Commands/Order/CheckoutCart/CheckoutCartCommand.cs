using MediatR;
using PizzaStore.Application.Features.Queries.Order;

namespace PizzaStore.Application.Features.Commands.Order.CheckoutCart;

public record CheckoutCartCommand : IRequest<OrderDto>;
