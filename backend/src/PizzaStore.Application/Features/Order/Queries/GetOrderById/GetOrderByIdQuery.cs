using MediatR;
using PizzaStore.Application.Common.Interfaces;

namespace PizzaStore.Application.Features.Order.Queries.GetOrderById;

public record GetOrderByIdQuery(string OrderId) : IRequest<OrderDto>, ISecuredRequest;
