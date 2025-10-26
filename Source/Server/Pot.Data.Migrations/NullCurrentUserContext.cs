using Pot.Shared;

namespace Pot.Data.Migrations;

internal sealed class NullCurrentUserContext : ICurrentUserContext
{
    public Guid UserRowId => Guid.Empty;

    public void SetUserRowId(Guid userRowId)
    {
    }
}
