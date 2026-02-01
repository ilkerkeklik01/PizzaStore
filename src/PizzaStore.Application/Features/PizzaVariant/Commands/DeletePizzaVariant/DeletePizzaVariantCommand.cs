using MediatR;

namespace PizzaStore.Application.Features.PizzaVariant.Commands.DeletePizzaVariant;

public record DeletePizzaVariantCommand(string Id) : IRequest<DeletePizzaVariantResponse>;
