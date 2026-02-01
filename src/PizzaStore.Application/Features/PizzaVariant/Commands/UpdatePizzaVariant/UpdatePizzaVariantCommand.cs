using MediatR;

namespace PizzaStore.Application.Features.PizzaVariant.Commands.UpdatePizzaVariant;

public record UpdatePizzaVariantCommand(string Id, UpdatePizzaVariantDto UpdatePizzaVariantDto) : IRequest<UpdatePizzaVariantResponse>;
