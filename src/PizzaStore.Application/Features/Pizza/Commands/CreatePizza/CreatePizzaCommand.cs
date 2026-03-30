using MediatR;
using PizzaStore.Application.Common.Interfaces;

namespace PizzaStore.Application.Features.Pizza.Commands.CreatePizza;

public record CreatePizzaCommand(CreatePizzaDto CreatePizzaDto) : IRequest<CreatePizzaResponse>, IAdminRequest;
