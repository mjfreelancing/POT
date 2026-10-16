namespace Pot.App.Features.Settings.GetAll.Models;

public sealed record Output
{
    public required IReadOnlyList<CategorySettings> Categories { get; init; }
}
