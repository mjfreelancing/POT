namespace Pot.AspNetCore.Calculators.Models
{
    public sealed class ExpenseSummary
    {
        public double Total { get; set; }
        public double Accrued { get; set; }
        public double DailyAccrual { get; set; }
        public double DailyBalance { get; set; }
    }
}
