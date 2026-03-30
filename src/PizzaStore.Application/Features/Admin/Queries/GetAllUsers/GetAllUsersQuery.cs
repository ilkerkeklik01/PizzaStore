using MediatR;
using PizzaStore.Application.Common.Interfaces;

namespace PizzaStore.Application.Features.Admin.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<List<UserDto>>, IAdminRequest;
