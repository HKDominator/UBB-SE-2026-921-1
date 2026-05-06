using PussyCats.Library.Domain;

namespace PussyCats.App.Services;

public interface ICompletenessService
{
    int CalculateCompleteness(User? user);
    string GetNextEmptyFieldPrompt(User? user);
}
