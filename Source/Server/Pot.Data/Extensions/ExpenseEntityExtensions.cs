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

    public static int DaysFromAccrualStart(this ExpenseEntity expense, DateOnly currentDate)
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
        // Not using the expense frequency in case the accrual started earlier / later
        var accrualDays = Math.Max(expense.AccrualStart.DaysUntil(expense.NextDue), 1);
        return expense.Amount / accrualDays;
    }
}