using PussyCats.Library.Domain;

namespace PussyCats.App.Services;

public interface ICvParsingService
{
    User ParseCvFile(string content, string fileType);
}
