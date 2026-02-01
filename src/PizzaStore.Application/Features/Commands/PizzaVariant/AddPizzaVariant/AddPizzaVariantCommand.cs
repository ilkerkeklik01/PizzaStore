using MediatR;

namespace PizzaStore.Application.Features.Commands.PizzaVariant.AddPizzaVariant;

public record AddPizzaVariantCommand(AddPizzaVariantDto AddPizzaVariantDto) : IRequest<AddPizzaVariantResponse>;
