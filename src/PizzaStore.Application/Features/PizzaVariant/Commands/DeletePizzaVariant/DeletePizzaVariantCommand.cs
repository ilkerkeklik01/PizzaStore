using MediatR;
using PizzaStore.Application.Common.Interfaces;

namespace PizzaStore.Application.Features.PizzaVariant.Commands.DeletePizzaVariant;

public record DeletePizzaVariantCommand(string Id) : IRequest<DeletePizzaVariantResponse>, IAdminRequest;
