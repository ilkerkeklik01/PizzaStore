using MediatR;

namespace PizzaStore.Application.Features.Queries.Admin.GetUserById;

public record GetUserByIdQuery(string UserId) : IRequest<UserDto>;
