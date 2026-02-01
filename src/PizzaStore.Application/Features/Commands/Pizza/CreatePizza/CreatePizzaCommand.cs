using MediatR;

namespace PizzaStore.Application.Features.Commands.Pizza.CreatePizza;

public record CreatePizzaCommand(CreatePizzaDto CreatePizzaDto) : IRequest<CreatePizzaResponse>;
