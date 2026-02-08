namespace Pot.App.Features.Settings.GetAll.Models;

public sealed record Output
{
    public required CategorySettings[] Categories { get; init; }
}
