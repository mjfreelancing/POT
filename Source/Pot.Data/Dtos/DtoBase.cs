namespace Pot.Data.Dtos
{
    public abstract class DtoBase
    {
        public int Id { get; init; }
        public long ETag { get; init; }
    }
}
