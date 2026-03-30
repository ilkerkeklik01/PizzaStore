using MediatR;
using PizzaStore.Application.Common.Interfaces;
using PizzaStore.Application.Features.Order.Queries;

namespace PizzaStore.Application.Features.Order.Commands.CheckoutCart;

public record CheckoutCartCommand : IRequest<OrderDto>, ISecuredRequest;
