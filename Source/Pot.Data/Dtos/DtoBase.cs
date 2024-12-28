namespace Pot.Data.Dtos
{
    public abstract record DtoBase
    {
        public int Id { get; init; }
        public long ETag { get; init; }
    }
}
