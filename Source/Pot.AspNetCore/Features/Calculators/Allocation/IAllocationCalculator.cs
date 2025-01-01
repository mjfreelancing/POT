namespace Pot.AspNetCore.Features.Calculators.Allocation
{
    public interface IAllocationCalculator
    {
        // TODO: Update this so the calculations are applied against the entities
        void AllocateFunds(/*AccountDto[] accounts, ExpenseDto[] expenses,*/ AllocationCalculatorSettings calculatorSettings);
    }
}
