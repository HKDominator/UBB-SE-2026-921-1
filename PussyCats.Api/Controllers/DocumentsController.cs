using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Helpers;
using PussyCats.Library.Services.Documents;
using PussyCats.Library.Services.Users;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService documentService;
    private readonly IUserService userService;

    public DocumentsController(IDocumentService documentService, IUserService userService)
    {
        this.documentService = documentService;
        this.userService = userService;
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUserId(int userId, CancellationToken cancellationToken)
    {
        var documents = await documentService.GetDocumentsByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        return Ok(documents);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        [FromForm] int userId,
        [FromForm] string documentName,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file provided.");

        var user = await userService.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
            return NotFound($"User {userId} not found.");

        var document = new Document
        {
            User = user,
            DocumentName = documentName,
        };

        await using var stream = file.OpenReadStream();
        try
        {
            var saved = await documentService.UploadDocumentAsync(document, stream, file.FileName, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetByUserId), new { userId = saved.User.UserId }, saved);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{documentId:int}")]
    public async Task<IActionResult> Delete(int documentId, CancellationToken cancellationToken)
    {
        try
        {
            await documentService.DeleteDocumentAsync(documentId, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{documentId:int}/path")]
    public async Task<IActionResult> GetPath(int documentId, CancellationToken cancellationToken)
    {
        try
        {
            var path = await documentService.GetDocumentPathAsync(documentId, cancellationToken).ConfigureAwait(false);
            return Ok(path);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}