namespace Pot.Maui.Domain.Accounts.Models
{
    public sealed record Account
    {
        public long Id { get; set; }
        public string Bsb { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Balance { get; set; }
        public double Reserved { get; set; }
        public double Allocated { get; set; }
        public double DailyAccrual { get; set; }
        public double Available => Balance - Reserved - Allocated;
    }
}
