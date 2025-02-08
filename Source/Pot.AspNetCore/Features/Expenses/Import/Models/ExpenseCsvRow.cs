using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using Pot.AspNetCore.Features.Expenses.Import.Converters;
using Pot.Data.Models;

namespace Pot.AspNetCore.Features.Expenses.Import.Models;

public sealed class ExpenseCsvRow
{
    [Index(0)]
    public Guid? AccountId { get; init; }

    [Index(1)]
    public Guid? ExpenseId { get; init; }

    [Index(2)]
    public required string Description { get; init; }

    [Index(3)]
    [Format("yyyy-MM-dd")]
    [TypeConverter(typeof(DateOnlyConverter))]
    public DateOnly NextDue { get; init; }

    [Index(4)]
    [Format("yyyy-MM-dd")]
    [TypeConverter(typeof(DateOnlyConverter))]
    public DateOnly AccrualStart { get; init; }

    [Index(5)]
    [TypeConverter(typeof(ExpenseFrequencyConverter))]
    public required ExpenseFrequency Frequency { get; init; }

    [Index(6)]
    public int FrequencyCount { get; init; }

    [Index(7)]
    public bool Recurring { get; init; }

    [Index(8)]
    public double Amount { get; init; }

    [Index(9)]
    public double Allocated { get; init; }
}
