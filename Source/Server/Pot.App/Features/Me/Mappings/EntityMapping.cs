using Pot.App.Features.Me.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Me.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this UserEntity user)
    {
        var site = user.Site;

        return new Output
        {
            RowId = user.RowId,
            Etag = user.Etag,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Site = new Output.SiteModel
            {
                RowId = site.RowId,
                Etag = site.Etag,
                Name = site.Name,
                Description = site.Description
            }
        };
    }
}