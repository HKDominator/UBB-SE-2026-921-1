using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Jobs;
using PussyCats.Library.Repositories.Matches;
using PussyCats.Library.Repositories.Skills;
using PussyCats.Library.Repositories.Users;
using System.Threading;
using System.Threading.Tasks;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/matches")]
public class MatchesController : ControllerBase
{
    private readonly IMatchRepository matchRepository;
    //private readonly MatchService _matchService;

    public MatchesController(IMatchRepository matches)
    {
        matchRepository = matches;
        //_matchService = matchService;
    }

    [HttpPost]
    public async Task<IActionResult> Apply(Match match, CancellationToken ct)
    {
        var created = await matchRepository.AddAsync(match, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.MatchId }, created);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var match = await matchRepository.GetByIdAsync(id, ct);
        return match is null ? NotFound() : Ok(match);
    }

    [HttpPatch("{id:int}/decision")]
    public async Task<IActionResult> SubmitDecision(int id, [FromBody] string decision, CancellationToken ct)
    {
        // MatchService handles the state machine — this is the "use a service" case from MergePlan
        //await _matchService.SubmitDecisionAsync(id, decision, ct);
        //return NoContent();
        throw new NotImplementedException();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await matchRepository.RemoveAsync(id, ct);
        return NoContent();
    }
}