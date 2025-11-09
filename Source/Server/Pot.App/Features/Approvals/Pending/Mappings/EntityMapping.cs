using Pot.App.Features.Approvals.Pending.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Approvals.Pending.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this UserEntity user)
    {
        return new Output
        {
            RowId = user.RowId,
            Etag = user.Etag,
            Username = user.Username,
            Email = user.Email
        };
    }
}