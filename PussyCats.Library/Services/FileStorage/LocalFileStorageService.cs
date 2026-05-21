using PussyCats.Library.Services.FileStorage;

namespace PussyCats.Api.Services;

public class LocalFileStorageService : ILocalFileStorageService
{
    private readonly string uploadsPath = Path.Combine("uploads", "files");

    public LocalFileStorageService()
    {
        Directory.CreateDirectory(uploadsPath);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(originalFileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(uploadsPath, fileName);

        await using var stream = File.Create(fullPath);
        await fileStream.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);

        return fileName;
    }

    public async Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(uploadsPath, Path.GetFileName(relativePath));
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(uploadsPath, Path.GetFileName(relativePath));
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File not found.", relativePath);

        var memory = new MemoryStream();
        await using var fileStream = File.OpenRead(fullPath);
        await fileStream.CopyToAsync(memory, cancellationToken).ConfigureAwait(false);
        memory.Position = 0;
        return memory;
    }

    public string GetUrl(string relativePath)
    {
        return relativePath; // on the server, path is already relative; FilesController serves it
    }
}