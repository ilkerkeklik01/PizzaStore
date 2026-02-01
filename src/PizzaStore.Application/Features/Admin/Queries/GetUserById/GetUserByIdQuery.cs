using MediatR;

namespace PizzaStore.Application.Features.Admin.Queries.GetUserById;

public record GetUserByIdQuery(string UserId) : IRequest<UserDto>;
