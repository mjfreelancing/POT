namespace Pot.Shared.Extensions;

public static class DateOnlyExtensions
{
    public static int DaysUntil(this DateOnly startDate, DateOnly endDate)
    {
        return endDate.DayNumber - startDate.DayNumber;
    }
}
