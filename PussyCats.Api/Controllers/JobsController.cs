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
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly IJobRepository jobRepository;
    private readonly IMatchRepository matchRepository;

    public JobsController(IJobRepository jobs, IMatchRepository matches)
    {
        jobRepository = jobs;
        matchRepository = matches;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? location, [FromQuery] string? type, CancellationToken ct)
    {
        var jobs = await jobRepository.GetAllAsync(ct);
        // filtering is done in-memory for now; move to repo if perf matters
        if (!string.IsNullOrEmpty(location))
            jobs = jobs.Where(job => job.Location == location).ToList();
        return Ok(jobs);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var job = await jobRepository.GetByIdAsync(id, ct);
        return job is null ? NotFound() : Ok(job);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Job job, CancellationToken ct)
    {
        var created = await jobRepository.AddAsync(job, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.JobId }, created);
    }

    [HttpGet("{id:int}/applicants")]
    public async Task<IActionResult> GetApplicants(int id, CancellationToken ct)
    {
        //var matches = await matchRepository.GetByJobIdAsync(id, ct);
        //return Ok(matches);
        throw new NotImplementedException();
    }
}