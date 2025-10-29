using Pot.App.Features.Users.Update.Models;

namespace Pot.App.Features.Users.Update.EntityChecks;

internal sealed class InputState
{
    public required Input Input { get; init; }
    public required long UserEtag { get; init; }
}
