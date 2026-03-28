using System.Text;

namespace Pot.RazorComponents.Models;

public sealed class EmailBudgetReminderInfo : EmailConfigBase
{
    public sealed class IncomeInfo
    {
        public required string Description { get; init; }
        public required double Amount { get; init; }
        public required DateOnly NextDue { get; init; }
        public required int DaysDue { get; init; }
        public required string? Note { get; init; }
    }

    public sealed class ExpenseInfo
    {
        public required string Description { get; init; }
        public required double Amount { get; init; }
        public required DateOnly NextDue { get; init; }
        public required int DaysDue { get; init; }
        public required string? Note { get; init; }
    }

    [EmailFormat(EmailFormatType.Both)]
    public required DateTime GeneratedDateTime { get; init; }

    [EmailFormat(EmailFormatType.Both)]
    public required string CurrencySymbol { get; init; }

    [EmailFormat(EmailFormatType.Both)]
    public required int ReminderDays { get; init; }

    [EmailFormat(EmailFormatType.Both)]
    public required string DisplayName { get; init; }

    [EmailFormat(EmailFormatType.Both)]
    public required DateTime LastLoggedInUtc { get; init; }

    [EmailFormat(EmailFormatType.Html)]
    public required IReadOnlyList<IncomeInfo> UserIncomes { get; init; }

    [EmailFormat(EmailFormatType.Html)]
    public required IReadOnlyList<ExpenseInfo> UserExpenses { get; init; }

    [EmailFormat(EmailFormatType.PlainText)]
    public string IncomesSection => BuildIncomesSection(UserIncomes, ReminderDays, CurrencySymbol);

    [EmailFormat(EmailFormatType.PlainText)]
    public string ExpensesSection => BuildExpensesSection(UserExpenses, ReminderDays, CurrencySymbol);

    private static string BuildIncomesSection(IReadOnlyList<IncomeInfo> incomes, int reminderDays, string currencySymbol)
    {
        var section = new StringBuilder();

        if (incomes.Count == 0)
        {
            section.AppendLine("=== INCOMES ===");
            section.AppendLine($"No upcoming incomes in the next {reminderDays} days.");
        }
        else
        {
            section.AppendLine($"=== INCOMES ({incomes.Count}) ===");
            section.AppendLine();

            for (var i = 0; i < incomes.Count; i++)
            {
                var income = incomes[i];
                var statusText = FormatDueStatus(income.DaysDue);

                section.AppendLine($"{i + 1}. {income.Description}");
                section.AppendLine($"   Amount: {currencySymbol}{income.Amount:N2}");
                section.AppendLine($"   Due: {income.NextDue:ddd, dd MMM yyyy} ({statusText})");

                section.AppendLine();
            }
        }

        return section.ToString();
    }

    private static string BuildExpensesSection(IReadOnlyList<ExpenseInfo> expenses, int reminderDays, string currencySymbol)
    {
        var section = new StringBuilder();

        if (expenses.Count == 0)
        {
            section.AppendLine("=== EXPENSES ===");
            section.AppendLine($"No upcoming expenses in the next {reminderDays} days.");
        }
        else
        {
            section.AppendLine($"=== EXPENSES ({expenses.Count}) ===");
            section.AppendLine();

            for (var i = 0; i < expenses.Count; i++)
            {
                var expense = expenses[i];
                var statusText = FormatDueStatus(expense.DaysDue);

                section.AppendLine($"{i + 1}. {expense.Description}");
                section.AppendLine($"   Amount: {currencySymbol}{expense.Amount:N2}");
                section.AppendLine($"   Due: {expense.NextDue:ddd, dd MMM yyyy} ({statusText})");

                section.AppendLine();
            }
        }

        return section.ToString();
    }

    private static string FormatDueStatus(int daysDue)
    {
        return daysDue switch
        {
            < 0 => $"{Math.Abs(daysDue)} {(Math.Abs(daysDue) == 1 ? "day" : "days")} overdue",
            0 => "due today",
            _ => $"in {daysDue} {(daysDue == 1 ? "day" : "days")}"
        };
    }
}
