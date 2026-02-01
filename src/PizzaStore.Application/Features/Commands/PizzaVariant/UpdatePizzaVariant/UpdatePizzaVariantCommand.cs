using MediatR;

namespace PizzaStore.Application.Features.Commands.PizzaVariant.UpdatePizzaVariant;

public record UpdatePizzaVariantCommand(string Id, UpdatePizzaVariantDto UpdatePizzaVariantDto) : IRequest<UpdatePizzaVariantResponse>;
