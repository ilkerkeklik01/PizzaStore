using MediatR;
using PizzaStore.Application.Common.Interfaces;

namespace PizzaStore.Application.Features.Cart.Commands.ClearCart;

public record ClearCartCommand : IRequest<ClearCartResponse>, ISecuredRequest;
