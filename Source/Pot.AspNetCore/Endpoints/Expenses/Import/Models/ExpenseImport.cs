﻿using CsvHelper.Configuration.Attributes;
using Pot.AspNetCore.Endpoints.Expenses.Import.Converters;
using Pot.Data.Models;

namespace Pot.AspNetCore.Endpoints.Expenses.Import.Models
{
    public sealed class ExpenseImport
    {
        [Index(0)]
        public int Id { get; init; }

        [Index(1)]
        public int AccountId { get; init; }

        [Index(2)]
        public string Description { get; init; } = string.Empty;

        [Index(3)]
        public DateTime NextDue { get; init; }

        [Index(4)]
        public DateTime AccrualStart { get; init; }

        [Index(5)]
        [TypeConverter(typeof(ExpenseFrequencyConverter))]
        public ExpenseFrequency Frequency { get; init; } = ExpenseFrequency.Months;

        [Index(6)]
        public int FrequencyCount { get; init; }

        [Index(7)]
        public bool Recurring { get; init; }

        [Index(8)]
        public double Amount { get; init; }

        [Index(9)]
        public double Allocated { get; init; }
    }
}
