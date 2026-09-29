using Pot.Data.Entities;
using Pot.Shared.Models;

namespace Pot.Data.Repositories.Accounts.Dtos;

/// <summary>
/// An account together with the counts of its linked expenses and incomes and the accrual facts for its
/// non-excluded expenses.
/// </summary>
/// <remarks>
/// A narrow, EF-projectable read model. The account's expense and income graphs are counted and projected rather
/// than loaded, so the accounts read paths never materialise those collections.
/// </remarks>
public sealed class AccountWithLinkedCounts
{
    /// <summary>The account row.</summary>
    public required AccountEntity Account { get; init; }

    /// <summary>The total number of expenses linked to the account, including those excluded from calculations.</summary>
    public required int LinkedExpenses { get; init; }

    /// <summary>The total number of incomes linked to the account.</summary>
    public required int LinkedIncomes { get; init; }

    /// <summary>
    /// The accrual-relevant facts for the account's non-excluded expenses.
    /// </summary>
    /// <remarks>
    /// Projected rather than loaded, so the accounts read paths never materialise the expense graph. Excluded rows
    /// are filtered out in the query, which is the single place the exclusion decision is made.
    /// </remarks>
    public required IReadOnlyList<ExpenseAccrualInput> AccrualExpenses { get; init; }
}
