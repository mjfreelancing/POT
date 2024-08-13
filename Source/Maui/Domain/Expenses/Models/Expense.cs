using Pot.Maui.Domain.Models;

namespace Pot.Maui.Domain.Expenses.Models
{
    public sealed record Expense
    {
        internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;

        private DateTime CurrentDate => TimeProvider.GetLocalNow().DateTime;

        public long Id { get; set; }
        public long AccountId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime NextDue { get; set; }
        public DateTime AccrualStart { get; set; }
        public ExpenseFrequency Frequency { get; set; }
        public int FrequencyCount { get; set; }
        public bool Recurring { get; set; }
        public double Amount { get; set; }
        public double Allocated { get; set; }
        public int DaysDue => Math.Max(0, (NextDue.Date - CurrentDate).Days);
        public int DaysElapsed => (CurrentDate - AccrualStart).Days;
        public double Balance => Amount - Allocated;
        public double BalanceDaily => Balance / Math.Max(DaysDue, 1);
        public double AccrualDaily => Amount / Math.Max((NextDue - AccrualStart).Days, 1);
    }
}
