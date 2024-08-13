namespace Pot.Features.Calculators.Allocation
{
    public sealed record AllocationCalculatorSettings
    {
        public bool AllowNegativeBalance { get; set; } = true;
    }
}
