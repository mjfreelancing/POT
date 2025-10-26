using AllOverIt.Assertion;
using Pot.Shared;
using System.Diagnostics;

namespace Pot.Data;

internal sealed class CurrentUserContext : ICurrentUserContext
{
    private Guid? _userRowId = null;

    public Guid UserRowId => GetUserRowId();

    public void SetUserRowId(Guid userRowId)
    {
        _userRowId = userRowId;
    }

    private Guid GetUserRowId()
    {
        Throw<UnreachableException>.WhenNot(_userRowId.HasValue, "The user identifier has not been set");

        return _userRowId.Value;
    }
}
