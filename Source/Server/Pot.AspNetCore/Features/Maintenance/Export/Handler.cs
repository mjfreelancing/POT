using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Concerns.Time;
using Pot.App.Features.Maintenance.Export;

namespace Pot.AspNetCore.Features.Maintenance.Export;

internal sealed class Handler
{
    public static async Task<Results<FileStreamHttpResult, ProblemHttpResult>> Invoke(
        IExportDataService exportAccountsService, ITimeProvider timeProvider,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var content = await exportAccountsService.ExportAllAsync(cancellationToken);

        var stream = new MemoryStream();    // This will be disposed of by the response

        await stream.WriteAsync(content, cancellationToken);
        await stream.FlushAsync(cancellationToken);
        stream.Position = 0;

        var timestamp = timeProvider.GetLocalDateTimeNow();
        var fileName = $"pot-{timestamp:yyyy-MM-dd_HHmmss}.export";

        // The stream is disposed of after the response is sent.
        return TypedResults.File(stream, "application/octet-stream", fileName);
    }
}
