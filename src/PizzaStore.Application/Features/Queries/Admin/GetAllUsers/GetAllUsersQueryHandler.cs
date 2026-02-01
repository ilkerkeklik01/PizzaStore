using MediatR;
using Microsoft.AspNetCore.Identity;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Features.Queries.Admin.GetAllUsers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetAllUsersQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = _userManager.Users.ToList();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(UserDto.FromEntity(user, roles));
        }

        return userDtos.OrderBy(u => u.UserName).ToList();
    }
}
