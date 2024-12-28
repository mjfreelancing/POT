using Pot.Data.Models;

namespace Pot.Data.Dtos
{
    public sealed record ExpenseDto : DtoBase
    {
        public int AccountId { get; init; }

        public string Description { get; init; } = string.Empty;
        public DateTime NextDue { get; init; }
        public DateTime AccrualStart { get; init; }
        public ExpenseFrequency Frequency { get; init; } = ExpenseFrequency.Months;
        public int FrequencyCount { get; init; }
        public bool Recurring { get; init; }
        public double Amount { get; init; }
        public double Allocated { get; init; }

        // Calculated
        public int DaysDue { get; }
        public int DaysElapsed { get; }
        public double Balance => Amount - Allocated;
        public double BalanceDaily { get; }
        public double AccrualDaily => Amount / Math.Max((NextDue - AccrualStart).Days, 1);

        public ExpenseDto(TimeProvider? timeProvider = null)
        {
            var currentDate = (timeProvider ?? TimeProvider.System).GetLocalNow().DateTime;

            DaysDue = Math.Max(0, (NextDue.Date - currentDate).Days);
            DaysElapsed = (currentDate - AccrualStart).Days;
            BalanceDaily = Balance / Math.Max(DaysDue, 1);
        }
    }
}