using Pot.Data.Entities;

namespace Pot.Data.Repositories.Accounts.Dtos;

public sealed class AccountWithLinkedCounts
{
    public required AccountEntity Account { get; init; }
    public required int LinkedExpenses { get; init; }
    public required int LinkedIncomes { get; init; }
}
