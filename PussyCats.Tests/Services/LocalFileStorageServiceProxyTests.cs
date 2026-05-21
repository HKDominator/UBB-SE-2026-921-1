using FluentAssertions;
using NSubstitute;
using PussyCats.App.RepositoryProxies;
using PussyCats.Library.Services.FileStorage;
using PussyCats_App.ServiceProxies;
using PussyCats.App.ServiceProxies;

namespace PussyCats.Tests.Services;

public class LocalFileStorageServiceProxyTests
{
    private readonly IFilesProxy filesProxy = Substitute.For<IFilesProxy>();
    private readonly LocalFileStorageServiceProxy serviceProxy;

    public LocalFileStorageServiceProxyTests()
    {
        serviceProxy = new LocalFileStorageServiceProxy(filesProxy);
    }

    [Fact]
    public async Task SaveFileAsync_ValidStreamProvided_DelegatesUploadToFilesProxy()
    {
        filesProxy.UploadAsync(Arg.Any<Stream>(), "x.pdf", Arg.Any<CancellationToken>())
            .Returns("uploads/x.pdf");
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        var result = await serviceProxy.SaveFileAsync(stream, "x.pdf");

        result.Should().Be("uploads/x.pdf");
        await filesProxy.Received(1).UploadAsync(stream, "x.pdf", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteFileAsync_FilePathProvided_DelegatesToFilesProxy()
    {
        await serviceProxy.DeleteFileAsync("uploads/x.pdf");

        await filesProxy.Received(1).DeleteAsync("uploads/x.pdf", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OpenReadAsync_delegates_to_files_proxy()
    {
        using var stream = new MemoryStream(new byte[] { 4, 5, 6 });
        filesProxy.DownloadAsync("uploads/x.pdf", Arg.Any<CancellationToken>()).Returns(stream);

        var result = await serviceProxy.OpenReadAsync("uploads/x.pdf");

        result.Should().BeSameAs(stream);
        await filesProxy.Received(1).DownloadAsync("uploads/x.pdf", Arg.Any<CancellationToken>());
    }

    [Fact]
    public void GetFilePath_RelativePathProvided_ReturnsProxyResolvedUrl()
    {
        filesProxy.GetUrl("uploads/x.pdf").Returns("https://api/api/files/x.pdf");

        serviceProxy.GetFilePath("uploads/x.pdf").Should().Be("https://api/api/files/x.pdf");
    }

    [Fact]
    public void GetFilePath_PathIsNull_ThrowsArgumentNullException()
    {
        Action act = () => serviceProxy.GetFilePath(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}