using MediatR;
using PizzaStore.Application.Common.Interfaces;
using PizzaStore.Application.Features.Order.Queries;

namespace PizzaStore.Application.Features.Order.Commands.CancelOrder;

public record CancelOrderCommand(string OrderId) : IRequest<OrderDto>, ISecuredRequest;
