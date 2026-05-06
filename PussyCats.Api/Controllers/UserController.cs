using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Users;
using System.Threading;
using System.Threading.Tasks;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _users;

    public UsersController(IUserRepository users) => _users = users;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await _users.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create(User user, CancellationToken cancellationToken)
    {
        var created = await _users.AddAsync(user, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.UserId }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, User user, CancellationToken cancellationToken)
    {
        if (id != user.UserId) return BadRequest("Route id does not match body id.");
        await _users.UpdateAsync(user, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _users.RemoveAsync(id, cancellationToken);
        return NoContent();
    }
}