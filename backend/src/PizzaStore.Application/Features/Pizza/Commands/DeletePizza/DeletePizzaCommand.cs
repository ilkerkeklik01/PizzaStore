using MediatR;
using PizzaStore.Application.Common.Interfaces;

namespace PizzaStore.Application.Features.Pizza.Commands.DeletePizza;

public record DeletePizzaCommand(string Id) : IRequest<DeletePizzaResponse>, IAdminRequest;
