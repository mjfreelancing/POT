using CsvHelper;
using Pot.AspNetCore.Endpoints.Accounts.Import.Models;
using Pot.AspNetCore.Endpoints.Accounts.Import.Repository;
using Pot.AspNetCore.Endpoints.Expenses.Import.Models;
using Pot.AspNetCore.Endpoints.Expenses.Import.Repository;
using Pot.Data.Dtos;
using System.Globalization;

namespace Pot.AspNetCore.Extensions;

internal static class RouteBuilderExtensions
{
    public static RouteGroupBuilder AddAccountsEndpoints(this RouteGroupBuilder group)
    {
        group
            .MapGet("", Endpoints.Accounts.GetAll.Handler.GetAllAccounts)
            .Produces<List<AccountDto>>();

        group
            .MapPost("/import", async (IFormFile file, IAccountImportRepository importRepository, CancellationToken cancellationToken) =>
            {
                try
                {
                    using var reader = new StreamReader(file.OpenReadStream());
                    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                    var accounts = csv.GetRecords<AccountImport>().ToArray();

                    // TODO: test and handle errors such as conflicts
                    // TODO: add problem details
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

        return group;
    }

    public static RouteGroupBuilder AddExpensesEndpoints(this RouteGroupBuilder group)
    {
        group
            .MapGet("", Endpoints.Expenses.GetAll.Handler.GetAllExpenses)
            .Produces<List<ExpenseDto>>();

        group
            .MapPost("/import", async (IFormFile file, IExpenseImportRepository importRepository, CancellationToken cancellationToken) =>
            {
                try
                {
                    using var reader = new StreamReader(file.OpenReadStream());
                    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                    var expenses = csv.GetRecords<ExpenseImport>().ToArray();

                    await importRepository.ImportExpensesAsync(expenses, cancellationToken);
                }
                catch (Exception exception)
                {
                    return Results.BadRequest("Invalid file.");
                }

                return Results.Ok();
            })
            .Accepts<IFormFile>("multipart/form-data")
            .DisableAntiforgery();


        return group;
    }
}
