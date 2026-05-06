using PussyCats.Library.Domain;

namespace PussyCats.App.Services;

public interface IPdfExportService
{
    Task RenderProfileAsync(User profile);
    Task DownloadPdfAsync();
}
