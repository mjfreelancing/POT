using Pot.Maui.Domain.Accounts.Models;
using Pot.Maui.Domain.Expenses.Models;

namespace Pot.Maui.Features.Calculators.Allocation
{
    public interface IAllocationCalculator
    {
        void AllocateFunds(Account[] accounts, Expense[] expenses, AllocationCalculatorSettings calculatorSettings);
    }
}
