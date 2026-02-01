using MediatR;
using PizzaStore.Application.Features.Queries.Order;

namespace PizzaStore.Application.Features.Commands.Order.CancelOrder;

public record CancelOrderCommand(string OrderId) : IRequest<OrderDto>;
