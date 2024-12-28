namespace Pot.Data.Dtos
{
    public sealed record AccountDto : DtoBase
    {
        public string Bsb { get; init; } = string.Empty;
        public string Number { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public double Balance { get; init; }
        public double Reserved { get; init; }
        public double Allocated { get; init; }
        public double DailyAccrual { get; init; }

        // Calculated
        public double Available => Balance - Reserved - Allocated;
    }
}
