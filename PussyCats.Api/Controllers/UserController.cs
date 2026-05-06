using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Matches;
using PussyCats.Library.Repositories.Skills;
using PussyCats.Library.Repositories.Users;
using System.Threading;
using System.Threading.Tasks;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository userRepository;
    private readonly IUserSkillRepository userSkillRepository;
    private readonly IMatchRepository matchRepository;

    public UsersController(
        IUserRepository users,
        IUserSkillRepository userSkills,
        IMatchRepository matches)
    {
        userRepository = users;
        userSkillRepository = userSkills;
        matchRepository = matches;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(id, ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, User user, CancellationToken ct)
    {
        if (id != user.UserId) return BadRequest("Route id does not match body id.");
        await userRepository.UpdateAsync(user, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/skills")]
    public async Task<IActionResult> AddSkill(int id, UserSkill skill, CancellationToken ct)
    {
        skill.UserId = id;
        var created = await userSkillRepository.AddAsync(skill, ct);
        return Ok(created);
    }

    [HttpDelete("{id:int}/skills/{skillId:int}")]
    public async Task<IActionResult> RemoveSkill(int id, int skillId, CancellationToken ct)
    {
        await userSkillRepository.RemoveAsync(id, skillId, ct);
        return NoContent();
    }

    [HttpGet("{id:int}/compatibility")]
    public async Task<IActionResult> GetCompatibility(int id, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(id, ct);
        if (user is null) return NotFound();
        // var result = await compatibilityService.ComputeAsync(user, ct);
        //return Ok(result);
        throw new NotImplementedException();
    }

    [HttpGet("{id:int}/matches")]
    public async Task<IActionResult> GetMatches(int id, CancellationToken ct)
    {
        var matches = await matchRepository.GetByUserIdAsync(id, ct);
        return Ok(matches);
    }

    [HttpPost("{id:int}/personality-test")]
    public async Task<IActionResult> SubmitPersonalityTest(
        int id,
        [FromBody] Dictionary<string, string> answers,
        CancellationToken ct)
    {
        // Phase 5 wires the real PersonalityTestService here
        throw new NotImplementedException("Personality test submission wired in Phase 5.");
    }

    [HttpPost("{id:int}/cv")]
    public async Task<IActionResult> UploadCv(int id, IFormFile file, CancellationToken ct)
    {
        // Phase 5 wires CVParsingService + file storage here
        throw new NotImplementedException("CV upload wired in Phase 5.");
    }
}