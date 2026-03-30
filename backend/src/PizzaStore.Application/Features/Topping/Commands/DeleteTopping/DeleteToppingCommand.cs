using MediatR;
using PizzaStore.Application.Common.Interfaces;

namespace PizzaStore.Application.Features.Topping.Commands.DeleteTopping;

public record DeleteToppingCommand(string Id) : IRequest<DeleteToppingResponse>, IAdminRequest;
