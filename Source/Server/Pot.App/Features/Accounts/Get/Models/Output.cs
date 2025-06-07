namespace Pot.App.Features.Accounts.Get.Models;

public sealed class Output
{
    public required Guid RowId { get; init; }
    public required long Etag { get; init; }
    public required string Bsb { get; init; }
    public required string Number { get; init; }
    public required string Description { get; init; }
    public required double Balance { get; init; }
    public required double Reserved { get; init; }
    public required double Allocated { get; init; }
    public required double DailyAccrual { get; init; }
    public double Available => Balance - Reserved - Allocated;

}
