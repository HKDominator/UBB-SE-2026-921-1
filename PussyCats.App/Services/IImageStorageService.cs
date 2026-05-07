namespace PussyCats.App.Services;

public interface IImageStorageService
{
    string SaveImage(Stream fileStream, string fileName);

    void DeleteImage(string relativePath);

    void CheckFileSize(Stream fileStream);
}
