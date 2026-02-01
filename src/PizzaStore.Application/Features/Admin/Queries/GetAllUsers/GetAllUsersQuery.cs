using MediatR;

namespace PizzaStore.Application.Features.Admin.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<List<UserDto>>;
