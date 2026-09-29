using AllOverIt.Assertion;
using AllOverIt.Csv;
using AllOverIt.Csv.Exporter;
using Pot.App.Features.Accounts.GetAll;
using Pot.App.Features.Maintenance.Export.Models;

namespace Pot.App.Features.Maintenance.Export.Accounts;

/// <summary>
/// Default implementation of <see cref="IAccountsExporter"/>.
/// </summary>
internal sealed class AccountsExporter : MemoryCsvExporterBase<AccountData>, IAccountsExporter
{
    private readonly IGetAllAccountsService _expensesService;

    public AccountsExporter(IGetAllAccountsService expensesService)
    {
        _expensesService = expensesService.WhenNotNull();
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportAllAsync(CancellationToken cancellationToken)
    {
        Configure();

        var accounts = await _expensesService
            .GetAllAccountsAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var account in accounts)
        {
            var accountData = new AccountData
            {
                RowId = account.RowId,
                Description = account.Description,
                Balance = account.Balance,
                Reserved = account.Reserved
            };

            await AddDataAsync(accountData, cancellationToken).ConfigureAwait(false);
        }

        return await GetContentAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override ICsvSerializer<AccountData> CreateSerializer(IEnumerable<AccountData>? configData = null)
    {
        var serializer = new CsvSerializer<AccountData>();

        serializer.AddField(nameof(AccountData.RowId), entity => entity.RowId);
        serializer.AddField(nameof(AccountData.Description), entity => entity.Description);
        serializer.AddField(nameof(AccountData.Balance), entity => entity.Balance);
        serializer.AddField(nameof(AccountData.Reserved), entity => entity.Reserved);

        return serializer;
    }
}
