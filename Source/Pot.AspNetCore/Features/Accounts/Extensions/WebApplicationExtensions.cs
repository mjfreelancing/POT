using CsvHelper;
using Pot.AspNetCore.Features.Accounts.Import.Models;
using Pot.AspNetCore.Features.Accounts.Import.Repository;
using Pot.Data.Dtos;
using System.Globalization;

namespace Pot.AspNetCore.Features.Accounts.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication AddAccounts(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Account Routes]"))
        {
            app.Logger.LogInformation("Adding account endpoints");

            var group = app
                .MapGroup("api/accounts")
                .WithTags("Accounts Api");

            group
                .MapGet("", GetAll.Handler.GetAllAccounts)
                .Produces<List<AccountDto>>();

            group
                .MapPost("/import", async (IFormFile file, IAccountImportRepository importRepository, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        using var reader = new StreamReader(file.OpenReadStream());
                        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                        var accounts = csv.GetRecords<AccountImport>().ToArray();

                        await importRepository.ImportAccountsAsync(accounts, cancellationToken);
                    }
                    catch (Exception exception)
                    {
                        return Results.BadRequest("Invalid file.");
                    }

                    return Results.Ok();
                })
                .Accepts<IFormFile>("multipart/form-data")
                .DisableAntiforgery();

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
