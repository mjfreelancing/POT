using Pot.App.Features.Sites.Update.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Sites.Update.EntityChecks;

internal sealed class InputState
{
    public required Input Input { get; init; }
    public required SiteEntity SiteToUpdate { get; init; }
}
