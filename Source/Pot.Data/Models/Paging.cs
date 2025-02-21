namespace Pot.Data.Models;

public sealed class Paging
{
    public int Limit { get; init; } = 100;
    public string? Continuation { get; init; }
}
