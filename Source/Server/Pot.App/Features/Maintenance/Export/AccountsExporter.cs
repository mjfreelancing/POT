using AllOverIt.Assertion;
using AllOverIt.Csv;
using AllOverIt.Csv.Exporter;
using Pot.App.Features.Accounts.GetAll;
using Pot.App.Features.Maintenance.Export.Models;

namespace Pot.App.Features.Maintenance.Export;

internal sealed class AccountsExporter : MemoryCsvExporterBase<AccountData>, IAccountsExporter
{
    private readonly IGetAllAccountsService _expensesService;

    public AccountsExporter(IGetAllAccountsService expensesService)
    {
        _expensesService = expensesService.WhenNotNull();
    }

    public async Task<byte[]> ExportAllAsync(CancellationToken cancellationToken)
    {
        Configure();

        var accounts = await _expensesService.GetAllAccountsAsync(cancellationToken);

        foreach (var account in accounts)
        {
            var accountData = new AccountData
            {
                RowId = account.RowId,
                Bsb = account.Bsb,
                Number = account.Number,
                Description = account.Description,
                Balance = account.Balance,
                Reserved = account.Reserved,
                TotalExpenseAccrued = account.TotalExpenseAccrued,
                DailyExpenseAccrual = account.DailyExpenseAccrual
            };

            await AddDataAsync(accountData, cancellationToken);
        }

        return await GetContentAsync(cancellationToken);
    }

    protected override ICsvSerializer<AccountData> CreateSerializer(IEnumerable<AccountData>? configData = null)
    {
        var serializer = new CsvSerializer<AccountData>();

        serializer.AddField(nameof(AccountData.RowId), entity => entity.RowId);
        serializer.AddField(nameof(AccountData.Bsb), entity => entity.Bsb);
        serializer.AddField(nameof(AccountData.Number), entity => entity.Number);
        serializer.AddField(nameof(AccountData.Description), entity => entity.Description);
        serializer.AddField(nameof(AccountData.Balance), entity => entity.Balance);
        serializer.AddField(nameof(AccountData.Reserved), entity => entity.Reserved);
        serializer.AddField(nameof(AccountData.TotalExpenseAccrued), entity => entity.TotalExpenseAccrued);
        serializer.AddField(nameof(AccountData.DailyExpenseAccrual), entity => entity.DailyExpenseAccrual);

        return serializer;
    }
}
