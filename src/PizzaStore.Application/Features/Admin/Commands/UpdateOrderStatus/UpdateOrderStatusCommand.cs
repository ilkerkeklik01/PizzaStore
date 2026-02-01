using MediatR;
using PizzaStore.Application.Features.Order.Queries;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Features.Admin.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand(
    string OrderId,
    OrderStatus NewStatus
) : IRequest<OrderDto>;
