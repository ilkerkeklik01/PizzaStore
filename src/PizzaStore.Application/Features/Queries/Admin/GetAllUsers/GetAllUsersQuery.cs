using MediatR;

namespace PizzaStore.Application.Features.Queries.Admin.GetAllUsers;

public record GetAllUsersQuery : IRequest<List<UserDto>>;
