using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Concerns.Time;
using Pot.App.Features.Maintenance.Export;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.AspNetCore.Features.Maintenance.Export;
using Shouldly;
using System.Text;

namespace Pot.AspNetCore.Tests.Features.Maintenance.Export;

public class HandlerFixture
{
    private static readonly DateTime LocalNow = new(2026, 9, 24, 22, 23, 10, DateTimeKind.Local);
    private static readonly byte[] ExportContent = Encoding.UTF8.GetBytes("pot-export-package");

    private readonly IExportDataService _exportDataService = Substitute.For<IExportDataService>();
    private readonly ITimeProvider _timeProvider = Substitute.For<ITimeProvider>();
    private readonly ILogger<Handler> _logger = Substitute.For<ILogger<Handler>>();

    public HandlerFixture()
    {
        _exportDataService
            .ExportAllAsync(Arg.Any<CancellationToken>())
            .Returns(ExportContent);

        _timeProvider
            .GetLocalDateTimeNow()
            .Returns(LocalNow);
    }

    [Fact]
    public async Task Should_Name_The_Download_With_The_Export_Timestamp_And_The_Current_Package_Version()
    {
        var result = await InvokeAsync();

        var fileResult = result.Result.ShouldBeOfType<FileStreamHttpResult>();

        // The version must track MetadataBase.CurrentVersion rather than a locally hardcoded value, so a
        // package format bump cannot leave the download name advertising the previous version.
        fileResult.FileDownloadName.ShouldBe($"pot-2026-09-24_222310.v{MetadataBase.CurrentVersion}.export");
    }

    [Fact]
    public async Task Should_Return_The_Export_Content_As_A_Seekable_Octet_Stream()
    {
        var result = await InvokeAsync();

        var fileResult = result.Result.ShouldBeOfType<FileStreamHttpResult>();

        fileResult.ContentType.ShouldBe("application/octet-stream");
        fileResult.FileStream.CanSeek.ShouldBeTrue();
        fileResult.FileStream.Position.ShouldBe(0);

        using var contentStream = new MemoryStream();

        await fileResult.FileStream.CopyToAsync(contentStream, TestContext.Current.CancellationToken);

        contentStream.ToArray().ShouldBe(ExportContent);
    }

    private Task<Results<FileStreamHttpResult, ProblemHttpResult>> InvokeAsync()
    {
        return Handler.Invoke(_exportDataService, _timeProvider, _logger, TestContext.Current.CancellationToken);
    }
}
