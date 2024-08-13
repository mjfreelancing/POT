namespace Pot.Maui.Domain.Expenses.Models
{
    public sealed class ExpenseSummary
    {
        public double Total { get; set; }
        public double Allocated { get; set; }
        public double AccrualDaily { get; set; }
        public double BalanceDaily { get; set; }
    }
}
