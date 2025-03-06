using Pot.Data.Models;
using Pot.Shared.Extensions;

namespace Pot.Data.Dtos
{
    public sealed class ExpenseDto : DtoBase
    {
        public int AccountId { get; init; }

        public string Description { get; init; } = string.Empty;
        public DateOnly NextDue { get; init; }
        public DateOnly AccrualStart { get; init; }
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
        public double AccrualDaily => Amount / Math.Max(NextDue.DaysFrom(AccrualStart), 1);

        public ExpenseDto(TimeProvider? timeProvider = null)
        {
            var currentDate = DateOnly.FromDateTime((timeProvider ?? TimeProvider.System).GetLocalNow().Date);

            DaysDue = Math.Max(0, NextDue.DaysFrom(currentDate));
            DaysElapsed = currentDate.DaysFrom(AccrualStart);
            BalanceDaily = Balance / Math.Max(DaysDue, 1);
        }
    }
}