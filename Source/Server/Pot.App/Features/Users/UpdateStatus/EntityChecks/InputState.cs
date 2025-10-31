using Pot.App.Features.Users.UpdateStatus.Models;

namespace Pot.App.Features.Users.UpdateStatus.EntityChecks;

internal sealed class InputState
{
    public required Input Input { get; init; }
    public required long UserEtag { get; init; }
}
