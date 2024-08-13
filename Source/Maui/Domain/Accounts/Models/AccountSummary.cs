namespace Pot.Maui.Domain.Accounts.Models
{
    public sealed class AccountSummary
    {
        public double Balance { get; set; }
        public double Reserved { get; set; }
        public double Allocated { get; set; }
        public double Available { get; set; }
        public double DailyAccrual { get; set; }
    }
}
