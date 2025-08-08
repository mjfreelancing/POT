using AllOverIt.Assertion;
using AllOverIt.Csv;
using AllOverIt.Csv.Exporter;
using Pot.App.Features.Incomes.GetAll;
using Pot.App.Features.Maintenance.Export.Incomes.Models;
using Pot.Shared;

namespace Pot.App.Features.Maintenance.Export.Incomes;

internal sealed class IncomesExporter : MemoryCsvExporterBase<IncomeData>, IIncomesExporter
{
    private readonly IGetAllIncomesService _incomesService;

    public IncomesExporter(IGetAllIncomesService incomesService)
    {
        _incomesService = incomesService.WhenNotNull();
    }

    public async Task<byte[]> ExportAllAsync(CancellationToken cancellationToken)
    {
        Configure();

        var pageResult = await _incomesService.GetAllIncomesAsync(new Paging(), cancellationToken);
        var incomes = pageResult.Results;

        foreach (var income in incomes)
        {
            var incomeData = new IncomeData
            {
                RowId = income.RowId,
                ExcludeFromCalcs = income.ExcludeFromCalcs,
                Description = income.Description,
                NextDue = income.NextDue,
                EndDate = income.EndDate,
                Frequency = income.Frequency,
                FrequencyCount = income.FrequencyCount,
                Amount = income.Amount,
                Note = income.Note,
                AccountRowId = income.Account.RowId
            };

            await AddDataAsync(incomeData, cancellationToken);
        }

        return await GetContentAsync(cancellationToken);
    }

    protected override ICsvSerializer<IncomeData> CreateSerializer(IEnumerable<IncomeData>? configData = null)
    {
        var serializer = new CsvSerializer<IncomeData>();

        serializer.AddField(nameof(IncomeData.RowId), entity => entity.RowId);
        serializer.AddField(nameof(IncomeData.ExcludeFromCalcs), entity => entity.ExcludeFromCalcs);
        serializer.AddField(nameof(IncomeData.Description), entity => entity.Description);
        serializer.AddField(nameof(IncomeData.NextDue), entity => entity.NextDue.ToString(format: "yyyy-MM-dd"));
        serializer.AddField(nameof(IncomeData.EndDate), entity => entity.EndDate?.ToString(format: "yyyy-MM-dd"));
        serializer.AddField(nameof(IncomeData.Frequency), entity => entity.Frequency);
        serializer.AddField(nameof(IncomeData.FrequencyCount), entity => entity.FrequencyCount);
        serializer.AddField(nameof(IncomeData.Amount), entity => entity.Amount);
        serializer.AddField(nameof(IncomeData.Note), entity => entity.Note);
        serializer.AddField(nameof(IncomeData.AccountRowId), entity => entity.AccountRowId);

        return serializer;
    }
}
