namespace Pot.AspNetCore.Calculators.Allocation
{
    public sealed record AllocationCalculatorSettings
    {
        public bool AllowNegativeBalance { get; set; } = true;
    }
}
