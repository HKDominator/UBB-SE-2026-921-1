using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Companies;
using PussyCats.Library.Repositories.Jobs;
using PussyCats.Library.Repositories.Matches;
using PussyCats.Library.Repositories.Skills;
using PussyCats.Library.Repositories.Users;
using System.Threading;
using System.Threading.Tasks;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/companies")]
public class CompanyController : ControllerBase
{
    private readonly ICompanyRepository companyRepository;
    private readonly IMatchRepository matchRepository;
    //private readonly CompanyRecommendationService _recommendations;

    public CompanyController(
        ICompanyRepository companies,
        IMatchRepository matches
        //CompanyRecommendationService recommendations
        )
    {
        companyRepository = companies;
        matchRepository = matches;
        //_recommendations = recommendations;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var company = await companyRepository.GetByIdAsync(id, ct);
        return company is null ? NotFound() : Ok(company);
    }

    [HttpGet("{id:int}/applicants")]
    public async Task<IActionResult> GetApplicants(int id, CancellationToken ct)
    {
        //var matches = await matchRepository.GetByCompanyIdAsync(id, ct);
        //return Ok(matches);
        throw new NotImplementedException();
    }

    [HttpGet("{id:int}/recommendations")]
    public async Task<IActionResult> GetRecommendations(int id, CancellationToken ct)
    {
        //var recs = await _recommendations.GetRecommendationsForCompanyAsync(id, ct);
        //return Ok(recs);
        throw new NotImplementedException();
    }
}