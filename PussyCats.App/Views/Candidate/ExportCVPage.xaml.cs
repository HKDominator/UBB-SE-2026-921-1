using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using PussyCats.App.ViewModels;
using PussyCats_App.Services.PdfExportService;
using System;
using System.IO;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace PussyCats_App.Views.Candidate;

public sealed partial class ExportCVPage : Page
{
    private ExportCVViewModel? viewModel;

    public ExportCVPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        loadingRing.IsActive = true;

        await CvWebView.EnsureCoreWebView2Async();

        viewModel = App.Services.GetRequiredService<ExportCVViewModel>();
        DataContext = viewModel;

        var previewUrl = viewModel.GetPreviewUrl();
        CvWebView.Source = new Uri(previewUrl);

        statusText.Text = "Loading preview...";

        loadingRing.IsActive = false;
        statusText.Text = string.Empty;
    }

    private async void OnDownloadClick(object sender, RoutedEventArgs e)
    {
        if (viewModel == null)
        {
            return;
        }

        loadingRing.IsActive = true;
        statusText.Text = "Downloading PDF...";

        try
        {
            var pdfBytes = await viewModel.DownloadPdfAsync();

            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = "CV"
            };

            savePicker.FileTypeChoices.Add("PDF", new List<string> { ".pdf" });

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            var file = await savePicker.PickSaveFileAsync();
            if (file == null)
            {
                statusText.Text = "Cancelled.";
                return;
            }

            await FileIO.WriteBytesAsync(file, pdfBytes);

            statusText.Text = "Downloaded successfully!";
        }
        catch (Exception ex)
        {
            statusText.Text = $"Error: {ex.Message}";
        }
        finally
        {
            loadingRing.IsActive = false;
        }
    }
}
