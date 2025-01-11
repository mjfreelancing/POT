namespace Pot.AspNetCore.Features.Accounts.Import.Models
{
    public sealed class ImportResult
    {
        public int Skipped { get; init; }
        public int Imported { get; init; }
        public int Total { get; init; }
    }
}
