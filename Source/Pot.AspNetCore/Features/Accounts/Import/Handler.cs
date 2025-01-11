using AllOverIt.Validation;
using CsvHelper;
using Pot.AspNetCore.Features.Accounts.Import.Models;
using Pot.AspNetCore.Features.Accounts.Import.Repository;
using System.Globalization;

namespace Pot.AspNetCore.Features.Accounts.Import;

internal static class Handler
{
    public static async Task<IResult> Invoke(IFormFile file, ILifetimeValidationInvoker validationInvoker, IAccountImportRepository importRepository, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var accounts = csv.GetRecords<AccountImport>().ToArray();

        validationInvoker.AssertValidation(accounts, cancellationToken);

        var importResult = await importRepository.ImportAccountsAsync(accounts, cancellationToken).ConfigureAwait(false);

        return Results.Ok(importResult);
    }
}
