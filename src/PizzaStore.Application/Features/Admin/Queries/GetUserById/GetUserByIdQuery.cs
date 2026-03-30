using MediatR;
using PizzaStore.Application.Common.Interfaces;

namespace PizzaStore.Application.Features.Admin.Queries.GetUserById;

public record GetUserByIdQuery(string UserId) : IRequest<UserDto>, IAdminRequest;
