using MediatR;
using Microsoft.AspNetCore.Identity;
using PizzaStore.Core.CrossCuttingConcerns.Exceptions;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Features.Queries.Admin.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserByIdQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException($"User with ID '{request.UserId}' not found");

        var roles = await _userManager.GetRolesAsync(user);
        return UserDto.FromEntity(user, roles);
    }
}
