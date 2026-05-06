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
[Route("api/files")]
public class FileController : ControllerBase
{

    public FileController()
    {

    }

    [HttpGet("/files")]
    public async Task<IActionResult> GetFiles(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    [HttpGet("/files/{fileId:int}")]
    public async Task<IActionResult> GetFileById(int fileId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}