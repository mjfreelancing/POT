using Microsoft.AspNetCore.Mvc;
using Pot.AspNetCore.Features.Accounts.Import.Models;
using Pot.Data.Dtos;

namespace Pot.AspNetCore.Features.Accounts.Extensions;

public static class WebApplicationExtensions
{
    private static readonly long _maxImportPayloadBytes = 1 * 1024 * 1024;

    public static WebApplication AddAccountEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Account Routes]"))
        {
            app.Logger.LogInformation("Adding account endpoints");

            var group = app
                .MapGroup("api/accounts")
                .WithTags("Accounts Api");

            group
                .MapGet("", GetAll.Handler.Invoke)
                .Produces<List<AccountDto>>();

            group
                .MapPost("/import", Import.Handler.Invoke)
                .WithMetadata(new RequestSizeLimitAttribute(_maxImportPayloadBytes))    // Will raise 413 Payload Too Large if the file exceeds this limit
                .DisableAntiforgery()
                .Produces<ImportResult>();

            //group.MapGet("/export/{fileName}", (string fileName) =>
            //{
            //    var filePath = Path.Combine("Exports", fileName);

            //    if (!File.Exists(filePath))
            //    {
            //        return Results.NotFound("File not found.");
            //    }

            //    var fileBytes = File.ReadAllBytes(filePath);

            //    return Results.File(fileBytes, "application/octet-stream", fileName);
            //});

            //group.MapGet("/export-stream/{fileName}", async (string fileName, HttpContext httpContext, CancellationToken cancellationToken) =>
            //{
            //    var filePath = Path.Combine("Exports", fileName);

            //    if (!File.Exists(filePath))
            //    {
            //        return Results.NotFound("File not found.");
            //    }

            //    httpContext.Response.ContentType = "application/octet-stream";
            //    httpContext.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");

            //    using (var stream = new FileStream(filePath, FileMode.Open))
            //    {
            //        await stream.CopyToAsync(httpContext.Response.Body, cancellationToken);
            //    }

            //    return Results.Ok();
            //});
        }

        return app;
    }
}
