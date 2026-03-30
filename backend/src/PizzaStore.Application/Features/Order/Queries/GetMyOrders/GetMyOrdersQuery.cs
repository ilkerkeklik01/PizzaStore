using MediatR;
using PizzaStore.Application.Common.Interfaces;

namespace PizzaStore.Application.Features.Order.Queries.GetMyOrders;

public record GetMyOrdersQuery : IRequest<List<OrderDto>>, ISecuredRequest;
