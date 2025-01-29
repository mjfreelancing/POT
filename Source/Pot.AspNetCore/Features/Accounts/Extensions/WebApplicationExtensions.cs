namespace Pot.AspNetCore.Features.Accounts.Extensions;

internal static class WebApplicationExtensions
{
    private const long MaxImportPayloadBytes = 1 * 1024 * 1024;

    public static WebApplication AddAccountEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Account Routes]"))
        {
            app.Logger.LogInformation("Adding account endpoints");

            app.MapGroup("api/accounts")
                //.RequireAuthorization()
                .WithTags("Accounts Api")
                .GetAllAccounts()
                .GetAccount()
                .CreateAccount()
                .UpdateAccount()
                .ImportAccounts(MaxImportPayloadBytes);



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
