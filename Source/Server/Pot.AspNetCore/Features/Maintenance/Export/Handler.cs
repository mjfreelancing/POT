using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Maintenance.Export;

namespace Pot.AspNetCore.Features.Maintenance.Export;

internal sealed class Handler
{
    //internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    public static async Task<Results<FileStreamHttpResult, ProblemHttpResult>> Invoke(
        IExportDataService exportAccountsService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);



        //var deletedResult = await incomeService.DeleteIncomeAsync(id, cancellationToken);

        //if (deletedResult.IsSuccess)
        //{
        //    return deletedResult.Value
        //        ? TypedResults.Ok()
        //        : TypedResults.NotFound();
        //}

        //return TypedResults.Problem(deletedResult.Error!.GetProblemDetails());


        var content = await exportAccountsService.ExportAllAsync(cancellationToken);




        var stream = new MemoryStream();

        await stream.WriteAsync(content, cancellationToken);
        await stream.FlushAsync(cancellationToken);
        stream.Seek(0, SeekOrigin.Begin);

        //var timestamp = $"{TimeProvider.GetLocalNow().DateTime}:yyyy-MM-dd-HH-mm-ss";

        var fileName = $"pot-export-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.zip";

        // The stream is disposed of after the response is sent.
        return TypedResults.File(stream, "application/zip", fileName);
    }
}