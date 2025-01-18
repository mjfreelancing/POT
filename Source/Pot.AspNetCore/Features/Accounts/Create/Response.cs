namespace Pot.AspNetCore.Features.Accounts.Create;

internal partial class Handler
{
    internal sealed class Response
    {
        public int Id { get; init; }
        public long ETag { get; init; }
    }
}
