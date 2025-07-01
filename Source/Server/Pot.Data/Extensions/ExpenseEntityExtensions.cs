using Pot.Data.Entities;
using Pot.Shared.Extensions;

namespace Pot.Data.Extensions;

public static class ExpenseEntityExtensions
{
    public static double Balance(this ExpenseEntity expense)
    {
        return expense.Amount - expense.Accrued;
    }

    public static int DaysDue(this ExpenseEntity expense, DateOnly currentDate)
    {
        return Math.Max(0, currentDate.DaysUntil(expense.NextDue));
    }

    public static int DaysElapsed(this ExpenseEntity expense, DateOnly currentDate)
    {
        return expense.AccrualStart.DaysUntil(currentDate);
    }

    public static double DailyBalance(this ExpenseEntity expense, DateOnly currentDate)
    {
        var daysDue = expense.DaysDue(currentDate);
        return expense.Balance() / Math.Max(daysDue, 1);
    }

    public static double DailyAccrual(this ExpenseEntity expense)
    {
        return expense.Amount / Math.Max(expense.AccrualStart.DaysUntil(expense.NextDue), 1);
    }
}