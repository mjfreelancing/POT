using System.Text;

namespace Pot.RazorComponents.Models;

/// <summary>
/// The content of the reminder sent to a user listing the incomes and expenses due within their reminder period.
/// </summary>
public sealed class EmailBudgetReminderInfo : EmailConfigBase
{
    /// <summary>An income due within the reminder period.</summary>
    public sealed class IncomeInfo
    {
        /// <summary>Gets the description of the income.</summary>
        public required string Description { get; init; }

        /// <summary>Gets the amount of the income.</summary>
        public required double Amount { get; init; }

        /// <summary>Gets the date the income is next due.</summary>
        public required DateOnly NextDue { get; init; }

        /// <summary>Gets the number of days until the income is due. This is negative when the income is overdue.</summary>
        public required int DaysDue { get; init; }

        /// <summary>Gets the note recorded against the income, if any.</summary>
        public required string? Note { get; init; }
    }

    /// <summary>An expense due within the reminder period.</summary>
    public sealed class ExpenseInfo
    {
        /// <summary>Gets the description of the expense.</summary>
        public required string Description { get; init; }

        /// <summary>Gets the amount of the expense.</summary>
        public required double Amount { get; init; }

        /// <summary>Gets the date the expense is next due.</summary>
        public required DateOnly NextDue { get; init; }

        /// <summary>Gets the number of days until the expense is due. This is negative when the expense is overdue.</summary>
        public required int DaysDue { get; init; }

        /// <summary>Gets the note recorded against the expense, if any.</summary>
        public required string? Note { get; init; }
    }

    /// <summary>Gets the local date and time the reminder was generated.</summary>
    [EmailFormat(EmailFormatType.None)]
    public required DateTime GeneratedDateTime { get; init; }

    /// <summary>Gets the symbol shown before each amount.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required string CurrencySymbol { get; init; }

    /// <summary>Gets the number of days ahead the reminder covers.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required int ReminderDays { get; init; }

    /// <summary>Gets the display name of the recipient.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required string DisplayName { get; init; }

    /// <summary>Gets the local date and time the user last signed in.</summary>
    [EmailFormat(EmailFormatType.None)]
    public required DateTime LastLoggedIn { get; init; }

    /// <summary>Gets the incomes due within the reminder period.</summary>
    [EmailFormat(EmailFormatType.Html)]
    public required IReadOnlyList<IncomeInfo> UserIncomes { get; init; }

    /// <summary>Gets the expenses due within the reminder period.</summary>
    [EmailFormat(EmailFormatType.Html)]
    public required IReadOnlyList<ExpenseInfo> UserExpenses { get; init; }

    /// <inheritdoc />
    [EmailFormat(EmailFormatType.None)]
    public override string RecipientName => DisplayName;

    /// <summary>Gets the formatted local date and time the reminder was generated.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public string GeneratedLabel => FormatDateTime(GeneratedDateTime);

    /// <summary>Gets the formatted local date and time the user last signed in.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public string LastLoggedInLabel => FormatDateTime(LastLoggedIn);

    /// <summary>Gets the incomes due within the reminder period, as plain text.</summary>
    [EmailFormat(EmailFormatType.PlainText)]
    public string IncomesSection => BuildIncomesSection(UserIncomes, ReminderDays, CurrencySymbol);

    /// <summary>Gets the expenses due within the reminder period, as plain text.</summary>
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
