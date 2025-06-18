namespace Pot.AspNetCore.Calculators.Models
{
    public sealed class AccountSummary
    {
        public double Balance { get; set; }
        public double Reserved { get; set; }
        public double DailyExpenseAccrual { get; set; }
        public double TotalExpenseAccrued { get; set; }
        public double Available { get; set; }
    }
}
