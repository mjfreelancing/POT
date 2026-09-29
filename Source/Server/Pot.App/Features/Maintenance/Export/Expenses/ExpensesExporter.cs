using AllOverIt.Assertion;
using AllOverIt.Csv;
using AllOverIt.Csv.Exporter;
using Pot.App.Features.Expenses.GetAll;
using Pot.App.Features.Maintenance.Export.Models;

namespace Pot.App.Features.Maintenance.Export.Expenses;

/// <summary>
/// Default implementation of <see cref="IExpensesExporter"/>.
/// </summary>
internal sealed class ExpensesExporter : MemoryCsvExporterBase<ExpenseData>, IExpensesExporter
{
    private readonly IGetExpensesService _expensesService;

    public ExpensesExporter(IGetExpensesService expensesService)
    {
        _expensesService = expensesService.WhenNotNull();
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportAllAsync(CancellationToken cancellationToken)
    {
        Configure();

        var expenses = await _expensesService
            .GetAllExpensesAsync(cancellationToken)
            .ConfigureAwait(false);

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
                AccrualPolicy = expense.AccrualPolicy,
                Frequency = expense.Frequency,
                FrequencyCount = expense.FrequencyCount,
                Amount = expense.Amount,
                Note = expense.Note,
                AccountRowId = expense.Account.RowId
            };

            await AddDataAsync(expenseData, cancellationToken).ConfigureAwait(false);
        }

        return await GetContentAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override ICsvSerializer<ExpenseData> CreateSerializer(IEnumerable<ExpenseData>? configData = null)
    {
        var serializer = new CsvSerializer<ExpenseData>();

        serializer.AddField(nameof(ExpenseData.RowId), entity => entity.RowId);
        serializer.AddField(nameof(ExpenseData.ExcludeFromCalcs), entity => entity.ExcludeFromCalcs);
        serializer.AddField(nameof(ExpenseData.Description), entity => entity.Description);
        serializer.AddField(nameof(ExpenseData.AccrualStart), entity => entity.AccrualStart?.ToString(format: "yyyy-MM-dd"));
        serializer.AddField(nameof(ExpenseData.NextDue), entity => entity.NextDue.ToString(format: "yyyy-MM-dd"));
        serializer.AddField(nameof(ExpenseData.EndDate), entity => entity.EndDate?.ToString(format: "yyyy-MM-dd"));
        serializer.AddField(nameof(ExpenseData.AccrualPolicy), entity => entity.AccrualPolicy);
        serializer.AddField(nameof(ExpenseData.Frequency), entity => entity.Frequency);
        serializer.AddField(nameof(ExpenseData.FrequencyCount), entity => entity.FrequencyCount);
        serializer.AddField(nameof(ExpenseData.Amount), entity => entity.Amount);
        serializer.AddField(nameof(ExpenseData.Note), entity => entity.Note);
        serializer.AddField(nameof(ExpenseData.AccountRowId), entity => entity.AccountRowId);

        return serializer;
    }
}
