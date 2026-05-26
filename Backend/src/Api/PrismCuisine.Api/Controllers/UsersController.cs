using MediatR;
using Microsoft.AspNetCore.Mvc;
using PrismCuisine.Modules.Identity.Application.Users.Queries.GetUserById;

namespace PrismCuisine.Api.Controllers;

[ApiController]
[Route("api/identity/users")]
public sealed class UsersController(ISender sender) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await sender.Send(new GetUserByIdQuery(id), cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }
}
