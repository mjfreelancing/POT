namespace Pot.AspNetCore.Features.Calculators.Summary
{
    //    public sealed class AccountDto : DtoBase
    //    {
    //        public string Bsb { get; init; } = string.Empty;
    //        public string Number { get; init; } = string.Empty;
    //        public string Description { get; init; } = string.Empty;
    //        public double Balance { get; init; }
    //        public double Reserved { get; init; }
    //        public double Allocated { get; init; }
    //        public double DailyAccrual { get; init; }

    //        // Calculated
    //        public double Available => Balance - Reserved - Allocated;
    //    }


    //public sealed class ExpenseDto : DtoBase
    //{
    //    public int AccountId { get; init; }

    //    public string Description { get; init; } = string.Empty;
    //    public DateOnly NextDue { get; init; }
    //    public DateOnly AccrualStart { get; init; }
    //    public ExpenseFrequency Frequency { get; init; } = ExpenseFrequency.Months;
    //    public int FrequencyCount { get; init; }
    //    public bool Recurring { get; init; }
    //    public double Amount { get; init; }
    //    public double Allocated { get; init; }

    //    // Calculated
    //    public int DaysDue { get; }
    //    public int DaysElapsed { get; }
    //    public double Balance => Amount - Allocated;
    //    public double BalanceDaily { get; }
    //    public double AccrualDaily => Amount / Math.Max(NextDue.DaysFrom(AccrualStart), 1);

    //    public ExpenseDto(TimeProvider? timeProvider = null)
    //    {
    //        var currentDate = DateOnly.FromDateTime((timeProvider ?? TimeProvider.System).GetLocalNow().Date);

    //        DaysDue = Math.Max(0, NextDue.DaysFrom(currentDate));
    //        DaysElapsed = currentDate.DaysFrom(AccrualStart);
    //        BalanceDaily = Balance / Math.Max(DaysDue, 1);
    //    }
    //}


    public static class SummaryCalculator
    {
        //public static AccountSummary GetAccountSummary(AccountDto[] accounts)
        //{
        //    var summary = new AccountSummary();

        //    foreach (var account in accounts)
        //    {
        //        summary.Balance += account.Balance;
        //        summary.Reserved += account.Reserved;
        //        summary.Allocated += account.Allocated;
        //        summary.Available += account.Available;
        //        summary.DailyAccrual += account.DailyAccrual;
        //    }

        //    return summary;
        //}

        //public static ExpenseSummary GetExpenseSummary(ExpenseDto[] expenses)
        //{
        //    var summary = new ExpenseSummary();

        //    foreach (var expense in expenses)
        //    {
        //        summary.Total += expense.Amount;
        //        summary.Allocated += expense.Allocated;
        //        summary.AccrualDaily += expense.AccrualDaily;
        //        summary.BalanceDaily += expense.BalanceDaily;
        //    }

        //    return summary;
        //}
    }
}
