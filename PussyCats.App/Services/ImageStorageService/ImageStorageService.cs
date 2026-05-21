using PussyCats.Library.Services.FileStorage;
using PussyCats.Library.Services.ImageStorageService;

namespace PussyCats_App.Services.ImageStorageService;

//this needs to stay here
public class ImageStorageService : IImageStorageService
{
    private const int BytesPerKilobyte = 1024;
    private const int BytesPerMegabyte = 1024 * BytesPerKilobyte;
    private const int MaxFileSizeInMb = 20;
    private const int MaxFileSize = MaxFileSizeInMb * BytesPerMegabyte;

    private readonly ILocalFileStorageService fileStorageService;
    private readonly HashSet<string> allowedExtensions = new() { ".jpg", ".jpeg", ".png" };

    public ImageStorageService(ILocalFileStorageService fileStorageService)
    {
        this.fileStorageService = fileStorageService;
    }

    public Task<string> SaveImageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var extension = GetImageExtension(fileName);
        CheckFileSize(fileStream);
        return fileStorageService.SaveFileAsync(fileStream, $"upload{extension}", cancellationToken);
    }

    public Task DeleteImageAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        return fileStorageService.DeleteFileAsync(relativePath, cancellationToken);
    }

    private string GetImageExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            throw new ArgumentException(
                $"Unsupported file type. Allowed formats are: {string.Join(", ", allowedExtensions.Order())}");
        }

        return extension;
    }

    public void CheckFileSize(Stream fileStream)
    {
        if (fileStream.Length > MaxFileSize)
        {
            var fileSizeInMb = fileStream.Length / (double)BytesPerMegabyte;
            throw new InvalidOperationException(
                $"File size exceeds the maximum limit of {MaxFileSizeInMb} MB. Selected file is {fileSizeInMb:0.##} MB.");
        }
    }
}