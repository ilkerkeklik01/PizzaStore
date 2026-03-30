using MediatR;
using PizzaStore.Application.Common.Interfaces;
using PizzaStore.Application.Features.Order.Queries;

namespace PizzaStore.Application.Features.Admin.Queries.GetOrdersByUserId;

public record GetOrdersByUserIdQuery(string UserId) : IRequest<List<OrderDto>>, IAdminRequest;
