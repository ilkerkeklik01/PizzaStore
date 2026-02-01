using MediatR;

namespace PizzaStore.Application.Features.Commands.PizzaVariant.DeletePizzaVariant;

public record DeletePizzaVariantCommand(string Id) : IRequest<DeletePizzaVariantResponse>;
