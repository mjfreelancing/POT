using AllOverIt.Assertion;
using AllOverIt.Csv;
using AllOverIt.Csv.Exporter;
using Pot.App.Features.Expenses.GetAll;
using Pot.App.Features.Maintenance.Export.Models;

namespace Pot.App.Features.Maintenance.Export.Expenses;

internal sealed class ExpensesExporter : MemoryCsvExporterBase<ExpenseData>, IExpensesExporter
{
    private readonly IGetExpensesService _expensesService;

    public ExpensesExporter(IGetExpensesService expensesService)
    {
        _expensesService = expensesService.WhenNotNull();
    }

    public async Task<byte[]> ExportAllAsync(CancellationToken cancellationToken)
    {
        Configure();

        var expenses = await _expensesService.GetAllExpensesAsync(cancellationToken);

        foreach (var expense in expenses)
        {
            var expenseData = new ExpenseData
            {
                RowId = expense.RowId,
                ExcludeFromCalcs = expense.ExcludeFromCalcs,
                Description = expense.Description,
                AccrualStart = expense.AccrualStart,
                NextDue = expense.NextDue,
                EndDate = expense.EndDate,
                Frequency = expense.Frequency,
                FrequencyCount = expense.FrequencyCount,
                Amount = expense.Amount,
                Accrued = expense.Accrued,
                LastAccruedUpdate = expense.LastAccruedUpdate,
                AccruedIsDirty = expense.AccruedIsDirty,
                Note = expense.Note,
                AccountRowId = expense.Account.RowId
            };

            await AddDataAsync(expenseData, cancellationToken);
        }

        return await GetContentAsync(cancellationToken);
    }

    protected override ICsvSerializer<ExpenseData> CreateSerializer(IEnumerable<ExpenseData>? configData = null)
    {
        var serializer = new CsvSerializer<ExpenseData>();

        serializer.AddField(nameof(ExpenseData.RowId), entity => entity.RowId);
        serializer.AddField(nameof(ExpenseData.ExcludeFromCalcs), entity => entity.ExcludeFromCalcs);
        serializer.AddField(nameof(ExpenseData.Description), entity => entity.Description);
        serializer.AddField(nameof(ExpenseData.AccrualStart), entity => entity.AccrualStart.ToString(format: "yyyy-MM-dd"));
        serializer.AddField(nameof(ExpenseData.NextDue), entity => entity.NextDue.ToString(format: "yyyy-MM-dd"));
        serializer.AddField(nameof(ExpenseData.EndDate), entity => entity.EndDate?.ToString(format: "yyyy-MM-dd"));
        serializer.AddField(nameof(ExpenseData.Frequency), entity => entity.Frequency);
        serializer.AddField(nameof(ExpenseData.FrequencyCount), entity => entity.FrequencyCount);
        serializer.AddField(nameof(ExpenseData.Amount), entity => entity.Amount);
        serializer.AddField(nameof(ExpenseData.Accrued), entity => entity.Accrued);
        serializer.AddField(nameof(ExpenseData.AccruedIsDirty), entity => entity.AccruedIsDirty);
        serializer.AddField(nameof(ExpenseData.LastAccruedUpdate), entity => entity.LastAccruedUpdate?.ToString(format: "yyyy-MM-dd"));
        serializer.AddField(nameof(ExpenseData.Note), entity => entity.Note);
        serializer.AddField(nameof(ExpenseData.AccountRowId), entity => entity.AccountRowId);

        return serializer;
    }
}
