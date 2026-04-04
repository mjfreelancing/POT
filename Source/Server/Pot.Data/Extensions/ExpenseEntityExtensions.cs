using Pot.Data.Entities;
using Pot.Shared.Extensions;

namespace Pot.Data.Extensions;

public static class ExpenseEntityExtensions
{
    public static double Balance(this ExpenseEntity expense)
    {
        return expense.Amount - expense.Accrued;
    }

    public static int DaysDueFrom(this ExpenseEntity expense, DateOnly currentDate)
    {
        return Math.Max(0, currentDate.DaysUntil(expense.NextDue));
    }

    public static int DaysFromAccrualStart(this ExpenseEntity expense, DateOnly currentDate)
    {
        return expense.AccrualStart.HasValue
            ? expense.AccrualStart.Value.DaysUntil(currentDate)
            : 0;
    }

    public static double DailyBalance(this ExpenseEntity expense, DateOnly currentDate)
    {
        var daysDue = expense.DaysDueFrom(currentDate);
        return expense.Balance() / Math.Max(daysDue, 1);
    }

    public static double DailyAccrual(this ExpenseEntity expense)
    {
        if (!expense.AccrualStart.HasValue)
        {
            return 0.0d;
        }

        // Not using the expense frequency in case the accrual started earlier / later
        var accrualDays = Math.Max(expense.AccrualStart.Value.DaysUntil(expense.NextDue), 1);
        return expense.Amount / accrualDays;
    }
}