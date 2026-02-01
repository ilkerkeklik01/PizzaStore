using MediatR;
using PizzaStore.Application.Features.Queries.Order;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Features.Commands.Admin.UpdateOrderStatus;

public record UpdateOrderStatusCommand(
    string OrderId,
    OrderStatus NewStatus
) : IRequest<OrderDto>;
