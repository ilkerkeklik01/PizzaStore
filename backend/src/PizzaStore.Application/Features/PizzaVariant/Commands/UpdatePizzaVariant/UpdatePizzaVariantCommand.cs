using MediatR;
using PizzaStore.Application.Common.Interfaces;

namespace PizzaStore.Application.Features.PizzaVariant.Commands.UpdatePizzaVariant;

public record UpdatePizzaVariantCommand(string Id, UpdatePizzaVariantDto UpdatePizzaVariantDto) : IRequest<UpdatePizzaVariantResponse>, IAdminRequest;
