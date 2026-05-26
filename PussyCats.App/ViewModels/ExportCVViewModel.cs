using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats_App.Services.PdfExportService;
using PussyCats.Library.Services.UserProfileService;

namespace PussyCats.App.ViewModels;

public partial class ExportCVViewModel : DispatchableObservableObject
{
    private readonly IUserProfileService userProfileService;
    private readonly SessionContext? session;

    private string statusText = string.Empty;
    private bool isLoading;

    public ExportCVViewModel(IUserProfileService userProfileService, SessionContext session)
    {
        this.userProfileService = userProfileService;
        this.session = session;
    }

    public int UserId { get; set; }

    public string StatusText
    {
        get => statusText;
        set => SetProperty(ref statusText, value);
    }

    public bool IsLoading
    {
        get => isLoading;
        set => SetProperty(ref isLoading, value);
    }

    public string GetPreviewUrl()
    {
        var id = UserId > 0
            ? UserId
            : ViewModelSupport.ResolveUserId(session);

        return $"https://localhost:5001/api/users/{id}/cv/html";
    }

    public async Task<byte[]> DownloadPdfAsync(CancellationToken cancellationToken = default)
    {
        var id = UserId > 0
            ? UserId
            : ViewModelSupport.ResolveUserId(session);

        using var http = new HttpClient();

        var url = $"https://localhost:5001/api/users/{id}/cv/pdf";

        return await http.GetByteArrayAsync(url, cancellationToken);
    }
}
