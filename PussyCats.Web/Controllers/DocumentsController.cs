using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Documents;
using PussyCats.Web.Configuration;

namespace PussyCats.Web.Controllers;

public class DocumentsController : Controller
{
    private readonly IDocumentService service;
    private readonly int DefaultUserId;

    public DocumentsController(IDocumentService service, ApiConfiguration config)
    {
        this.service = service;
        DefaultUserId=config.TemporaryUserId;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (TempData["UploadError"] != null)
        {
            ModelState.AddModelError("file", TempData["UploadError"].ToString()!);
        }
        if (TempData["StatusMessage"] != null)
        {
            ViewBag.StatusMessage = TempData["StatusMessage"].ToString();
        }

        var documents = await service.GetDocumentsByUserIdAsync(DefaultUserId, ct);

        // Resolve target file URLs ahead of time for direct browser downloading/viewing
        var fileUrlMapping = new Dictionary<int, string>();
        foreach (var doc in documents)
        {
            try
            {
                var fullPath = await service.GetDocumentPathAsync(doc.DocumentId, ct);
                fileUrlMapping[doc.DocumentId] = fullPath;
            }
            catch
            {
                fileUrlMapping[doc.DocumentId] = "#"; // Fallback placeholder if missing
            }
        }

        ViewBag.FileUrls = fileUrlMapping;
        return View(documents);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(string documentName, IFormFile? file, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(documentName))
        {
            TempData["UploadError"] = "Document name is required.";
            return RedirectToAction(nameof(Index));
        }

        if (file == null || file.Length == 0)
        {
            TempData["UploadError"] = "Please select a valid file to upload.";
            return RedirectToAction(nameof(Index));
        }

        var allowedExtensions = new[] { ".pdf", ".jpg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            TempData["UploadError"] = "Accepted formats are restricted to: PDF, JPG, PNG.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var document = new Document
            {
                DocumentName = documentName,
                User = new User { UserId = DefaultUserId }
            };

            using var stream = file.OpenReadStream();
            await service.UploadDocumentAsync(document, stream, file.FileName, ct);
        }
        catch (Exception ex)
        {
            TempData["UploadError"] = $"Error uploading file to server: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            await service.DeleteDocumentAsync(id, ct);
        }
        catch (Exception ex)
        {
            TempData["StatusMessage"] = $"Error deleting document: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}