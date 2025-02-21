using Pot.Data.Models;

namespace Pot.AspNetCore.Models;

public class PagedRequest
{
    public Paging Paging { get; init; } = new();
}
