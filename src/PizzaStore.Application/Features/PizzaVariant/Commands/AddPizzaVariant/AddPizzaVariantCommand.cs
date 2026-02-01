using MediatR;

namespace PizzaStore.Application.Features.PizzaVariant.Commands.AddPizzaVariant;

public record AddPizzaVariantCommand(AddPizzaVariantDto AddPizzaVariantDto) : IRequest<AddPizzaVariantResponse>;
